# Cards System Documentation

## Purpose

The Cards system owns the player card lifecycle during combat turns:

- Creating card instances from ScriptableObject configs.
- Presenting cards in hand and handling card interaction states (hover, drag, drop).
- Executing card usage pipeline (energy gate, attack animation trigger, damage, effects, discard).
- Showing targeting preview UI while dragging.

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
  - CardTargetingPreview resolves preview targets and maintains crosshair presenters while dragging.
- Key interfaces:
  - ICard, ICardMovement, ICardRotation, ICardScaler, ICardInteractions, ICardUsage, ICardDamage.
  - ICardsHandManager and ICardsHandPresenter.
  - External dependencies via DI: ITurnManager, IEnergyManager, ITargetsProvider, IPlayerParty, IUITransformsProvider, IPointerPositioner.
- Runtime flow:
  1. CardsHandManager.FillHand instantiates card prefab(s) and initializes each with CardConfigBaseSO.
  2. CardPresenter reads config data and binds title/description/cost/sprites.
  3. User hover/drag events in CardInteractions drive movement, scale, rotation, and sorting order changes.
  4. CardUsageArea.OnDrop identifies Card and calls CardUsage.Use.
  5. CardUsage checks energy, deducts cost, triggers attack animation, executes damage and effects, then discards the card.
  6. CardDamage resolves targets through ITargetsProvider and applies status-effect-modified damage.
  7. CardTargetingPreview mirrors usage target resolution for crosshair/effect chance UI.

### Config to Runtime Mapping

- CardConfigBaseSO:
  - Core display and gameplay fields: Title, Description, StartEnergyCost, Element, FrontGraphic.
  - Visual theme: CardElementalVisualBaseSO.
  - Damage profile: CardDamageSO (DamageValue, AttackCount, StartPosition, TargetMode).
  - Effects list: List<EffectSO>.

## Rules and Invariants

- Critical behavior rules:
  - A card can only be used if current energy is at least current card cost.
  - Damage is resolved before card effects in current usage pipeline.
  - Card discard destroys visual and root object immediately after use.
  - For TargetingMode.Same, one target receives AttackCount hits.
  - For TargetingMode.Other, up to AttackCount targets receive one hit each.
- Ordering or sequencing guarantees:
  - Hand is filled on player turn start and discarded on player turn end.
  - Drag start disables raycast blocking on card visual; drag end restores it.
  - Target preview starts on drag start and is torn down on drag end/disable.
- Constraints contributors must preserve:
  - Keep DI-based dependencies (do not reintroduce singleton lookups).
  - Keep inspector-driven card authoring via ScriptableObjects.
  - Keep preview and execution target logic aligned to avoid misleading UI.
  - Preserve interaction state machine correctness when adding new states.

## Extension Points

- Safe extension areas:
  - New card configs under Assets/Cards/CardsLibrary with existing CardConfigBaseSO schema.
  - New EffectSO implementations consumed by CardUsage context.
  - New targeting UI detail inside CardTargetCrosshairPresenter and effect presenters.
- Required dependencies and contracts:
  - ITargetsProvider must support GetFirst and GetFromStartPosition semantics used by usage and preview.
  - IPlayerParty should continue exposing GetAllCharacters and per-element attack animation hooks.
  - Card prefab must contain all required card subsystem components.
- Testing implications:
  - Validate hover/drag/drop behavior across fast pointer movement and rapid card interactions.
  - Validate energy edge cases (exact-cost usage, insufficient energy).
  - Validate preview data matches actual applied damage/effects for each targeting mode.

## Integration Notes

- Upstream dependencies:
  - Turn events from ITurnManager.
  - Pointer and UI coordinate systems from IPointerPositioner and Unity UI.
  - Energy and target systems via IEnergyManager and ITargetsProvider.
- Downstream consumers:
  - Enemy and player damage/status systems through ITarget and damage/effect execution.
  - UI target indicator presenters.
- Cross-system coupling risks:
  - CardUsage and CardTargetingPreview duplicate effect target selection assumptions.
  - CardInteractions has direct dependency on hand presenter behavior.
  - Card aggregate relies on prefab-local GetComponent composition and required component presence.

## Known Risks and Open Questions

- Known limitations:
  - Card effects currently target only GetFirst in usage/preview path; this may not match future card archetypes.
  - Hand fill behavior is fixed at START_CARDS_NUMBER each turn and test config random selection.
- Open design questions:
  - Should cards support multi-target or self-target effects independent from damage target mode?
  - Should card visuals be pooled instead of destroyed for performance under heavy draw/discard rates?
- Suggested follow-up tasks:
  - Implement high-priority fixes first (visibility, rotation semantics, target resolver).
  - Add lightweight automated tests for usage gating and target resolution.
  - Add a card system validation checklist for manual QA in combat scenes.
