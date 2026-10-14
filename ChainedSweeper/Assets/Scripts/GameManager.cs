using System.Collections.Generic;
using Challenges;
using DevTools;
using Input;
using Map;
using Protag;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField]
    private GameUI _gameUI;

    [SerializeField]
    private ChallengePatternRoster _patternRoster;

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

    private float LastReloadTime => _gameState.LastReloadTime;
    private float TimeLeft => _gameState.TimeLeft;
    private float ComboTimeLeft => _gameState.ComboTimerRemaining;
    private int Score => _gameState.Score;

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

        if (index < 0f || index >= _gameState.Setup.SweepNodes.Count)
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
                RefreshComboTimer(_setup.ComboTimerOnNodeSwept);
                AddCombo();
                _gameState.Score++;
                if (didFinish)
                {
                    _onFinishChallenge?.Invoke();
                    _gameState.Challenges.Remove(challenge);
                    _gameState.TimeSinceChallengeDone = Time.time;

                    SwitchPhase(GamePhase.WaitingForChallenge);
                    foreach (SweepNode node in challenge.InvolvedNodes)
                    {
                        node.ChallengeEnded();
                    }

                    _gameState.UseBroomCharge();
                    RefreshComboTimer(_setup.ComboTimerOnCompleteChallenge);
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

        if (Time.time - LastReloadTime < 0.25f)
        {
            return;
        }

        if (_gameState.TryAddBroomCharge())
        {
            _gameState.LastReloadTime = Time.time;
            RefreshComboTimer(_setup.ComboTimerOnReload);
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
        _gameUI.UpdateUI(_gameState);

        if (!_gameState.Phase.IsGameplayPhase())
        {
            return;
        }

        _gameState.TimeLeft = Mathf.Max(0f, _gameState.TimeLeft - Time.deltaTime);

        if (TimeLeft <= 0f)
        {
            SwitchPhase(GamePhase.ScoreScreen);
            return;
        }

        _gameState.ComboTimerRemaining -= Time.deltaTime;

        if (_gameState.ComboTimerRemaining <= 0f)
        {
            _gameState.ComboTimerRemaining = 0f;
            _gameState.CurrentCombo = 0;
        }

        if (_gameState.Phase is GamePhase.WaitingForChallenge &&
            Time.time - _gameState.TimeSinceChallengeDone >= _setup.DelayBetweenChallenge)
        {
            ChallengeRosterEntry pattern = _patternRoster.GetPattern();
            var parity = 1;
            if (!pattern.BlockSymmetry)
            {
                parity = Random.Range(0, 2) == 0 ? -1 : 1;
            }

            Debug.Log("Starting new challenge");
            // Create a new challenge
            List<ChallengeStep> challengeNodes = new();
            SweepNode nextNode = _gameState.GetRandomSweepNode(out int nodeIndex);
            challengeNodes.Add(ChallengeStep.CreateStep(nextNode, nextNode));

            foreach (ChallengePatternStep step in pattern.PatternSO.Steps)
            {
                nodeIndex += step.StepDirection * parity;
                SweepNode connectTo = _gameState.GetNodeLoopedIndex(nodeIndex, out nodeIndex);

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
                break;
        }

        switch (newPhase)
        {
            case GamePhase.ScoreScreen:
                _onGameEnd?.Invoke();
                break;
            case GamePhase.StartingGame:
                SwitchPhase(GamePhase.WaitingForChallenge);
                _stateDebug = _gameState;
                _gameState.ResetState(_setup);
                _onGameStart?.Invoke();
                break;
        }
    }

    private void AddCombo()
    {
        _gameState.CurrentCombo++;
        if (_gameState.CurrentCombo > _gameState.ComboHigh)
        {
            _gameState.ComboHigh = _gameState.CurrentCombo;
        }
    }

    private void RefreshComboTimer(float newTimeRemaining)
    {
        _gameState.ComboTimerRemaining = Mathf.Max(_gameState.ComboTimerRemaining, newTimeRemaining);
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < _setup.SweepNodes.Count; i++)
        {
            LabelUtils.Label(_setup.SweepNodes[i].Pos + Vector3.up, $"{i}", Color.red);
        }
    }
}