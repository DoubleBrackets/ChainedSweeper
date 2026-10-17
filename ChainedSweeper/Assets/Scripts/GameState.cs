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
    public Challenge ActiveChallenge;

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

    public SweepNode GetRandomSweepNode(out int index)
    {
        if (Setup.SweepNodes.Count == 0)
        {
            index = -1;
            return null;
        }

        index = Random.Range(0, Setup.SweepNodes.Count);
        return Setup.SweepNodes[index];
    }

    public bool TryGetCurrentChallenge(out Challenge challenge)
    {
        if (ActiveChallenge != null)
        {
            challenge = ActiveChallenge;
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

    public SweepNode GetNodeLoopedIndex(int nodeIndex, out int loopedIndex)
    {
        loopedIndex = nodeIndex;
        int count = Setup.SweepNodes.Count;
        if (loopedIndex < 0)
        {
            loopedIndex = count + loopedIndex;
        }
        else if (loopedIndex >= count)
        {
            loopedIndex %= count;
        }

        return Setup.SweepNodes[loopedIndex];
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
public class PatternStep
{
    public SweepNode From;
    public SweepNode To;

    public bool Completed;

    public static PatternStep CreateStep(SweepNode from, SweepNode to)
    {
        return new PatternStep
        {
            From = from,
            To = to,
            Completed = false
        };
    }
}

[Serializable]
public class PatternInstance
{
    public List<PatternStep> ChallengeSteps;
    public HashSet<SweepNode> InvolvedNodes;
    public int StepIndex;

    public bool Finished => StepIndex >= ChallengeSteps.Count;

    public PatternInstance(List<PatternStep> steps)
    {
        InvolvedNodes = new HashSet<SweepNode>();
        ChallengeSteps = steps;

        foreach (PatternStep step in ChallengeSteps)
        {
            InvolvedNodes.Add(step.From);
            InvolvedNodes.Add(step.To);
        }
    }

    public bool TryGetNextStep(out PatternStep patternStep)
    {
        for (var i = 0; i < ChallengeSteps.Count; i++)
        {
            PatternStep step = ChallengeSteps[i];
            if (i == StepIndex)
            {
                patternStep = step;
                return true;
            }
        }

        patternStep = null;
        return false;
    }

    public List<PatternStep> StepsThatGoTo(SweepNode sweepNode)
    {
        return ChallengeSteps.Where(a => a.To == sweepNode).ToList();
    }

    public void CompleteNextStep(out bool didFinish)
    {
        for (var i = 0; i < ChallengeSteps.Count; i++)
        {
            PatternStep step = ChallengeSteps[i];
            if (i == StepIndex)
            {
                step.Completed = true;
                if (i < ChallengeSteps.Count - 1)
                {
                    StepIndex++;
                    didFinish = false;
                    return;
                }

                StepIndex++;
                didFinish = true;
                return;
            }
        }

        Debug.LogWarning("Tried to complete next step when there was none!");
        didFinish = false;
    }

    public bool IsAfterNext(PatternStep step)
    {
        return ChallengeSteps.IndexOf(step) == StepIndex + 1;
    }

    public bool IsNext(PatternStep step)
    {
        return ChallengeSteps.IndexOf(step) == StepIndex;
    }
}

[Serializable]
public class Challenge
{
    public Challenge(List<List<PatternStep>> patterns)
    {
        Patterns = new List<PatternInstance>();
        foreach (List<PatternStep> pattern in patterns)
        {
            Patterns.Add(new PatternInstance(pattern));
        }
    }

    public List<PatternInstance> Patterns;

    public bool IsFinished()
    {
        foreach (PatternInstance pattern in Patterns)
        {
            if (!pattern.Finished)
            {
                return false;
            }
        }

        return true;
    }
}

[Serializable]
public class PlayerState
{
    public int BroomCharges;
}