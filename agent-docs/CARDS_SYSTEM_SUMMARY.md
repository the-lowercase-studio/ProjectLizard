# Cards System Documentation

## Purpose

The Cards system owns the player card lifecycle during combat turns:

- Creating card instances from ScriptableObject configs.
- Presenting cards in hand and handling card interaction states (hover, drag, drop).
- Executing card usage pipeline (energy gate, attack animation trigger, attack-step flow, discard).
- Showing targeting preview UI on card hover.

The system is not responsible for:

- Turn sequencing policy (owned by TurnManager).
- Energy rules beyond cost check and deduction (owned by Energy system).
- Enemy ordering/selection strategy internals (owned by TargetsProvider).
- Damage number spawn implementation (owned by DamageNumbers/UI integration).

## Reading Map

- Primary code locations:
  - Assets/Cards/Base/Card.cs
  - Assets/Cards/Base/CardInteractions.cs
  - Assets/Cards/Base/CardMovement.cs
  - Assets/Cards/Base/CardRotation.cs
  - Assets/Cards/Base/CardScaler.cs
  - Assets/Cards/Base/CardPresenter.cs
  - Assets/Cards/Base/Usage/CardUsage.cs
  - Assets/Cards/Base/Usage/CardUsageArea.cs
  - Assets/Cards/Base/Damage/CardDamage.cs
  - Assets/Cards/Base/Damage/CardDamageSO.cs
  - Assets/Cards/CardsHand/CardsHandManager.cs
  - Assets/Cards/CardsHand/CardsHandPresenter.cs
  - Assets/Cards/TargetingPreview/CardTargetingPreview.cs
  - Assets/Cards/TargetingPreview/UI/CardTargetCrosshairPresenter.cs
  - Assets/Cards/TargetingPreview/UI/CardTargetEffectChancePresenter.cs
  - Assets/Cards/Constants/\*
- Related docs:
  - agent-docs/AGENTS.md
  - agent-docs/TECHNOLOGY_DOCUMENTATION.md
  - agent-docs/PROJECT_CODING_STANDARDS.md
  - agent-docs/SYSTEM_ARCHITECTURE_VISUAL.md
- Related agents or instructions:
  - .agents/skills/document-system/SKILL.md
  - .agents/skills/architecture-review/SKILL.md

## Architecture and Data Flow

- Core components:
  - Card (aggregate root) composes movement, rotation, scaling, interactions, usage, and damage modules.
  - CardsHandManager owns runtime hand collection and card spawning/discard wiring.
  - CardsHandPresenter computes visual overlap/curvature and updates card placement.
  - CardTargetingPreview resolves preview targets and maintains crosshair presenters while hovered.
- Key interfaces:
  - ICard, ICardMovement, ICardRotation, ICardScaler, ICardInteractions, ICardUsage, ICardDamage.
  - ICardsHandManager and ICardsHandPresenter.
  - External dependencies via DI: ITurnManager, IEnergyManager, ITargetsProvider, IPlayerParty, IUITransformsProvider, IPointerPositioner.
- Runtime flow:
  1. CardsHandManager.FillHand instantiates card prefab(s) and initializes each with CardConfigBaseSO.
  2. CardPresenter reads config data and binds title/description/cost/sprites.
  3. User hover/drag events in CardInteractions drive movement, scale, rotation, and sorting order changes.
  4. CardUsageArea.OnDrop identifies Card and calls CardUsage.Use.
  5. CardUsage checks energy, deducts cost, triggers attack animation, resolves a hit plan, then executes per-hit damage and per-hit effect chance, and finally discards the card.
  6. CardDamage applies status-effect-modified damage for each resolved hit target.
  7. CardTargetingPreview resolves and caches the same hit plan on hover, then aggregates per-target crosshair/effect chance UI.

### Config to Runtime Mapping

- CardConfigBaseSO:
  - Core display and gameplay fields: Title, Description, StartEnergyCost, Element, FrontGraphic.
  - Visual theme: CardElementalVisualBaseSO.
  - AttackSteps: List<CardAttackStep>.
- CardAttackStep:
  - Damage: CardDamageSO.
  - Effect: optional EffectSO.
  - EffectChance: float in range [0, 1], rolled per hit.
- CardDamageSO:
  - DamageValue, AttackCount, StartPosition, TargetMode.
  - TargetMode values are Same, All, Random.

## Rules and Invariants

- Critical behavior rules:
  - A card can only be used if current energy is at least current card cost.
  - Per-hit execution order is damage first, then effect chance roll, then effect execution.
  - Card discard destroys visual and root object immediately after use.
  - For TargetingMode.Same, one target receives AttackCount hits.
  - For TargetingMode.All, each alive target receives AttackCount hits.
  - For TargetingMode.Random, each hit selects a random alive target.
  - Random target samples resolved by hover preview are reused by usage when available.
- Ordering or sequencing guarantees:
  - Hand is filled on player turn start and discarded on player turn end.
  - Drag start disables raycast blocking on card visual; drag end restores it.
  - Target preview starts on hover start and is torn down on hover end/disable.
- Constraints contributors must preserve:
  - Keep DI-based dependencies (do not reintroduce singleton lookups).
  - Keep inspector-driven card authoring via ScriptableObjects.
  - Keep preview and execution hit-plan logic aligned to avoid misleading UI.
  - Preserve interaction state machine correctness when adding new states.

## Extension Points

- Safe extension areas:
  - New card configs under Assets/Cards/CardsLibrary with the CardConfigBaseSO AttackSteps schema.
  - New EffectSO implementations consumed by CardUsage context.
  - New targeting UI detail inside CardTargetCrosshairPresenter and effect presenters.
- Required dependencies and contracts:
  - ITargetsProvider must provide stable target ordering via GetAll for StartPosition-based resolver selection.
  - IPlayerParty should continue exposing GetAllCharacters and per-element attack animation hooks.
  - Card prefab must contain all required card subsystem components.
- Testing implications:
  - Validate hover/drag/drop behavior across fast pointer movement and rapid card interactions.
  - Validate energy edge cases (exact-cost usage, insufficient energy).
  - Validate preview data matches actual applied damage/effects for Same, All, and Random targeting modes.
  - Validate random-hit dead-target reroll behavior during usage.

## Integration Notes

- Upstream dependencies:
  - Turn events from ITurnManager.
  - Pointer and UI coordinate systems from IPointerPositioner and Unity UI.
  - Energy and target systems via IEnergyManager and ITargetsProvider.
- Downstream consumers:
  - Enemy and player damage/status systems through ITarget and damage/effect execution.
  - UI target indicator presenters.
- Cross-system coupling risks:
  - CardUsage and CardTargetingPreview both rely on CardResolvedHit planning behavior and must stay synchronized.
  - CardInteractions has direct dependency on hand presenter behavior.
  - Card aggregate relies on prefab-local GetComponent composition and required component presence.

## Known Risks and Open Questions

- Known limitations:
  - Effect target selection is bound to the hit target of each attack step; independent effect target policies are not modeled.
  - Hand fill behavior is fixed at START_CARDS_NUMBER each turn and test config random selection.
- Open design questions:
  - Should cards support effect targets that differ from step damage targets?
  - Should card visuals be pooled instead of destroyed for performance under heavy draw/discard rates?
- Suggested follow-up tasks:
  - Implement high-priority fixes first (visibility, rotation semantics, target resolver).
  - Add lightweight automated tests for usage gating, random reroll behavior, and target resolution.
  - Add a card system validation checklist for manual QA in combat scenes.
