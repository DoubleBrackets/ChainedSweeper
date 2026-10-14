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
    public float TimeSinceChallengeDone;
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
        TimeSinceChallengeDone = 0;
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
    public float DelayBetweenChallenge;
}

[Serializable]
public class ChallengeStep
{
    public SweepNode From;
    public SweepNode To;

    public bool Completed;

    public static ChallengeStep CreateStep(SweepNode from, SweepNode to)
    {
        return new ChallengeStep
        {
            From = from,
            To = to,
            Completed = false
        };
    }
}

[Serializable]
public class Challenge
{
    public Challenge(List<ChallengeStep> steps)
    {
        InvolvedNodes = new HashSet<SweepNode>();
        ChallengeSteps = steps;

        foreach (ChallengeStep step in ChallengeSteps)
        {
            InvolvedNodes.Add(step.From);
            InvolvedNodes.Add(step.To);
        }
    }

    public List<ChallengeStep> ChallengeSteps;
    public HashSet<SweepNode> InvolvedNodes;
    public int StepIndex;

    public bool TryGetNextStep(out ChallengeStep challengeStep)
    {
        for (var i = 0; i < ChallengeSteps.Count; i++)
        {
            ChallengeStep step = ChallengeSteps[i];
            if (i == StepIndex)
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
            if (i == StepIndex)
            {
                step.Completed = true;
                if (i < ChallengeSteps.Count - 1)
                {
                    StepIndex++;
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

    public bool IsAfterNext(ChallengeStep step)
    {
        return ChallengeSteps.IndexOf(step) == StepIndex + 1;
    }

    public bool IsNext(ChallengeStep step)
    {
        return ChallengeSteps.IndexOf(step) == StepIndex;
    }
}

[Serializable]
public class PlayerState
{
    public int BroomCharges;
}