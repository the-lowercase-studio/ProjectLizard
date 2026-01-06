using Assets.Effects.StatusEffects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Turns
{
    public class TurnManager : MonoBehaviour
    {
        public TurnManager Instance { get; private set; }
        public int CurrentTurn { get; private set; } = 1;

        private List<IStatusEffectReceiver> targets = new();

        private TurnManager()
        {
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartTurn()
        {
            foreach (IStatusEffectReceiver target in targets)
            {
                target.ProcessStatusEffectsOnTurnStart();
            }
        }

        public void EndTurn()
        {
            foreach (IStatusEffectReceiver target in targets)
            {
                target.ProcessStatusEffectsOnTurnEnd();
            }

            CurrentTurn++;
        }

        public void RegisterTarget(IStatusEffectReceiver target)
        {
            if (!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        public void UnregisterTarget(IStatusEffectReceiver target)
        {
            targets.Remove(target);
        }
    }
}
