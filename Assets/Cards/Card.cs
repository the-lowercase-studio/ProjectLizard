using Assets.Interfaces;
using UnityEngine;

namespace Assets.Cards
{
    public class Card : MonoBehaviour, IInitializableByConfig<CardBaseSO>
    {
        [field: SerializeField] public CardBaseSO Config { get; private set; }

        public void Initialize(CardBaseSO config)
        {
            if (Config == null)
            {
                Config = config;
            }
        }
    }
}
