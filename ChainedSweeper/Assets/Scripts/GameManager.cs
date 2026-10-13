using System.Collections.Generic;
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

    private GameState _gameState;
    private bool _initialized;

    private float _lastReloadTime;

    private void Start()
    {
        InitializeGame();
    }

    private void OnDestroy()
    {
        EndGame();
    }

    private void InitializeGame()
    {
        _gameState = new GameState(_setup);
        _stateDebug = _gameState;
        _protag.Initialize(_inputClient);
        _inputClient.Initialize();
        _initialized = true;

        _inputClient.OnReload += HandleReload;
        _inputClient.OnHitSweepNode += HandleHitSweepNode;
    }

    private void HandleHitSweepNode(int index)
    {
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
                    _gameState.Phase = GamePhase.WaitingForChallenge;
                    foreach (SweepNode node in _gameState.Setup.SweepNodes)
                    {
                        node.ChallengeEnded();
                    }

                    _gameState.UseBroomCharge();
                }
            }
        }
    }

    private void HandleReload()
    {
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

        _initialized = false;
        _protag.Cleanup();
    }

    private void Update()
    {
        if (!_initialized)
        {
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
            _gameState.Phase = GamePhase.ChallengeRunning;
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
}