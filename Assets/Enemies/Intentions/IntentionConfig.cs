using System;
using UnityEngine;

namespace Assets.Enemies.Intentions
{
    [Serializable]
    public class IntentionConfig
    {
        [SerializeField] private IntentionType _intentionType;
        [SerializeField][Range(0, 100)] private int _probability = 33;
        [SerializeReference] private EnemyActionBase _action;

        public IntentionType IntentionType => _intentionType;
        public int Probability => _probability;
        public EnemyActionBase Action => _action;

        public IntentionConfig(IntentionType intentionType, int probability, EnemyActionBase action)
        {
            _intentionType = intentionType;
            _probability = probability;
            _action = action;
        }
    }
}
