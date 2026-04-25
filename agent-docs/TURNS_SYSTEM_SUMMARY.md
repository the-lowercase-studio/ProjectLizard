# Turns System Documentation

## Purpose

The Turns system owns combat phase sequencing and exposes the event stream that other combat systems use to react to player and enemy turns.

Primary responsibilities:

- Start the first player turn when the scene begins.
- Publish player-turn and enemy-turn lifecycle events through `ITurnManager`.
- Advance `CurrentTurn` after the enemy-turn delay and before the next player turn begins.
- Provide the player end-turn UI bridge through `EndPlayerTurnButton`.

The system is not responsible for:

- Card draw, discard, targeting, or usage rules.
- Energy refill or bonus-energy logic.
- Enemy intention selection or action behavior.
- Status-effect internals or damage/shield resolution.
- Combat victory/defeat resolution.

## Reading Map

- Primary code locations:
  - `Assets/Turns/TurnManager.cs`
  - `Assets/Turns/TurnExecutionState.cs`
  - `Assets/Turns/TurnState.cs`
  - `Assets/Turns/EndPlayerTurnButton.cs`
  - `Assets/Installers/SceneInstaller.cs`
- Related docs:
  - `agent-docs/AGENTS.md`
  - `agent-docs/TECHNOLOGY_DOCUMENTATION.md`
  - `agent-docs/PROJECT_CODING_STANDARDS.md`
  - `agent-docs/SYSTEM_ARCHITECTURE_VISUAL.md`
  - `agent-docs/CARDS_SYSTEM_SUMMARY.md`
  - `agent-docs/ENERGY_SYSTEM_SUMMARY.md`
  - `agent-docs/ENEMIES_SYSTEM_SUMMARY.md`
  - `agent-docs/EFFECTS_SYSTEM_SUMMARY.md`
- Related agents or instructions:
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/skills/architecture-review/SKILL.md`

## Architecture and Data Flow

- Core components:
  - `ITurnManager`: interface consumed by gameplay systems through Reflex injection.
  - `TurnManager`: scene-level `MonoBehaviour` that emits turn lifecycle events and owns `CurrentTurn`.
  - `EndPlayerTurnButton`: Unity UI adapter that shows during player turns, hides after player-turn end, and calls `ITurnManager.EndPlayerTurn` on click.
  - `TurnExecutionState`: enum used by effects to map status processing to turn phases.
  - `TurnState`: smaller enum with player start, enemy start, and enemy end values; currently no direct usage was found in the main runtime references.
- Key interfaces:
  - `ITurnManager` exposes `CurrentTurn`, four lifecycle events, phase transition methods, and target registration methods.
  - Downstream systems subscribe to `OnPlayerTurnStart`, `OnPlayerTurnEnd`, `OnEnemyTurnStart`, and `OnEnemyTurnEnd`.
- Runtime flow:
  1. `TurnManager.Start` calls `StartPlayerTurn`.
  2. `StartPlayerTurn` invokes `OnPlayerTurnStart`.
  3. Player-controlled systems run until the end-turn button calls `EndPlayerTurn`.
  4. `EndPlayerTurn` invokes `OnPlayerTurnEnd`, then calls `StartEnemyTurn`.
  5. `StartEnemyTurn` starts a coroutine that waits `0.5` seconds.
  6. After the wait, `OnEnemyTurnStart` is invoked, then `EndEnemyTurn` is called immediately.
  7. `EndEnemyTurn` starts a coroutine that waits `0.5` seconds.
  8. After the wait, `CurrentTurn` increments, `OnEnemyTurnEnd` is invoked, then `StartPlayerTurn` starts the next cycle.

### Main Event Consumers

- `EnergyManager` listens to `OnPlayerTurnStart` to refill current energy and apply any stored next-turn bonus energy.
- `CardsHandManager` listens to `OnPlayerTurnStart` to fill the hand and `OnPlayerTurnEnd` to discard it.
- `EndPlayerTurnButton` listens to `OnPlayerTurnStart` and `OnPlayerTurnEnd` to toggle button visibility.
- `PlayerParty` listens to player-turn events and clears party shield on `OnPlayerTurnStart`.
- `EnemyBase` listens to:
  - `OnPlayerTurnStart` to select and display the next intention.
  - `OnEnemyTurnStart` to clear enemy shield.
  - `OnEnemyTurnEnd` to execute the selected intention.
- `AppliedEffectsPresenterUpdater` listens to `OnPlayerTurnStart`, `OnEnemyTurnStart`, and `OnEnemyTurnEnd` to process status effects matching those `TurnExecutionState` values.

## Rules and Invariants

- Critical behavior rules:
  - `CurrentTurn` starts at `1`.
  - `CurrentTurn` increments before `OnEnemyTurnEnd` fires and before the next player turn starts.
  - `OnPlayerTurnEnd` is the only lifecycle event emitted directly by the end-turn button path.
  - Enemy intention selection happens during player-turn start, while execution currently happens during enemy-turn end.
  - Player shield clears on player-turn start; enemy shield clears on enemy-turn start.
- Ordering or sequencing guarantees:
  - Current order is `OnPlayerTurnStart` -> `OnPlayerTurnEnd` -> wait -> `OnEnemyTurnStart` -> wait -> increment `CurrentTurn` -> `OnEnemyTurnEnd` -> next `OnPlayerTurnStart`.
  - `OnEnemyTurnStart` and `OnEnemyTurnEnd` are separated by a fixed coroutine wait, but there is no explicit wait for enemy action completion.
  - Subscribers run in Unity event subscription order; contributors should not rely on a specific subscriber order unless they make it explicit.
- Constraints contributors must preserve:
  - Keep `ITurnManager` as the injected turn dependency; do not add singleton access to the turn manager.
  - Preserve event names and phase semantics unless updating all dependent systems and docs together.
  - Treat turn-event changes as high-risk because cards, energy, enemies, effects, player party, and UI all subscribe to this stream.
  - Do not edit prefabs directly when wiring turn UI or manager references; configure prefab/scene references in the Unity Editor.

## Extension Points

- Safe extension areas:
  - Add explicit phase state tracking if systems need to query the active phase rather than only react to events.
  - Replace fixed wait durations with serialized fields or a pacing service.
  - Add action-completion gating so enemy turn end waits for enemy animations/actions and status processing to finish.
  - Expand `ITurnManager` with narrowly scoped methods when multiple systems need a shared turn query.
- Required dependencies and contracts:
  - SceneInstaller must bind the scene `TurnManager` instance as `ITurnManager`.
  - UI buttons should call `ITurnManager` rather than reference `TurnManager` directly.
  - Systems that subscribe to turn events must unsubscribe in matching lifecycle methods.
  - Status effects that use `TurnExecutionState.OnPlayerTurnEnd` currently need an event bridge before they will tick at that phase.
- Testing implications:
  - Validate event order after any change to `TurnManager`.
  - Validate card hand fill/discard and energy refill around player-turn boundaries.
  - Validate enemy shield clear, intention display, and intention execution phase.
  - Validate status effects tick only on their configured turn phase.
  - Validate rapid clicking or repeated `EndPlayerTurn` calls does not start overlapping enemy-turn coroutines if turn gating is added.

## Integration Notes

- Upstream dependencies:
  - Reflex scene binding in `SceneInstaller`.
  - Unity lifecycle methods and coroutines.
  - End-turn UI click events.
- Downstream consumers:
  - Cards, Energy, Player Party, Enemies, Effects UI/status processing, and turn UI.
- Cross-system coupling risks:
  - Effects expose `TurnExecutionState.OnPlayerTurnEnd`, but `AppliedEffectsPresenterUpdater` does not currently process `OnPlayerTurnEnd`.
  - Enemy action execution on `OnEnemyTurnEnd` means the event name describes the phase used for execution, not necessarily a fully completed enemy turn.
  - `TurnManager` has target registration methods and a local target list, but no current turn resolution logic uses that list.
  - The current coroutine chain can advance turns without waiting for card/effect/enemy animations or action completion callbacks.

## Known Risks and Open Questions

- Known limitations:
  - Fixed `0.5` second waits are hard-coded in `TurnManager`.
  - There is no active phase guard, so repeated external calls can potentially start overlapping transitions.
  - The TODO in `TurnManager` notes that the current implementation is for testing and does not wait for effects or enemy actions to complete.
  - `TurnManager` keeps a `targets` list through `RegisterTarget` and `UnregisterTarget`, but no active runtime behavior uses it.
  - Private field `targets` does not follow the current `_camelCase` field convention.
- Open design questions:
  - Should enemy intentions execute on enemy-turn start, enemy-turn end, or a separate enemy-action phase?
  - Should `OnPlayerTurnEnd` be wired into status-effect processing or removed from `TurnExecutionState` if intentionally unsupported?
  - Should `TurnState` be merged with `TurnExecutionState`, expanded, or removed if unused?
  - Should turn advancement wait on completion contracts from cards, status effects, enemies, VFX, or animation systems?
- Suggested follow-up tasks:
  - Add a small event-order test or manual validation checklist for turn transitions.
  - Add an active phase/state guard before changing turn pacing or adding asynchronous completion.
  - Decide and document intended enemy action timing, then align `ENEMIES_SYSTEM_SUMMARY.md` if the phase changes.
  - Rename `targets` to `_targets` during the next code touch in `TurnManager`.
