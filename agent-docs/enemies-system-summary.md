# Enemies System Documentation

## Purpose

The Enemies system owns runtime enemy combat entities, their per-turn intention selection and execution, enemy-specific action implementations, enemy UI intention display, and enemy death finalization.

Primary responsibilities:

- Represent an enemy combat target through `EnemyBase` (health, shield handling, status-receiver access, and damage intake behavior).
- Select and execute enemy intentions on turn events.
- Define modular intention actions (attack, defense, special) through a polymorphic action contract.
- Present selected intention icon/value to the player.
- Hide enemy UI and complete destroy lifecycle after death flow finishes.

The system is not responsible for:

- Global turn sequencing policy and pacing delays (owned by `TurnManager`).
- Player-targeting policy abstraction (consumes `IPlayerParty` as target).
- Generic health and shield internals (owned by Health and Shield systems).
- Card/effect authoring pipeline (owned by Cards and Effects systems).

## Reading Map

- Primary code locations:
  - `Assets/Enemies/Base/EnemyBase.cs`
  - `Assets/Enemies/Base/EnemyConfigSO.cs`
  - `Assets/Enemies/Base/EnemyDeathHandler.cs`
  - `Assets/Enemies/Intentions/EnemyActionBase.cs`
  - `Assets/Enemies/Intentions/IntentionConfig.cs`
  - `Assets/Enemies/Intentions/IntentionSelector.cs`
  - `Assets/Enemies/Intentions/IntentionType.cs`
  - `Assets/Enemies/Intentions/IntentionTypeAttribute.cs`
  - `Assets/Enemies/Intentions/EnemyAnimationPlayer.cs`
  - `Assets/Enemies/UI/IntentionIndicator.cs`
  - `Assets/Enemies/Actions/AttackAction.cs`
  - `Assets/Enemies/Actions/DefenseAction.cs`
  - `Assets/Enemies/Actions/SpecialAction.cs`
  - `Assets/Editor/Enemies/IntentionConfigPropertyDrawer.cs`
- Related docs:
  - `agent-docs/AGENTS.md`
  - `agent-docs/technology-documentation.md`
  - `agent-docs/project-coding-standards.md`
  - `agent-docs/enemy-intention-system-summary.md`
  - `agent-docs/effects-system-summary.md`
  - `agent-docs/damage-numbers-system-summary.md`
- Related agents or instructions:
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/skills/architecture-review/SKILL.md`

## Architecture and Data Flow

- Core components:
  - `EnemyBase`: runtime enemy root implementing `IEnemyBase`, `IDamageable`, `IShielded`, and `IParalyzable`; subscribes to turn events and runs intention lifecycle.
  - `EnemyConfigSO`: designer-authored ScriptableObject with identity fields, sprite, max health, and intention list.
  - `IntentionConfig`: serializable tuple of intention type + probability + polymorphic action (`[SerializeReference] EnemyActionBase`).
  - `IntentionSelector`: weighted random chooser over configured probabilities.
  - `EnemyActionBase` / `IEnemyAction`: action contract with value-roll lifecycle (`RefreshValue` and `GetValue`) and execute hook.
  - Built-in actions:
    - `AttackAction`: damages target.
    - `DefenseAction`: adds shield to self.
    - `SpecialAction`: currently same behavior as attack (placeholder).
  - `EnemyAnimationPlayer`: translates intention type into animator trigger if supported by this enemy.
  - `IntentionIndicator`: runtime icon/value presenter for selected intention.
  - `EnemyDeathHandler`: hides enemy-specific UI, plays the death animation when available, and destroys the enemy after the animation callback.
- Key interfaces:
  - Enemy-facing: `IEnemyBase`, `IEnemyAction`, `IEnemyAnimationPlayer`.
  - Turn integration: `ITurnManager` (`OnPlayerTurnStart`, `OnEnemyTurnStart`, `OnEnemyTurnEnd`).
  - Combat integration: `ITarget`, `IDamageable`, `IShielded`, `IShieldReceiver`, `IParalyzable`, `IHealth`, `IStatusEffectReceiver`.
- Runtime flow:
  1. Enemy enables and subscribes to turn/death sequence events.
  2. On `OnPlayerTurnStart`, living enemy selects next intention (or shows self-paralysis intention if paralyzed).
  3. Selection uses weighted probability from config and refreshes action rolled value.
  4. Selected intention is shown via `IntentionIndicator`.
  5. On `OnEnemyTurnStart`, enemy clears its current shield.
  6. On `OnEnemyTurnEnd`, living enemy executes selected action against injected `IPlayerParty` target and triggers matching animation.
  7. Enemy damage intake applies shield-first split, emits shield/health damage numbers, updates health, and plays hit VFX when still alive.
  8. On death, `EnemyDeathHandler` hides enemy UI, plays death animation through `EnemyAnimationPlayer` when available, and destroys the enemy object when the animation path completes.
  9. Separately, `DeathHandlerBase.OnCompleted` is relayed by `EnemyBase.OnCanBeDestroyed` after death VFX/audio completion; this event should not be assumed to be the same moment as object destruction.

### Inspector Authoring Path

- `EnemyConfigSO.Intentions` stores a list of `IntentionConfig` entries.
- `IntentionConfigPropertyDrawer` constrains available action types based on selected intention enum by scanning `EnemyActionBase` subtypes annotated with `IntentionTypeAttribute`.
- Changing intention type clears previously chosen action reference to prevent mismatched pairs.

## Rules and Invariants

- Critical behavior rules:
  - Intention selection only occurs for alive enemies.
  - Enemy paralysis blocks normal intention selection and action execution.
  - Intention action value must be refreshed at selection time before indicator display.
  - Damage resolution is shield-first, then remaining health damage.
  - Enemy turn-start clears shield each turn for alive enemies.
- Ordering or sequencing guarantees:
  - Selection happens at player-turn start.
  - Execution happens on enemy-turn end event.
  - Intention animation trigger is attempted before action execution.
  - Death UI is hidden before death animation playback begins.
  - `OnCanBeDestroyed` is emitted only after the inherited death effects sequence completes.
- Constraints contributors must preserve:
  - Keep `IntentionConfig` + `[SerializeReference]` action model compatible with existing property drawer workflow.
  - Keep action classes serializable and default-constructible when exposed through drawer (`Activator.CreateInstance`).
  - Preserve intention-type to action-type mapping via `IntentionTypeAttribute`.
  - Preserve shield-first damage popup split behavior in `EnemyBase.TakeDamage`.

## Extension Points

- Safe extension areas:
  - Add new intention actions by deriving from `EnemyActionBase`.
  - Add new intention enum values and corresponding indicator icon + animation trigger mapping.
  - Add enemy-specific animation triggers/controllers without changing core intention flow.
  - Add richer defense/special behavior inside new action classes.
- Required dependencies and contracts:
  - New action classes should include `[Serializable]` and `[IntentionType(...)]` to appear in the drawer.
  - New actions must tolerate missing or incompatible target contracts safely.
  - New intention types require synchronized updates in:
    - `IntentionType` enum.
    - `IntentionIndicator.ShowActionIntention` switch.
    - `EnemyAnimationPlayer.GetTriggerNameForIntention` mapping.
- Testing implications:
  - Validate weighted selection distribution and zero-probability behavior.
  - Validate paralysis behavior for both indicator and action skip.
  - Validate shield clear on enemy-turn start and shield-first damage split.
  - Validate death path hides UI and destroys exactly once.

## Integration Notes

- Upstream dependencies:
  - `TurnManager` event stream drives intention lifecycle.
  - Enemy config assets and prefab references define runtime behavior/visuals.
  - Effects system may call `IParalyzable` methods on enemy targets.
- Downstream consumers:
  - Enemy death and target availability feed broader combat turn resolution.
  - Damage numbers and VFX systems consume enemy damage events.
  - Player party takes action damage through `ITarget.Damageable`.
- Cross-system coupling risks:
  - Enemy actions currently assume `IPlayerParty` as a single target abstraction.
  - Intention visualization and animation mapping must stay in sync with intention enum growth.
  - Editor drawer reflection-based discovery depends on assembly/type metadata and attributes.
  - Enemy death currently crosses animation, VFX/audio completion, and object destruction paths; changes should preserve their intended ordering explicitly.

## Known Risks and Open Questions

- Known limitations:
  - `SelectIntention` calls `_currentIntention.Action.RefreshValue()` before verifying action is non-null, so a config with missing action can throw.
  - `IntentionSelector` has no guard against negative probabilities; invalid data can skew or break expected weighting semantics.
  - `SpecialAction` is currently behavior-equivalent to `AttackAction` (placeholder with TODO).
  - `EnemyBase.Name` returns `Config.name` (asset object name) instead of `Config.Name` (inspector field), which may be intentional but can surprise designers.
  - `IntentionConfigPropertyDrawer` contains an unused `_lastIntentionTypes` cache field.
  - `DeathHandlerBase` waits for both VFX and audio completion events, but enemy audio death playback is currently commented out; verify completion behavior before depending on `OnCanBeDestroyed`.
- Open design questions:
  - Should intention execution happen on enemy-turn start instead of enemy-turn end for clearer phase semantics?
  - Should intention indicator be explicitly hidden after execution/skip, or intentionally persist until next selection?
  - Should action contracts support multi-target selection and context objects instead of single `ITarget`?
  - Should probability validation be enforced in editor (for non-negative and optional normalization checks)?
- Suggested follow-up tasks:
  - Add null-safe handling for selected intentions with missing actions.
  - Add validation utility/editor checks for intention probability constraints.
  - Decide and document intended turn phase for execution and indicator visibility policy.
  - Implement a distinct special-action baseline behavior or rename to avoid ambiguity.
