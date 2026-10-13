using System.Collections.Generic;
using DevTools;
using Input;
using Map;
using Protag;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Utils;

/// <summary>
///     Top level game script
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ProtagController _protag;

    [SerializeField]
    private BroomClient _inputClient;

    [SerializeField]
    private ParticleSystem _isLoadedParticles;

    [SerializeField]
    private GameSetup _setup;

    [Header("Game UI")]

    [SerializeField]
    private TMP_Text _timerText;

    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private CanvasGroup _gameplayScoreGroup;

    [Header("Score Screen UI")]

    [SerializeField]
    private GameObject _timeText;

    [SerializeField]
    private TMP_Text _scoreScreenScoreText;

    [SerializeField]
    private CinemachineCamera _scoreScreenCamera;

    [Header("Debug")]

    [SerializeField]
    private GameState _stateDebug;

    [Header("Events")]

    [SerializeField]
    private UnityEvent _onReload;

    [SerializeField]
    private UnityEvent _onSweepNode;

    [SerializeField]
    private UnityEvent _onFinishChallenge;

    [SerializeField]
    private UnityEvent _onGameStart;

    [SerializeField]
    private UnityEvent _onGameEnd;

    private GameState _gameState;

    private float _lastReloadTime;
    private float _timeLeft;
    private int _score;

    private void Start()
    {
        Initialize();
        SwitchPhase(GamePhase.StartingGame);
    }

    private void OnDestroy()
    {
        EndGame();
    }

    private void Initialize()
    {
        _gameState = new GameState(_setup);
        _gameState.Phase = GamePhase.None;

        _protag.Initialize(_inputClient);
        _inputClient.Initialize();

        _inputClient.OnReload += HandleReload;
        _inputClient.OnHitSweepNode += HandleHitSweepNode;
    }

    private void HandleHitSweepNode(int index)
    {
        if (!_gameState.Phase.IsGameplayPhase())
        {
            return;
        }

        if (!_gameState.TryGetCurrentChallenge(out Challenge challenge))
        {
            return;
        }

        if (_gameState.PlayerState.BroomCharges == 0)
        {
            return;
        }

        SweepNode nodeSwept = _gameState.Setup.SweepNodes[index];

        if (challenge.TryGetNextStep(out ChallengeStep step))
        {
            if (step.To == nodeSwept)
            {
                challenge.CompleteNextStep(out bool didFinish);
                _onSweepNode?.Invoke();
                if (didFinish)
                {
                    _onFinishChallenge?.Invoke();
                    _gameState.Challenges.Remove(challenge);
                    SwitchPhase(GamePhase.WaitingForChallenge);
                    foreach (SweepNode node in _gameState.Setup.SweepNodes)
                    {
                        node.ChallengeEnded();
                    }

                    _score++;
                    _gameState.UseBroomCharge();
                }
            }
        }
    }

    private void HandleReload()
    {
        if (_gameState.Phase is GamePhase.ScoreScreen)
        {
            SwitchPhase(GamePhase.StartingGame);
        }

        if (!_gameState.Phase.IsGameplayPhase())
        {
            return;
        }

        if (Time.time - _lastReloadTime < 0.25f)
        {
            return;
        }

        if (_gameState.TryAddBroomCharge())
        {
            _lastReloadTime = Time.time;
            _onReload?.Invoke();
        }
    }

    private void EndGame()
    {
        _inputClient.Cleanup();
        _inputClient.OnReload -= HandleReload;
        _inputClient.OnHitSweepNode -= HandleHitSweepNode;
        _protag.Cleanup();
    }

    private void Update()
    {
        if (!_gameState.Phase.IsGameplayPhase())
        {
            return;
        }

        _timeLeft = Mathf.Max(0f, _timeLeft - Time.deltaTime);
        _timerText.text = $"{_timeLeft:f0}s";
        _scoreText.text = _score.ToString();

        if (_timeLeft <= 0f)
        {
            SwitchPhase(GamePhase.ScoreScreen);
            return;
        }

        if (_gameState.Phase is GamePhase.WaitingForChallenge)
        {
            Debug.Log("Starting new challenge");
            // Create a new challenge
            List<ChallengeStep> challengeNodes = new();
            SweepNode nextNode = _gameState.GetRandomSweepNode();
            challengeNodes.Add(ChallengeStep.CreateStep(nextNode, nextNode));

            for (var i = 0; i < 2; i++)
            {
                SweepNode connectTo = nextNode.GetRandomAdjacentNode();

                challengeNodes.Add(ChallengeStep.CreateStep(nextNode, connectTo));
                nextNode = connectTo;
            }

            _gameState.Challenges.Add(new Challenge(challengeNodes));
            SwitchPhase(GamePhase.ChallengeRunning);
        }

        foreach (SweepNode node in _gameState.Setup.SweepNodes)
        {
            node.Tick(_gameState);
        }

        if (_gameState.PlayerState.BroomCharges > 0)
        {
            _isLoadedParticles.Play();
        }
        else
        {
            _isLoadedParticles.Stop();
        }
    }

    private void SwitchPhase(GamePhase newPhase)
    {
        if (_gameState.Phase == newPhase)
        {
            return;
        }

        GamePhase oldPhase = _gameState.Phase;
        _gameState.Phase = newPhase;

        switch (oldPhase)
        {
            case GamePhase.ScoreScreen:
                _gameplayScoreGroup.SetVisible(true);
                _scoreScreenCamera.enabled = false;
                _timeText.SetActive(false);
                break;
        }

        switch (newPhase)
        {
            case GamePhase.ScoreScreen:
                _gameplayScoreGroup.SetVisible(false);
                _scoreScreenCamera.enabled = true;
                _timeText.SetActive(true);
                _scoreScreenScoreText.text = _score.ToString();
                _onGameEnd?.Invoke();
                break;
            case GamePhase.StartingGame:
                SwitchPhase(GamePhase.WaitingForChallenge);
                _stateDebug = _gameState;
                _timeLeft = _setup.Time;
                _score = 0;
                _onGameStart?.Invoke();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < _setup.SweepNodes.Count; i++)
        {
            LabelUtils.Label(_setup.SweepNodes[i].Pos + Vector3.up, $"{i}", Color.red);
        }
    }
}