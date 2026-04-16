# Energy System Documentation

## Purpose

The Energy system owns turn-based player energy state and its combat UI presentation.

Primary responsibilities:

- Store and expose current energy and per-turn energy values.
- Refill current energy at player turn start.
- Validate and apply energy spend/gain operations.
- Publish change events for energy UI updates.

The system is not responsible for:

- Turn sequencing policy (owned by TurnManager).
- Card cost calculation (owned by card/config systems).
- Deciding whether a card is playable in hand presentation logic (consumers may use energy state, but policy remains outside EnergyManager).

## Reading Map

- Primary code locations:
  - Assets/Energy/EnergyManager.cs
  - Assets/Energy/EnergyPresenter.cs
- Related integration points:
  - Assets/Installers/SceneInstaller.cs
  - Assets/Turns/TurnManager.cs
  - Assets/Cards/Base/Usage/CardUsage.cs
- Related docs:
  - agent-docs/AGENTS.md
  - agent-docs/TECHNOLOGY_DOCUMENTATION.md
  - agent-docs/PROJECT_CODING_STANDARDS.md
  - agent-docs/CARDS_SYSTEM_SUMMARY.md

## Architecture and Data Flow

- Core components:
  - IEnergyManager: runtime contract for querying and mutating energy values.
  - EnergyManager: MonoBehaviour implementation holding state and publishing events.
  - EnergyPresenter: UI adapter that renders current/per-turn energy text and toggles visibility by turn phase.
- Key interfaces:
  - IEnergyManager, ITurnManager.
- Runtime flow:
  1. SceneInstaller registers EnergyManager as IEnergyManager for DI consumers.
  2. TurnManager raises OnPlayerTurnStart.
  3. EnergyManager handles OnPlayerTurnStart and refills CurrentEnergy to EnergyPerTurn.
  4. EnergyManager publishes OnCurrentEnergyChange.
  5. EnergyPresenter receives energy events and updates TextMeshPro text in x/y format.
  6. CardUsage checks card cost against IEnergyManager.CurrentEnergy; on success, it spends energy via DecreaseCurrentEnergy before executing damage/effects.

### Value Limits and Defaults

- Start energy per turn: 3.
- Max energy per turn: 9.
- Current energy starts at 3 and is refilled to EnergyPerTurn each player turn start.

## Rules and Invariants

- Critical behavior rules:
  - Non-positive mutation requests are ignored.
  - Current energy can only decrease if the result would stay >= 0.
  - Current energy can only increase if the result would stay <= EnergyPerTurn.
  - Energy per turn can only decrease down to the start floor (3).
  - Energy per turn can only increase up to the max cap (9).
- Ordering or sequencing guarantees:
  - Refill is event-driven and currently tied to ITurnManager.OnPlayerTurnStart.
  - OnCurrentEnergyChange is emitted after successful current-energy write.
  - OnEnergyPerTurnChange is emitted after successful per-turn-energy write.
- Constraints contributors must preserve:
  - Preserve DI binding through IEnergyManager rather than direct singleton/global access.
  - Preserve turn-start refill contract unless intentionally redesigning game pacing.
  - Preserve event publication on state change for UI synchronization.

## Extension Points

- Safe extension areas:
  - Add temporary modifiers/buffs by calling IncreaseEnergyPerTurn and DecreaseEnergyPerTurn from effect systems.
  - Add alternative presenters (for example segmented bar or animated counter) by subscribing to IEnergyManager events.
  - Add card preview/playability indicators by consuming IEnergyManager.CurrentEnergy.
- Required dependencies and contracts:
  - Consumers must use IEnergyManager from DI (SceneInstaller binding).
  - Any turn-phase-dependent behavior should use ITurnManager events.
  - Presenter implementations should stay resilient to object enable/disable lifecycle transitions.
- Testing implications:
  - Verify all mutation boundaries (0 floor, 3 floor, 9 cap).
  - Verify refill on each player turn start.
  - Verify card usage spends only when cost <= current energy.
  - Verify UI text updates after both current and per-turn value changes.

## Integration Notes

- Upstream dependencies:
  - ITurnManager event stream for refill and presenter visibility state.
  - Reflex scene bindings from SceneInstaller.
- Downstream consumers:
  - CardUsage gate/spend flow.
  - EnergyPresenter (and any future energy UI).
- Cross-system coupling risks:
  - Turn event naming/timing changes in TurnManager directly affect refill timing.
  - Any card pipeline change that bypasses CardUsage may skip energy spend.
  - Presenter text parsing assumes fixed x/y format in a single TMP text field.

## Known Risks and Open Questions

- Known limitations:
  - Method name RefilCurrentEnergy is misspelled and exposed in IEnergyManager API.
  - EnergyPresenter subscribes to turn-manager events in Start but does not unsubscribe from those turn events in OnDisable.
  - Current energy is not automatically clamped when EnergyPerTurn is decreased; temporarily CurrentEnergy can exceed EnergyPerTurn until next refill.
  - Presenter text updates depend on string split behavior, which is brittle if the display format changes.
- Open design questions:
  - Should EnergyPerTurn changes immediately clamp CurrentEnergy to maintain CurrentEnergy <= EnergyPerTurn at all times?
  - Should refill timing remain strictly OnPlayerTurnStart, or become configurable by mechanics/effects?
  - Should energy UI use structured fields (current and max text separately) instead of parsing combined text?
- Suggested follow-up tasks:
  - Rename RefilCurrentEnergy to RefillCurrentEnergy with a compatibility migration.
  - Add missing turn-event unsubscription in EnergyPresenter lifecycle.
  - Decide and document clamp policy when per-turn energy is reduced below current energy.
  - Add focused tests or validation checklist for energy boundary behavior and turn-start refill.
