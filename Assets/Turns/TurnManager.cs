using Assets.Effects.StatusEffects;
using Assets.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Turns
{
    public interface ITurnManager
    {
        int CurrentTurn { get; }

        event EventHandler OnPlayerTurnStart;

        event EventHandler OnPlayerTurnEnd;

        event EventHandler OnEnemyTurnStart;

        event EventHandler OnEnemyTurnEnd;

        void StartPlayerTurn();

        void EndPlayerTurn();

        void StartEnemyTurn();

        void EndEnemyTurn();

        void RegisterTarget(ITarget target);

        void UnregisterTarget(ITarget target);
    }

    public class TurnManager : MonoBehaviour, ITurnManager
    {
        public int CurrentTurn { get; private set; } = 1;

        public event EventHandler OnPlayerTurnStart;
        public event EventHandler OnPlayerTurnEnd;
        public event EventHandler OnEnemyTurnStart;
        public event EventHandler OnEnemyTurnEnd;

        private List<ITarget> targets = new();

        //TODO: Rewrite it currently it is only for testing so tours are chaning
        // one after another without waiting for effects to end or enemies to perform actions

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

            OnEnemyTurnStart?.Invoke(this, EventArgs.Empty);

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
