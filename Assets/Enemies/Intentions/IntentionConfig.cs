using System;
using UnityEngine;

namespace Assets.Enemies.Intentions
{
    [Serializable]
    public class IntentionConfig
    {
        [SerializeField] private IntentionType _intentionType;
        [SerializeField][Range(0, 100)] private int _probability = 33;
        [SerializeReference] private IEnemyAction _action;

        public IntentionType IntentionType => _intentionType;
        public int Probability => _probability;
        public IEnemyAction Action => _action;

        public IntentionConfig(IntentionType intentionType, int probability, IEnemyAction action)
        {
            _intentionType = intentionType;
            _probability = probability;
            _action = action;
        }
    }
}
