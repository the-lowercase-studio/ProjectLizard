using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Enemies.Base.Intentions
{
    public class IntentionSelector
    {
        public IntentionConfig SelectIntention(List<IntentionConfig> intentions)
        {
            if (intentions == null || intentions.Count == 0)
            {
                Debug.LogWarning("No intentions available to select from!");
                return null;
            }

            int totalWeight = intentions.Sum(i => i.Probability);

            if (totalWeight == 0)
            {
                Debug.LogWarning("All intention probabilities are 0!");
                return null;
            }

            int randomValue = Random.Range(0, totalWeight);

            int cumulativeWeight = 0;
            foreach (var intention in intentions)
            {
                cumulativeWeight += intention.Probability;
                if (randomValue < cumulativeWeight)
                {
                    return intention;
                }
            }

            return intentions[0];
        }
    }
}
