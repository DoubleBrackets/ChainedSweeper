using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Challenges
{
    [Serializable]
    public struct ChallengeRosterEntry
    {
        public ChallengeSO ChallengeSO;
        public int Weight;
    }

    [CreateAssetMenu(fileName = "ChallengePatternRoster", menuName = "PatternRoster")]
    public class ChallengePatternRoster : ScriptableObject
    {
        [field: SerializeField]
        public List<ChallengeRosterEntry> Rosters { get; private set; }

        public ChallengeRosterEntry GetChallenge()
        {
            var totalWeight = 0;
            foreach (ChallengeRosterEntry entry in Rosters)
            {
                totalWeight += entry.Weight;
            }

            int rand = Random.Range(0, totalWeight);

            var accum = 0;
            foreach (ChallengeRosterEntry entry in Rosters)
            {
                accum += entry.Weight;
                if (accum > rand)
                {
                    return entry;
                }
            }

            return default;
        }
    }
}