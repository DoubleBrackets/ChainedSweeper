using System;
using System.Collections.Generic;
using UnityEngine;

namespace Challenges
{
    [Serializable]
    public struct ChallengePatternStep
    {
        public int StepDirection;
    }

    [CreateAssetMenu(fileName = "ChallengePatternSO", menuName = "Pattern")]
    public class ChallengePatternSO : ScriptableObject
    {
        [TextArea]
        public string DevNote;

        [field: SerializeField]
        public List<ChallengePatternStep> Steps { get; private set; }
    }
}