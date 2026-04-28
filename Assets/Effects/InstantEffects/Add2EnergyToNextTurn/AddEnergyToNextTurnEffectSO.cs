using Assets.Effects.Base;
using UnityEngine;

namespace Assets.Effects.InstantEffects.AddEnergyToNextTurn
{
    [CreateAssetMenu(fileName = "New Add Energy To Next Turn", menuName = "Scriptable Objects/Effects/Custom/Add Energy To Next Turn")]
    public class AddEnergyToNextTurnEffectSO : EffectSO
    {
        [field: Header("Add Energy To Next Turn Settings")]
        [field: SerializeField, Tooltip("The amount of energy to add to the next turn.")]
        public int EnergyToAdd { get; private set; } = 2;

        public override void Execute(CardEffectContext context)
        {
            if (context.EnergyManager == null)
            {
                Debug.LogWarning("AddEnergyToNextTurnEffect: Missing energy manager in effect context.");
                return;
            }

            context.EnergyManager.AddBonusEnergyForNextTurn(EnergyToAdd);
            Debug.Log($"AddEnergyToNextTurnEffect: Registered {EnergyToAdd} bonus energy for the next turn.");
        }
    }
}
