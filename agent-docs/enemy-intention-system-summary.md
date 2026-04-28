# Enemy Intention System Documentation

## Purpose

The Enemy Intention system lets each enemy declare the action it plans to take, show that plan to the player, and execute the selected action during the enemy phase.

It is part of the broader Enemies system. Use `agent-docs/enemies-system-summary.md` as the canonical overview for enemy runtime lifecycle, damage intake, shield behavior, and death handling.

The intention system is responsible for:

- Storing per-enemy intention options on `EnemyConfigSO`.
- Selecting one intention by weighted probability.
- Rolling the action value before displaying the intention.
- Showing an icon/value preview through `IntentionIndicator`.
- Executing the selected `EnemyActionBase` against the current target.
- Triggering matching enemy animation triggers when supported.

It is not responsible for:

- Global turn sequencing (`TurnManager` owns phase events).
- Player-party damage/shield internals.
- Health, death VFX/audio, or object destruction.
- Card/effect authoring outside status effects that interact with enemy targets.

## Reading Map

- Primary code locations:
  - `Assets/Enemies/Base/EnemyBase.cs`
  - `Assets/Enemies/Base/EnemyConfigSO.cs`
  - `Assets/Enemies/Intentions/IntentionConfig.cs`
  - `Assets/Enemies/Intentions/IntentionSelector.cs`
  - `Assets/Enemies/Intentions/EnemyActionBase.cs`
  - `Assets/Enemies/Intentions/IntentionType.cs`
  - `Assets/Enemies/Intentions/IntentionTypeAttribute.cs`
  - `Assets/Enemies/Intentions/EnemyAnimationPlayer.cs`
  - `Assets/Enemies/UI/IntentionIndicator.cs`
  - `Assets/Enemies/Actions/AttackAction.cs`
  - `Assets/Enemies/Actions/DefenseAction.cs`
  - `Assets/Enemies/Actions/SpecialAction.cs`
  - `Assets/Editor/Enemies/IntentionConfigPropertyDrawer.cs`
- Related docs:
  - `agent-docs/enemies-system-summary.md`
  - `agent-docs/effects-system-summary.md`
  - `agent-docs/project-coding-standards.md`
- Related agents or instructions:
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/skills/balance-analysis/SKILL.md`

## Architecture and Data Flow

- Core components:
  - `IntentionConfig`: serializable config entry with `IntentionType`, probability, and `[SerializeReference] EnemyActionBase` action.
  - `IntentionSelector`: selects one intention from the configured list using weighted random probability.
  - `EnemyActionBase`: base contract for rollable action values and execution.
  - `IntentionTypeAttribute`: maps action classes to compatible intention enum values for editor filtering.
  - `IntentionConfigPropertyDrawer`: inspector drawer that filters action types by selected intention and clears mismatched action references when type changes.
  - `IntentionIndicator`: maps selected intention type to sprite and displays the rolled value when an action exists.
  - `EnemyAnimationPlayer`: maps supported intention types to animator triggers.
- Runtime flow:
  1. On player-turn start, `EnemyBase` selects a new intention if alive and not paralyzed.
  2. `IntentionSelector` chooses from `EnemyConfigSO.Intentions` based on summed probabilities.
  3. The selected action rolls its current value through `RefreshValue`.
  4. `IntentionIndicator` displays the selected icon and rolled value.
  5. On enemy-turn start, `EnemyBase` clears its shield.
  6. On enemy-turn end, `EnemyBase` executes the selected action against injected `IPlayerParty`.
  7. If the enemy is paralyzed, it shows `SelfParalysis`, clears the current action, and skips execution.

## Rules and Invariants

- Critical behavior rules:
  - Intentions are selected only for living enemies.
  - Action value is rolled before the indicator reads `GetValue`.
  - `Probability` values are treated as weights, not percentages that must sum to 100.
  - An all-zero probability list returns no intention.
  - `SelfParalysis` is a display/skip state, not a normal configured action.
- Ordering or sequencing guarantees:
  - Selection happens on `ITurnManager.OnPlayerTurnStart`.
  - Shield clearing happens on `ITurnManager.OnEnemyTurnStart`.
  - Action execution happens on `ITurnManager.OnEnemyTurnEnd`.
  - Animation trigger is attempted immediately before action execution.
- Constraints contributors must preserve:
  - Keep action classes `[Serializable]`, derive from `EnemyActionBase`, and add `[IntentionType(...)]` when they should be selectable in the inspector.
  - Keep action classes default-constructible because the property drawer uses `Activator.CreateInstance`.
  - Update indicator icons and animation trigger mapping when adding new intention types.

## Extension Points

- Safe extension areas:
  - Add new `EnemyActionBase` subclasses for richer attack, defense, support, or status-effect behavior.
  - Add new `IntentionType` values when the player needs a distinct preview category.
  - Add enemy-specific animator trigger support through controller parameters.
- Required dependencies and contracts:
  - Actions receive the acting `IEnemyBase` and an `ITarget`.
  - Attack-like actions should use `target.Damageable`.
  - Defense-like actions should use the enemy's `IShielded.ShieldReceiver`.
- Testing implications:
  - Validate inspector action filtering when adding intention/action pairs.
  - Validate null or missing action behavior in configs.
  - Validate weighted selection behavior with zero and mixed probabilities.
  - Validate paralyzed enemies show the correct indicator and do not execute stale actions.

## Integration Notes

- Upstream dependencies:
  - `TurnManager` provides the event timing.
  - `EnemyConfigSO` assets provide intention data.
  - Effects can drive paralysis through `IParalyzable`.
- Downstream consumers:
  - `IntentionIndicator` exposes enemy plans to players.
  - `EnemyAnimationPlayer` uses intention type to drive animation triggers.
  - Player party receives enemy action effects through target interfaces.
- Cross-system coupling risks:
  - New intention enum values require synchronized updates across config drawer, indicator, and animation mapping.
  - The current execution target is the injected `IPlayerParty`; multi-target or per-character targeting requires a broader target-selection design.
  - Reflection-based editor discovery depends on action class metadata being available in loaded assemblies.

## Known Risks and Open Questions

- Known limitations:
  - `EnemyBase.SelectIntention` calls `_currentIntention.Action.RefreshValue()` before null-checking `_currentIntention` or its action, so missing actions can throw.
  - `IntentionSelector` does not reject negative probabilities.
  - `SpecialAction` currently behaves like `AttackAction`.
  - The indicator is not explicitly hidden after action execution; it persists until the next selection or death UI hide.
- Open design questions:
  - Should enemy actions execute on enemy-turn start instead of enemy-turn end for clearer phase naming?
  - Should probability validation happen in `EnemyConfigSO`, the property drawer, or a separate editor validation tool?
  - Should actions receive a richer execution context instead of just enemy + target?
- Suggested follow-up tasks:
  - Add null-safe selection/execution handling for missing actions.
  - Add editor validation for negative probabilities and empty action references.
  - Give `SpecialAction` a distinct baseline behavior or remove it until needed.
