using Assets.Effects.StatusEffects;
using Assets.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Turns
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public int CurrentTurn { get; private set; } = 1;

        private List<ITarget> targets = new();

        public event EventHandler OnPlayerTurnStart;

        public event EventHandler OnPlayerTurnEnd;

        public event EventHandler OnEnemyTurnStart;

        public event EventHandler OnEnemyTurnEnd;

        //TODO: Rewrite it currently it is only for testing so tours are chaning
        // one after another without waiting for effects to end or enemies to perform actions

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

        private void Start()
        {
            StartPlayerTurn();
        }

        public void StartPlayerTurn()
        {
            OnPlayerTurnStart?.Invoke(this, EventArgs.Empty);
        }

        public void EndPlayerTurn()
        {
            OnPlayerTurnEnd?.Invoke(this, EventArgs.Empty);

            StartEnemyTurn();
        }

        public void StartEnemyTurn()
        {
            StartCoroutine(StartEnemyTurnWithWait());
        }

        private IEnumerator StartEnemyTurnWithWait()
        {
            yield return new WaitForSeconds(0.5f);

            foreach (IStatusEffectReceiver target in targets)
            {
                target.ProcessStatusEffectsOnTurnStart();
            }

            OnEnemyTurnEnd?.Invoke(this, EventArgs.Empty);

            EndEnemyTurn();
        }

        public void EndEnemyTurn()
        {
            StartCoroutine(EndEnemyTurnWithWait());
        }

        private IEnumerator EndEnemyTurnWithWait()
        {
            yield return new WaitForSeconds(0.5f);

            foreach (IStatusEffectReceiver target in targets)
            {
                target.ProcessStatusEffectsOnTurnEnd();
            }

            CurrentTurn++;

            OnEnemyTurnEnd?.Invoke(this, EventArgs.Empty);

            StartPlayerTurn();
        }

        public void RegisterTarget(ITarget target)
        {
            if (!targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        public void UnregisterTarget(ITarget target)
        {
            targets.Remove(target);
        }
    }
}
