# Effects System Documentation

## Purpose

The Effects system owns card-driven effect execution, runtime status-effect lifecycle, instant card effect behavior, and status-effect UI presentation.

Primary responsibilities:

- Execute configured card effects through `EffectSO` assets.
- Apply, stack, tick, and remove status effects on targets implementing `ITarget`.
- Run effect logic only after card attack-step chance gates pass.
- Trigger per-turn status processing based on `TurnExecutionState`.
- Render active effect icons, turn counters, effect values, and initial effect animations in combat UI.

The system is not responsible for:

- Turn sequencing policy, which is owned by `TurnManager`.
- Card energy payment, card discard, and attack-plan resolution, which are owned by the Cards system.
- Damage core pipeline for non-effect card damage, which is owned by `Cards/Base/Damage/CardDamage`.
- Target list ordering semantics, which are owned by `Targeting/ITargetsProvider`.
- Health and shield internals, which are owned by the Health and Shield systems.

## Reading Map

- Primary code locations:
  - `Assets/Effects/Base/EffectSO.cs`
  - `Assets/Effects/Base/EffectType.cs`
  - `Assets/Effects/StatusEffects/StatusEffectBase.cs`
  - `Assets/Effects/StatusEffects/StatusEffectReceiver.cs`
  - `Assets/Effects/StatusEffects/IncomingDamageModifier.cs`
  - `Assets/Effects/StatusEffects/CustomEffectSO.cs`
  - `Assets/Effects/StatusEffects/Burning/BurningEffectSO.cs`
  - `Assets/Effects/StatusEffects/Burning/BurningStatusEffect.cs`
  - `Assets/Effects/StatusEffects/Bleeding/BleedingEffectSO.cs`
  - `Assets/Effects/StatusEffects/Bleeding/BleedingStatusEffect.cs`
  - `Assets/Effects/StatusEffects/Paralysis/ParalysisEffectSO.cs`
  - `Assets/Effects/StatusEffects/Paralysis/ParalysisStatusEffect.cs`
  - `Assets/Effects/StatusEffects/Poisoning/PoisoningEffectSO.cs`
  - `Assets/Effects/StatusEffects/Poisoning/PoisoningStatusEffect.cs`
  - `Assets/Effects/InstantEffects/ExtendParalysis/ExtendParalysisEffectSO.cs`
  - `Assets/Effects/InstantEffects/ExtendParalysis/ExtendParalysisEffect.cs`
  - `Assets/Effects/InstantEffects/Add2EnergyToNextTurn/AddEnergyToNextTurnEffectSO.cs`
  - `Assets/Effects/InstantEffects/ConditionalBleedingDamageMultiplier/ConditionalDamageMultiplierEffectSO.cs`
  - `Assets/Effects/UI/AppliedEffectsPresenterUpdater.cs`
  - `Assets/Effects/UI/EffectsPresenter.cs`
  - `Assets/Effects/UI/AppliedEffectPresenter.cs`
  - `Assets/Effects/UI/InitialEffectPresenter.cs`
- Related docs:
  - `agent-docs/AGENTS.md`
  - `agent-docs/technology-documentation.md`
  - `agent-docs/project-coding-standards.md`
  - `agent-docs/cards-system-summary.md`
  - `agent-docs/cards-scriptable-objects-from-card-sheet-generator-summary.md`
  - `agent-docs/enemies-system-summary.md`
- Related agents or instructions:
  - `.agents/skills/create-effect/SKILL.md`
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/skills/architecture-review/SKILL.md`

## Architecture and Data Flow

- Core components:
  - `EffectSO` is the ScriptableObject base contract for card effects. It carries identity, description, sprite, initial animation, duration, stackability, `EffectType`, element, execution state, and `Execute(context)`.
  - `CardEffectContext` is the per-hit execution payload from card usage. It provides target, source object, target position, targets provider, and original step damage.
  - `StatusEffectBase` is the runtime stateful base for persistent status effects. It owns remaining turns, execution phase, stack/remove/tick lifecycle, and optional value display.
  - `StatusEffectReceiver` stores active effects, merges effects by `EffectType`, exposes query APIs, runs instant effects during application, and publishes `OnEffectsChanged`.
  - `AppliedEffectsPresenterUpdater` bridges gameplay and UI by processing status effects on turn events and refreshing presenter state.
  - `EffectsPresenter` owns runtime presenter instances keyed by active `EffectType`.
- Key interfaces:
  - `IStatusEffectBase`, `IStatusEffectReceiver`, `IIncomingDamageModifier`, `ICustomCardEffect`, `IEffectsPresenter`.
  - Cross-system interfaces: `ITarget`, `ITargetsProvider`, `ITurnManager`, `IParalyzable`, `IDamageable`.
- Runtime flow:
  1. `CardUsage` resolves attack hits and applies card damage per hit.
  2. For each hit, `CardUsage` rolls `CardAttackStep.EffectChance`; on success it builds `CardEffectContext` and calls `EffectSO.Execute`.
  3. Concrete `EffectSO` performs immediate behavior, applies a runtime status effect through `target.StatusEffectReceiver.ApplyStatusEffect(...)`, or performs instant card-only behavior.
  4. `StatusEffectReceiver` either stacks an existing effect with the same `EffectType` or applies a new effect instance to the target.
  5. If an applied or stacked effect has `TurnExecutionState.Instant`, `StatusEffectReceiver` immediately calls `PerformEffect`.
  6. `AppliedEffectsPresenterUpdater` listens to `ITurnManager` events and executes `effect.PerformEffect` for effects matching the current `TurnExecutionState`.
  7. `StatusEffectBase.PerformEffect` runs `ProcessTurnEffect`, decrements `RemainingTurns`, and removes itself when turns reach zero.
  8. On any add/remove/stack change, `OnEffectsChanged` triggers UI refresh through `EffectsPresenter`.

### Built-In Effects Behavior

- Burning:
  - Deals initial damage on `Execute`.
  - Applies per-turn burning damage.
  - Can spread to the nearest right valid target, then nearest left valid target, using `ITargetsProvider`.
  - Stacks duration and, when `CanStackValue` is true, per-turn damage.
- Bleeding:
  - Deals initial damage on `Execute`.
  - Applies per-turn damage.
  - Stacks duration and, when `CanStackValue` is true, per-turn damage.
- Paralysis:
  - Deals initial damage on `Execute`.
  - Calls `IParalyzable.ApplyParalysis` on apply and `IParalyzable.RemoveParalysis` on removal.
  - Uses remaining turns to keep the target unable to act for the configured duration.
- Poisoning:
  - Deals initial damage on `Execute`.
  - Applies per-turn damage.
  - Implements `IIncomingDamageModifier` to increase incoming card damage based on compatible or incompatible `Elements`.
  - Stacks duration and, when `CanStackValue` is true, per-turn damage.
- Extend Paralysis:
  - Checks whether the target currently has `EffectType.Paralysis`.
  - If present, applies a lightweight `ParalysisExtension` with `EffectType.Paralysis`, causing the existing paralysis effect to stack by the extension duration.
- Add Energy To Next Turn:
  - Adds configured bonus energy to the next turn through `EnergyManager.AddBonusEnergyForNextTurn`.
  - Currently locates `EnergyManager` with `FindAnyObjectByType`.
- Conditional Damage Multiplier:
  - Checks whether the target has any configured required `EffectType`.
  - Uses `CardEffectContext.StepDamage` to apply bonus damage after base card damage has already resolved.
- Custom Effect:
  - Delegates execution to an `ICustomCardEffect` component found on `CustomBehaviorPrefab`.

## Rules and Invariants

- Critical behavior rules:
  - Persistent effects are uniquely keyed for stacking by `EffectType` in `StatusEffectReceiver`.
  - Chance for applying card effects is owned by `CardAttackStep`, not by `EffectSO` implementations.
  - Stacking always adds `RemainingTurns`; value stacking only happens when `CanStackValue` is true.
  - Runtime effects must remove themselves through `target.StatusEffectReceiver.RemoveStatusEffect(this)`.
  - Card effect execution is per resolved hit, not once per card.
- Ordering or sequencing guarantees:
  - `CardUsage` applies card damage before effect execution on each hit.
  - Conditional bonus damage effects run after the base step damage because they are ordinary card effects.
  - Status ticks occur only on turn events currently wired in `AppliedEffectsPresenterUpdater`: `OnPlayerTurnStart`, `OnEnemyTurnStart`, and `OnEnemyTurnEnd`.
  - UI refresh happens after turn-state processing and on every `OnEffectsChanged` event.
- Constraints contributors must preserve:
  - Preserve `EffectType`-based uniqueness and stacking behavior unless intentionally redesigning persistence.
  - Keep chance/random gating outside `EffectSO` classes for card attack flow.
  - Preserve `TurnExecutionState` filtering to avoid effects ticking in unintended phases.
  - Preserve the `ITarget` contract dependency for effect application.
  - Keep effect metadata inspector-driven through `EffectSO` fields.

## Extension Points

- Safe extension areas:
  - Add new `EffectSO` implementations under `Assets/Effects/StatusEffects` or `Assets/Effects/InstantEffects`.
  - Add optional runtime status classes deriving from `StatusEffectBase`.
  - Assign `Sprite` and `InitialEffectAnimator` directly on each `EffectSO` asset for UI.
  - Add modifier-style behaviors by implementing interfaces such as `IIncomingDamageModifier`.
  - Add custom card-only behavior via `CustomEffectSO` and `ICustomCardEffect`.
- Required dependencies and contracts:
  - New persistent status effects must set `EffectType` and `ExecutionState` correctly through the backing `EffectSO` asset.
  - New runtime status effects must implement `OnApply`, `ProcessTurnEffect`, and `OnRemove` safely.
  - Effects relying on spread or retargeting must use `ITargetsProvider` semantics that match combat expectations.
  - UI expects active effects to expose usable `EffectData` when icons or initial animations are required.
  - Effects that block actions must target objects implementing `IParalyzable` or a future equivalent contract.
- Testing implications:
  - Validate stack behavior: same type stacks value/duration as intended and different types coexist.
  - Validate turn-state execution: each effect triggers only in the intended turn phase.
  - Validate removal path: effect is removed from receiver and UI presenter after expiration.
  - Validate poisoning modifier integration with card damage element combinations.
  - Validate instant effects that depend on `StepDamage`, existing active effects, or external scene services.

## Integration Notes

- Upstream dependencies:
  - Cards usage pipeline provides per-hit `CardEffectContext` and invokes `EffectSO.Execute` only when step chance passes.
  - `TurnManager` event stream drives status ticking through `AppliedEffectsPresenterUpdater`.
  - `ITargetsProvider` enables burning spread behavior and runtime retargeting helpers.
  - `EnergyManager` is used by `AddEnergyToNextTurnEffectSO`.
- Downstream consumers:
  - `CardDamage` reads active effects and applies `IIncomingDamageModifier` implementations.
  - Enemy, player party, and party character targets expose `IStatusEffectReceiver` through `ITarget` implementations.
  - UI presenters consume receiver state and `EffectSO` metadata for on-screen indicators.
  - Card targeting preview shows configured effect chances from attack steps, but does not simulate random rolls.
- Cross-system coupling risks:
  - `EffectType` enum growth requires coordinated asset setup and UI validation.
  - Poisoning damage scaling is coupled to the `Elements` enum and `CardDamage` modifier iteration order.
  - `AppliedEffectsPresenterUpdater` assumes a co-located `StatusEffectReceiver` and child `IEffectsPresenter`.
  - `AddEnergyToNextTurnEffectSO` bypasses the established DI pattern by finding `EnergyManager` globally.
  - `ConditionalDamageMultiplierEffectSO` depends on `CardEffectContext.StepDamage` being the unmodified configured step damage.

## Known Risks and Open Questions

- Known limitations:
  - `TurnExecutionState` includes `OnPlayerTurnEnd`, but `AppliedEffectsPresenterUpdater` currently does not process that event.
  - Several status effect classes keep a `_visualEffect` reference but never assign instantiated visual objects, making `RemoveVisualEffect` effectively a no-op.
  - `StatusEffectReceiver` stacks by `EffectType` only; multiple independent instances of the same effect type are not represented separately.
  - No hard cap or guard exists for stacked duration or stacked value growth.
  - `ExtendParalysis` relies on stacking into the existing paralysis effect and does not create its own visible effect instance.
  - `CustomEffectSO` executes an `ICustomCardEffect` component found directly on the configured prefab asset rather than instantiating it.
- Open design questions:
  - Should effects support independent instance stacks, with multiple icons or instances, rather than merged-by-type semantics?
  - Should `OnPlayerTurnEnd` be processed for parity with `TurnExecutionState`?
  - Should effect VFX be pooled or owned by status instances, or remain fire-and-forget from `EffectSO.Execute`?
  - Should instant effects receive injected services rather than using scene lookup or prefab-asset behavior?
- Suggested follow-up tasks:
  - Add explicit `OnPlayerTurnEnd` handling or remove the enum value if intentionally unsupported.
  - Define and implement visual effect lifecycle ownership policy for status effects.
  - Add regression tests/checklist for stacking and turn-phase execution rules.
  - Add regression coverage for per-hit `CardAttackStep` chance behavior and effect execution order.
  - Review instant effects for DI alignment and prefab instantiation semantics.
