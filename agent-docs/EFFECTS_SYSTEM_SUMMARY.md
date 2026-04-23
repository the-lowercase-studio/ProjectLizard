# Effects System Documentation

## Purpose

The Effects system owns card-driven effect execution, runtime status-effect lifecycle, and status-effect UI presentation.

Primary responsibilities:

- Execute configured card effects through EffectSO assets.
- Apply, stack, tick, and remove status effects on targets implementing ITarget.
- Run effect logic only after card attack-step chance gates pass.
- Trigger per-turn status processing based on TurnExecutionState.
- Render active effect icons/counters and initial effect animations in combat UI.

The system is not responsible for:

- Turn sequencing policy (owned by TurnManager).
- Damage core pipeline for non-effect card damage (owned by Cards/Base/Damage/CardDamage).
- Target list ordering semantics (owned by Targeting/TargetsProvider).
- Health/shield internals (owned by Health and Shield systems).

## Reading Map

- Primary code locations:
  - Assets/Effects/Base/EffectSO.cs
  - Assets/Effects/Base/EffectType.cs
  - Assets/Effects/StatusEffects/StatusEffectBase.cs
  - Assets/Effects/StatusEffects/StatusEffectReceiver.cs
  - Assets/Effects/StatusEffects/IncomingDamageModifier.cs
  - Assets/Effects/StatusEffects/CustomEffectSO.cs
  - Assets/Effects/StatusEffects/Burning/BurningEffectSO.cs
  - Assets/Effects/StatusEffects/Burning/BurningStatusEffect.cs
  - Assets/Effects/StatusEffects/Bleeding/BleedingEffectSO.cs
  - Assets/Effects/StatusEffects/Bleeding/BleedingStatusEffect.cs
  - Assets/Effects/StatusEffects/Paralysis/ParalysisEffectSO.cs
  - Assets/Effects/StatusEffects/Paralysis/ParalysisStatusEffect.cs
  - Assets/Effects/StatusEffects/Poisoning/PoisoningEffectSO.cs
  - Assets/Effects/StatusEffects/Poisoning/PoisoningStatusEffect.cs
  - Assets/Effects/UI/AppliedEffectsPresenterUpdater.cs
  - Assets/Effects/UI/EffectsPresenter.cs
  - Assets/Effects/UI/AppliedEffectPresenter.cs
  - Assets/Effects/UI/InitialEffectPresenter.cs
  - Assets/Effects/UI/EffectTypeSpriteMappingSO.cs
- Related docs:
  - agent-docs/AGENTS.md
  - agent-docs/TECHNOLOGY_DOCUMENTATION.md
  - agent-docs/PROJECT_CODING_STANDARDS.md
  - agent-docs/CARDS_SYSTEM_SUMMARY.md
  - agent-docs/ENEMY_INTENTION_SYSTEM_SUMMARY.md
- Related agents or instructions:
  - .agents/skills/document-system/SKILL.md
  - .agents/skills/architecture-review/SKILL.md

## Architecture and Data Flow

- Core components:
  - EffectSO is the ScriptableObject base contract for card effects; it carries metadata (duration, stackability, type, execution state) and Execute(context).
  - StatusEffectBase is the runtime stateful implementation base (RemainingTurns, EffectType, ExecutionState, stack/remove/tick lifecycle).
  - StatusEffectReceiver stores active effects, stacks by EffectType, exposes query APIs, and publishes OnEffectsChanged.
  - AppliedEffectsPresenterUpdater bridges gameplay and UI by processing effects on turn events and refreshing presenter state.
  - EffectsPresenter owns runtime presenter instances per active EffectType.
- Key interfaces:
  - IStatusEffect, IStatusEffectReceiver, IIncomingDamageModifier, ICustomCardEffect, IEffectsPresenter.
  - Cross-system interfaces: ITarget, ITargetsProvider, ITurnManager, IParalyzable, IDamageable.
- Runtime flow:
  1. CardUsage resolves attack hits and applies damage per hit.
  2. For each hit, CardUsage rolls CardAttackStep.EffectChance; on success it builds CardEffectContext and calls EffectSO.Execute.
  3. Concrete EffectSO performs immediate behavior (for example direct damage), then applies runtime status effect through target.StatusEffectReceiver.ApplyStatusEffect(...).
  4. StatusEffectReceiver either stacks an existing effect with same EffectType or applies a new instance to the target.
  5. AppliedEffectsPresenterUpdater listens to ITurnManager events and executes effect.PerformEffect for effects matching the current TurnExecutionState.
  6. StatusEffectBase.PerformEffect runs ProcessTurnEffect, decrements RemainingTurns, and removes itself when turns reach zero.
  7. On any add/remove/stack change, OnEffectsChanged triggers UI refresh through EffectsPresenter.

### Built-in Effects Behavior

- Burning:
  - Deals initial damage on Execute.
  - Applies per-turn burning damage.
  - May spread to nearest right/left valid target via ITargetsProvider.
- Bleeding:
  - Deals initial damage on Execute.
  - Applies per-turn damage and stacks damage value.
- Paralysis:
  - Deals initial damage on Execute.
  - Applies target paralysis via IParalyzable for effect duration.
- Poisoning:
  - Deals initial damage on Execute.
  - Applies per-turn damage.
  - Implements IIncomingDamageModifier to scale incoming damage based on element compatibility.

## Rules and Invariants

- Critical behavior rules:
  - Effects are uniquely keyed for stacking by EffectType in StatusEffectReceiver.
  - Chance for applying card effects is owned by CardAttackStep, not by EffectSO implementations.
  - Stacking always adds RemainingTurns; value stacking is conditional on CanStackValue and StackValue override.
  - Expiration happens only through RemainingTurns countdown in PerformEffect.
  - Removal always routes through target.StatusEffectReceiver.RemoveStatusEffect(this).
- Ordering or sequencing guarantees:
  - CardUsage executes damage before effect execution on each hit.
  - Effect ticks occur only on turn events currently wired in AppliedEffectsPresenterUpdater: OnPlayerTurnStart, OnEnemyTurnStart, OnEnemyTurnEnd.
  - UI refresh happens after turn-state processing and on any OnEffectsChanged event.
- Constraints contributors must preserve:
  - Preserve EffectType-based uniqueness/stacking behavior unless intentionally redesigning persistence model.
  - Keep chance/random gating outside EffectSO classes for card attack flow.
  - Preserve TurnExecutionState filtering to avoid effects ticking in unintended phases.
  - Preserve ITarget contract dependency for effect application (Damageable + StatusEffectReceiver).
  - Keep effect metadata inspector-driven through EffectSO fields.

## Extension Points

- Safe extension areas:
  - Add new EffectSO implementations and optional runtime status classes deriving from StatusEffectBase.
  - Add new display mappings in EffectTypeSpriteMappingSO for icon and initial animator.
  - Add new modifier-style behaviors by implementing interfaces such as IIncomingDamageModifier.
  - Add custom card-only behavior via CustomEffectSO + ICustomCardEffect.
- Required dependencies and contracts:
  - New persistent status effects must set EffectType and ExecutionState correctly and implement ProcessTurnEffect safely.
  - Any effect relying on spread/retargeting must use ITargetsProvider semantics that match combat expectations.
  - UI requires EffectTypeSpriteMappingSO entries for all user-visible effects.
  - If effect blocks actions, target must implement IParalyzable or equivalent contract.
- Testing implications:
  - Validate stack behavior: same type stacks value/duration as intended and different types coexist.
  - Validate turn-state execution: each effect triggers only in intended turn phase.
  - Validate removal path: effect is removed from receiver and UI presenter after expiration.
  - Validate poisoning modifier integration with card damage element combinations.

## Integration Notes

- Upstream dependencies:
  - Cards usage pipeline provides per-hit CardEffectContext and invokes EffectSO.Execute only when step chance passes.
  - TurnManager event stream drives status ticking through AppliedEffectsPresenterUpdater.
  - TargetsProvider enables burning spread behavior and runtime retargeting helpers.
- Downstream consumers:
  - CardDamage reads active effects and applies IIncomingDamageModifier implementations.
  - EnemyBase, PlayerParty, and PartyCharacter expose IStatusEffectReceiver through ITarget implementations.
  - UI presenters consume receiver state and effect metadata for on-screen indicators.
- Cross-system coupling risks:
  - EffectType enum growth requires coordinated updates in mapping assets and UI.
  - Poisoning damage scaling is coupled to Elements model and CardDamage modifier iteration order.
  - AppliedEffectsPresenterUpdater assumes co-located StatusEffectReceiver and child IEffectsPresenter.

## Known Risks and Open Questions

- Known limitations:
  - TurnExecutionState includes OnPlayerTurnEnd, but AppliedEffectsPresenterUpdater currently does not process that event.
  - Several status effect classes keep a \_visualEffect reference but never assign instantiated visual objects, making RemoveVisualEffect effectively a no-op.
  - StatusEffectReceiver stacks by EffectType only; multiple independent instances of same effect type are not represented separately.
  - No hard cap/guard exists for stacked duration or stacked value growth.
- Open design questions:
  - Should effects support independent instance stacks (multiple icons/instances) rather than merged-by-type semantics?
  - Should OnPlayerTurnEnd be processed for parity with TurnExecutionState enum?
  - Should effect VFX be pooled/owned by status instances, or remain fire-and-forget from EffectSO.Execute?
  - Should CustomEffectSO instantiate behavior prefabs before execution to avoid prefab-asset component side effects?
- Suggested follow-up tasks:
  - Add explicit OnPlayerTurnEnd handling or remove enum value if intentionally unsupported.
  - Define and implement visual effect lifecycle ownership policy for status effects.
  - Add regression tests/checklist for stacking and turn-phase execution rules.
  - Add regression coverage for per-hit CardAttackStep chance behavior and effect execution order.
