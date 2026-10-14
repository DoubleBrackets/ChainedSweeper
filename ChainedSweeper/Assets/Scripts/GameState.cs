using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameState
{
    public GameState(GameSetup setup)
    {
        Setup = setup;
        Phase = GamePhase.WaitingForChallenge;
    }

    public PlayerState PlayerState = new();
    public GamePhase Phase;
    public GameSetup Setup;
    public List<Challenge> Challenges = new();

    public float LastReloadTime;
    public float ComboTimerRemaining;
    public float TimeLeft;
    public int Score;
    public int CurrentCombo;
    public int ComboHigh;

    public void UseBroomCharge()
    {
        PlayerState.BroomCharges--;
    }

    public bool TryAddBroomCharge()
    {
        if (PlayerState.BroomCharges < Setup.MaxBroomCharges)
        {
            PlayerState.BroomCharges++;
            return true;
        }

        return false;
    }

    public SweepNode GetRandomSweepNode()
    {
        if (Setup.SweepNodes.Count == 0)
        {
            return null;
        }

        return Setup.SweepNodes[Random.Range(0, Setup.SweepNodes.Count)];
    }

    public bool TryGetCurrentChallenge(out Challenge challenge)
    {
        if (Challenges.Count > 0)
        {
            challenge = Challenges[0];
            return true;
        }

        challenge = null;
        return false;
    }

    public void ResetState(GameSetup setup)
    {
        TimeLeft = setup.Time;
        Score = 0;
        CurrentCombo = 0;
        ComboHigh = 0;
        ComboTimerRemaining = setup.InitialComboTimer;
    }
}

public enum GamePhase
{
    None,
    StartingGame,
    WaitingForChallenge,
    ChallengeRunning,
    ScoreScreen
}

public static class GamePhaseExtensions
{
    public static bool IsGameplayPhase(this GamePhase phase)
    {
        return phase is GamePhase.WaitingForChallenge or GamePhase.ChallengeRunning;
    }
}

[Serializable]
public class GameSetup
{
    public int MaxBroomCharges;
    public List<SweepNode> SweepNodes;
    public float Time;
    public float ComboTimerOnNodeSwept;
    public float ComboTimerOnReload;
    public float InitialComboTimer;
    public float ComboTimerOnCompleteChallenge;
}

[Serializable]
public class ChallengeStep
{
    public SweepNode From;
    public SweepNode To;

    public bool IsNext;
    public bool Completed;

    public static ChallengeStep CreateStep(SweepNode from, SweepNode to)
    {
        return new ChallengeStep
        {
            From = from,
            To = to,
            Completed = false,
            IsNext = false
        };
    }
}

[Serializable]
public class Challenge
{
    public Challenge(List<ChallengeStep> steps)
    {
        ChallengeSteps = steps;
        ChallengeSteps[0].IsNext = true;
    }

    public List<ChallengeStep> ChallengeSteps;

    public bool TryGetNextStep(out ChallengeStep challengeStep)
    {
        foreach (ChallengeStep step in ChallengeSteps)
        {
            if (step.IsNext)
            {
                challengeStep = step;
                return true;
            }
        }

        challengeStep = null;
        return false;
    }

    public List<ChallengeStep> StepsThatGoTo(SweepNode sweepNode)
    {
        return ChallengeSteps.Where(a => a.To == sweepNode).ToList();
    }

    public void CompleteNextStep(out bool didFinish)
    {
        for (var i = 0; i < ChallengeSteps.Count; i++)
        {
            ChallengeStep step = ChallengeSteps[i];
            if (step.IsNext)
            {
                step.Completed = true;
                step.IsNext = false;
                if (i < ChallengeSteps.Count - 1)
                {
                    ChallengeSteps[i + 1].IsNext = true;
                    didFinish = false;
                    return;
                }

                didFinish = true;
                return;
            }
        }

        Debug.LogWarning("Tried to complete next step when there was none!");
        didFinish = false;
    }
}

[Serializable]
public class PlayerState
{
    public int BroomCharges;
}