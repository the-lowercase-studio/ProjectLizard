using Assets.Effects.StatusEffects;
using UnityEngine;

namespace Assets.Effects.UI
{
    public class EffectDisplayTester : MonoBehaviour
    {
        [SerializeField] private StatusEffectReceiver _targetReceiver;
        [SerializeField] private int _testBurnTurns = 3;
        [SerializeField] private int _testBurnDamage = 5;
        [SerializeField] private int _testStunTurns = 2;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                ApplyTestBurning();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ApplyTestStun();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllEffects();
            }
        }

        public void ApplyTestBurning()
        {
            BurningStatusEffect burning = new BurningStatusEffect((byte)_testBurnTurns, _testBurnDamage);
            _targetReceiver.ApplyStatusEffect(burning);
            Debug.Log("Applied test burning effect");
        }

        public void ApplyTestStun()
        {
            StunStatusEffect stun = new StunStatusEffect((byte)_testStunTurns);
            _targetReceiver.ApplyStatusEffect(stun);
            Debug.Log("Applied test stun effect");
        }

        public void ClearAllEffects()
        {
            var effects = _targetReceiver.GetActiveEffects();
            foreach (var effect in effects)
            {
                effect.Remove();
            }
            Debug.Log("Cleared all effects");
        }
    }
}
