using System;
using System.Collections.Generic;
using UnityEngine;

namespace Challenges
{
    [Serializable]
    public struct Pattern
    {
        public List<int> StepIndexes;
    }

    [Serializable]
    public struct Challenge
    {
        public bool AllowSymmetry;
        public bool AllowTranslating;
        public List<Pattern> Patterns;
    }

    [CreateAssetMenu(fileName = "ChallengePatternSO", menuName = "Pattern")]
    public class ChallengeSO : ScriptableObject
    {
        [TextArea]
        public string DevNote;

        [field: SerializeField]
        public Challenge Challenge { get; private set; }
    }
}