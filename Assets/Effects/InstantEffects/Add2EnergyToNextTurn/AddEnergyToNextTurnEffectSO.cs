using Assets.Effects.Base;
using Assets.Energy;
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
            // Try to find the EnergyManager in the scene
            EnergyManager energyManager = FindAnyObjectByType<EnergyManager>();
            if (energyManager != null)
            {
                energyManager.AddBonusEnergyForNextTurn(EnergyToAdd);
                Debug.Log($"AddEnergyToNextTurnEffect: Registered {EnergyToAdd} bonus energy for the next turn. Next turn's energy will be capped at {EnergyManager.MAX_ENERGY_PER_TURN}.");
            }
            else
            {
                Debug.LogWarning("AddEnergyToNextTurnEffect: Could not find EnergyManager in the scene.");
            }
        }
    }
}
