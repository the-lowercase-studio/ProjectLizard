# Energy System Documentation

## Purpose

The Energy system owns turn-based player energy state and its combat UI presentation.

Primary responsibilities:

- Store and expose current energy and per-turn energy values.
- Refill current energy at player turn start.
- Validate and apply energy spend/gain operations.
- Store one-shot bonus energy that should be applied on the next player turn.
- Publish change events for energy UI updates.

The system is not responsible for:

- Turn sequencing policy (owned by TurnManager).
- Card cost calculation (owned by card/config systems).
- Card attack, effect chance, and discard flow after successful payment (owned by Cards system).
- Deciding whether a card is playable in hand presentation logic (consumers may use energy state, but policy remains outside EnergyManager).

## Reading Map

- Primary code locations:
  - Assets/Energy/EnergyManager.cs
  - Assets/Energy/EnergyPresenter.cs
- Related integration points:
  - Assets/Installers/SceneInstaller.cs
  - Assets/Turns/TurnManager.cs
  - Assets/Cards/Base/Usage/CardUsage.cs
  - Assets/Effects/InstantEffects/Add2EnergyToNextTurn/AddEnergyToNextTurnEffectSO.cs
- Related docs:
  - agent-docs/AGENTS.md
  - agent-docs/TECHNOLOGY_DOCUMENTATION.md
  - agent-docs/PROJECT_CODING_STANDARDS.md
  - agent-docs/CARDS_SYSTEM_SUMMARY.md
  - agent-docs/EFFECTS_SYSTEM_SUMMARY.md
  - agent-docs/implementation-summaries/effects/AddEnergyToNextTurnEffectSO.md
- Related agents or instructions:
  - .agents/skills/document-system/SKILL.md
  - .agents/skills/create-effect/SKILL.md
  - .agents/agents/card-system-specialist.agent.md
  - .agents/instructions/cards.instructions.md

## Architecture and Data Flow

- Core components:
  - IEnergyManager: runtime contract for querying and mutating energy values.
  - EnergyManager: MonoBehaviour implementation holding current energy, per-turn energy, next-turn bonus energy, and publishing events.
  - EnergyPresenter: UI adapter that renders current/per-turn energy text and toggles visibility by turn phase.
- Key interfaces:
  - IEnergyManager, ITurnManager.
- Runtime flow:
  1. SceneInstaller registers EnergyManager as IEnergyManager for DI consumers.
  2. TurnManager raises OnPlayerTurnStart.
  3. EnergyManager handles OnPlayerTurnStart and refills CurrentEnergy to EnergyPerTurn through RefilCurrentEnergy.
  4. EnergyManager publishes OnCurrentEnergyChange.
  5. If any next-turn bonus energy is buffered, EnergyManager adds it to CurrentEnergy, clamps the result to MAX_ENERGY_PER_TURN, clears the buffer, and publishes OnCurrentEnergyChange again.
  6. EnergyPresenter receives energy events and updates TextMeshPro text in x/y format.
  7. CardUsage checks card cost against IEnergyManager.CurrentEnergy; on success, it spends energy via DecreaseCurrentEnergy before executing animation, damage, effects, and discard.

### Card and Effect Flow

- CardUsage owns the payment gate:
  - It reads the current card cost from Card.GetCurrentEnergyCost.
  - It only continues when cost <= IEnergyManager.CurrentEnergy.
  - It deducts the cost before attack animation, attack-step damage/effects, and discard.
- AddEnergyToNextTurnEffectSO is the current effect integration:
  - It locates EnergyManager with FindAnyObjectByType instead of DI.
  - It calls AddBonusEnergyForNextTurn(EnergyToAdd).
  - The bonus is not visible immediately; it is applied during the next OnPlayerTurnStart handler.
  - Multiple effect executions before the next player turn stack in the bonus buffer.

### Value Limits and Defaults

- Start energy per turn: 3.
- Max energy per turn: 9.
- Current energy starts at 3 and is refilled to EnergyPerTurn each player turn start.
- Next-turn bonus energy can temporarily raise CurrentEnergy above EnergyPerTurn, but never above Max energy per turn.

## Rules and Invariants

- Critical behavior rules:
  - Non-positive mutation requests are ignored.
  - Current energy can only decrease if the result would stay >= 0.
  - Current energy can only increase if the result would stay <= EnergyPerTurn.
  - Energy per turn can only decrease down to the start floor (3).
  - Energy per turn can only increase up to the max cap (9).
  - AddBonusEnergyForNextTurn only accepts positive amounts and accumulates them until the next player turn start.
  - Next-turn bonus application uses MAX_ENERGY_PER_TURN as the current-energy cap, not EnergyPerTurn.
- Ordering or sequencing guarantees:
  - Refill is event-driven and currently tied to ITurnManager.OnPlayerTurnStart.
  - Base refill happens before next-turn bonus energy is applied.
  - If a bonus is applied, OnCurrentEnergyChange is emitted once for the base refill and once for the bonus-applied total.
  - OnCurrentEnergyChange is emitted after successful current-energy write.
  - OnEnergyPerTurnChange is emitted after successful per-turn-energy write.
  - AddBonusEnergyForNextTurn does not emit an event when the bonus is registered.
- Constraints contributors must preserve:
  - Preserve DI binding through IEnergyManager rather than direct singleton/global access.
  - Preserve turn-start refill contract unless intentionally redesigning game pacing.
  - Preserve event publication on state change for UI synchronization.
  - Preserve CardUsage as the energy payment gate unless a new card-use pipeline explicitly replaces it.
  - Keep any future bonus-energy effects aligned with the next-turn buffer semantics or document a new timing model.

## Extension Points

- Safe extension areas:
  - Add temporary modifiers/buffs by calling IncreaseEnergyPerTurn and DecreaseEnergyPerTurn from effect systems.
  - Add one-shot next-turn bonuses by calling AddBonusEnergyForNextTurn.
  - Add alternative presenters (for example segmented bar or animated counter) by subscribing to IEnergyManager events.
  - Add card preview/playability indicators by consuming IEnergyManager.CurrentEnergy.
- Required dependencies and contracts:
  - Consumers must use IEnergyManager from DI (SceneInstaller binding).
  - Any turn-phase-dependent behavior should use ITurnManager events.
  - Presenter implementations should stay resilient to object enable/disable lifecycle transitions.
- Testing implications:
  - Verify all mutation boundaries (0 floor, 3 floor, 9 cap).
  - Verify refill on each player turn start.
  - Verify next-turn bonus energy stacks, applies once, clears after application, and clamps at 9.
  - Verify card usage spends only when cost <= current energy.
  - Verify UI text updates after both current and per-turn value changes.
  - Verify bonus-current-energy states above EnergyPerTurn still allow valid card payments and eventually normalize on the next refill.

## Integration Notes

- Upstream dependencies:
  - ITurnManager event stream for refill and presenter visibility state.
  - Reflex scene bindings from SceneInstaller.
  - AddEnergyToNextTurnEffectSO for current next-turn bonus energy registration.
- Downstream consumers:
  - CardUsage gate/spend flow.
  - EnergyPresenter (and any future energy UI).
- Cross-system coupling risks:
  - Turn event naming/timing changes in TurnManager directly affect refill timing.
  - Any card pipeline change that bypasses CardUsage may skip energy spend.
  - Effects that find EnergyManager directly bypass the established IEnergyManager DI boundary.
  - Current-energy bonus states can exceed EnergyPerTurn, so consumers must not assume CurrentEnergy <= EnergyPerTurn during a bonus turn.
  - Presenter text parsing assumes fixed x/y format in a single TMP text field.

## Known Risks and Open Questions

- Known limitations:
  - Method name RefilCurrentEnergy is misspelled and exposed in IEnergyManager API.
  - EnergyPresenter subscribes to turn-manager events in Start but does not unsubscribe from those turn events in OnDisable.
  - Current energy is not automatically clamped when EnergyPerTurn is decreased; temporarily CurrentEnergy can exceed EnergyPerTurn until next refill.
  - Next-turn bonus energy intentionally allows CurrentEnergy to exceed EnergyPerTurn until spent or until a later refill normalizes it.
  - AddEnergyToNextTurnEffectSO uses FindAnyObjectByType<EnergyManager>, which bypasses the DI/interface pattern used by most runtime consumers.
  - Presenter text updates depend on string split behavior, which is brittle if the display format changes.
- Open design questions:
  - Should EnergyPerTurn changes immediately clamp CurrentEnergy to maintain CurrentEnergy <= EnergyPerTurn at all times?
  - Should refill timing remain strictly OnPlayerTurnStart, or become configurable by mechanics/effects?
  - Should energy UI use structured fields (current and max text separately) instead of parsing combined text?
  - Should next-turn energy bonuses be represented through IEnergyManager-only access so instant effects do not need scene lookup?
  - Should bonus-energy turns display EnergyPerTurn or MAX_ENERGY_PER_TURN as the denominator when CurrentEnergy exceeds EnergyPerTurn?
- Suggested follow-up tasks:
  - Rename RefilCurrentEnergy to RefillCurrentEnergy with a compatibility migration.
  - Add missing turn-event unsubscription in EnergyPresenter lifecycle.
  - Decide and document clamp policy when per-turn energy is reduced below current energy.
  - Migrate AddEnergyToNextTurnEffectSO toward an injected or context-provided IEnergyManager dependency.
  - Add focused tests or validation checklist for energy boundary behavior and turn-start refill.
