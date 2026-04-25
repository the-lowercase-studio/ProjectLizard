# Cards System Documentation

## Purpose

The Cards system owns the player card lifecycle during combat turns:

- Creating card instances from ScriptableObject configs.
- Presenting cards in hand and handling card interaction states (hover, drag, drop).
- Executing card usage pipeline (energy gate, attack animation trigger, attack-step flow, discard).
- Showing targeting preview UI on card hover.
- Supporting CSV-driven card authoring through a separate Unity Editor importer.

The system is not responsible for:

- Turn sequencing policy (owned by TurnManager).
- Energy rules beyond cost check and deduction (owned by Energy system).
- Enemy ordering/selection strategy internals (owned by TargetsProvider).
- Damage number spawn implementation (owned by DamageNumbers/UI integration).
- Effect behavior internals and status-effect lifecycle (owned by Effects system).
- Card sheet parsing/deletion implementation details beyond the runtime data contract (owned by the card sheet generator doc).

## Reading Map

- Primary code locations:
  - Assets/Cards/Base/Card.cs
  - Assets/Cards/Base/CardInteractions.cs
  - Assets/Cards/Base/CardMovement.cs
  - Assets/Cards/Base/CardRotation.cs
  - Assets/Cards/Base/CardScaler.cs
  - Assets/Cards/Base/CardPresenter.cs
  - Assets/Cards/Base/CardAttackStep.cs
  - Assets/Cards/Base/Interaction/CardDragLock.cs
  - Assets/Cards/Base/Interaction/CardInteractionStateMachine.cs
  - Assets/Cards/Base/Targeting/CardTargetResolver.cs
  - Assets/Cards/Base/Usage/CardUsage.cs
  - Assets/Cards/Base/Usage/CardUsageArea.cs
  - Assets/Cards/Base/Damage/CardDamage.cs
  - Assets/Cards/Base/Damage/CardDamagePreviewInfo.cs
  - Assets/Cards/Base/Damage/CardDamageSO.cs
  - Assets/Cards/CardsHand/CardsHandManager.cs
  - Assets/Cards/CardsHand/CardsHandPresenter.cs
  - Assets/Cards/TargetingPreview/CardTargetingPreview.cs
  - Assets/Cards/TargetingPreview/UI/CardTargetCrosshairPresenter.cs
  - Assets/Cards/TargetingPreview/UI/CardTargetEffectChancePresenter.cs
  - Assets/Cards/Constants/\*
  - Assets/Editor/Cards/CardSheetToScriptableObjectSync.cs
  - Assets/Editor/Cards/UnsyncedCardsDeletion.cs
- Related docs:
  - agent-docs/AGENTS.md
  - agent-docs/TECHNOLOGY_DOCUMENTATION.md
  - agent-docs/PROJECT_CODING_STANDARDS.md
  - agent-docs/SYSTEM_ARCHITECTURE_VISUAL.md
  - agent-docs/EFFECTS_SYSTEM_SUMMARY.md
  - agent-docs/ENERGY_SYSTEM_SUMMARY.md
  - agent-docs/DAMAGE_NUMBERS_SYSTEM_SUMMARY.md
  - agent-docs/CARDS_SCRIPTABLE_OBJECTS_FROM_CARD_SHEET_GENERATOR_SUMMARY.md
- Related agents or instructions:
  - .agents/skills/document-system/SKILL.md
  - .agents/skills/architecture-review/SKILL.md
  - .agents/instructions/cards.instructions.md
  - .agents/agents/card-system-specialist.agent.md

## Architecture and Data Flow

- Core components:
  - Card (aggregate root) composes movement, rotation, scaling, interactions, usage, and damage modules.
  - CardsHandManager owns runtime hand collection and card spawning/discard wiring.
  - CardsHandPresenter computes visual overlap/curvature and updates card placement.
  - CardInteractions owns pointer event handling and gates hover/drag/click transitions through CardInteractionStateMachine.
  - CardDragLock prevents multiple cards from entering dragged state at the same time.
  - CardTargetResolver converts CardConfigBaseSO.AttackSteps into ordered CardResolvedHit entries.
  - CardTargetingPreview resolves preview targets, caches the attack plan on the card, and maintains crosshair presenters while hovered or dragged.
- Key interfaces:
  - ICard, ICardMovement, ICardRotation, ICardScaler, ICardInteractions, ICardUsage, ICardDamage.
  - ICardTargetResolver and ICardDragLock.
  - ICardsHandManager and ICardsHandPresenter.
  - External dependencies via DI: ITurnManager, IEnergyManager, ITargetsProvider, IPlayerParty, IUITransformsProvider, IPointerPositioner.
- Runtime flow:
  1. CardsHandManager.FillHand instantiates card prefab(s) and initializes each with CardConfigBaseSO.
  2. CardPresenter reads config data and binds title/description/cost/sprites.
  3. User hover/drag events in CardInteractions drive movement, scale, rotation, and sorting order changes.
  4. CardUsageArea.OnDrop identifies Card and calls CardUsage.Use.
  5. CardUsage checks energy, deducts cost, triggers party attack animation by element, resolves or reuses a hit plan, executes per-hit damage/effect chance, and finally discards the card.
  6. CardDamage applies status-effect-modified damage for each resolved hit target.
  7. CardTargetingPreview resolves and caches the same hit plan on hover/drag preview, then aggregates per-target damage totals, hit counts, and effect chance UI.

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
- Card sheet importer:
  - The CSV at Assets/Cards/CardsLibrary/CardsSheet.csv is the source for generated CardConfigBaseSO assets.
  - Generated cards use the runtime AttackSteps schema; shared CardDamageSO assets are reused by damage tuple.
  - Full parsing, validation, and menu-command behavior is documented in CARDS_SCRIPTABLE_OBJECTS_FROM_CARD_SHEET_GENERATOR_SUMMARY.md.

## Rules and Invariants

- Critical behavior rules:
  - A card can only be used if current energy is at least current card cost.
  - Energy is spent before damage/effects execute and before the card is discarded.
  - Per-hit execution order is damage first, then effect chance roll, then effect execution.
  - Effects execute on the resolved hit target; CardEffectContext carries the target, source card object, target position, targets provider, and original configured step damage.
  - Card discard destroys visual and root object immediately after use.
  - For TargetingMode.Same, one target receives AttackCount hits.
  - For TargetingMode.All, each alive target receives AttackCount hits.
  - For TargetingMode.Random, each hit selects a random alive target.
  - Random target samples resolved by hover/drag preview are reused by usage when available.
  - If a cached random target dies before execution, usage rerolls to an alive target when possible.
- Ordering or sequencing guarantees:
  - Hand is filled on player turn start and discarded on player turn end.
  - Drag start disables raycast blocking on card visual; drag end restores it.
  - Only one card can be dragged at a time through ICardDragLock.
  - Target preview starts when hover or drag state allows it and is torn down on hover/drag end or disable.
  - Cached attack plans are cleared on card discard and when CardTargetingPreview is disabled.
- Constraints contributors must preserve:
  - Keep DI-based dependencies (do not reintroduce singleton lookups).
  - Keep inspector-driven card authoring via ScriptableObjects.
  - Keep preview and execution hit-plan logic aligned to avoid misleading UI.
  - Preserve interaction state machine correctness when adding new states.
  - Keep card sheet generated assets compatible with private serialized backing fields on ScriptableObjects.

## Extension Points

- Safe extension areas:
  - New card configs under Assets/Cards/CardsLibrary with the CardConfigBaseSO AttackSteps schema.
  - New card sheet rows using the documented Attack cell convention when content should be generated.
  - New CardDamageSO assets for reusable damage tuples.
  - New EffectSO implementations consumed by CardUsage context.
  - New targeting UI detail inside CardTargetCrosshairPresenter and effect presenters.
- Required dependencies and contracts:
  - ITargetsProvider must provide stable target ordering via GetAll for StartPosition-based resolver selection.
  - IPlayerParty should continue exposing GetAllCharacters and per-element attack animation hooks.
  - Card prefab must contain all required card subsystem components.
  - SceneInstaller must bind ICardTargetResolver and ICardDragLock for runtime card behavior.
- Testing implications:
  - Validate hover/drag/drop behavior across fast pointer movement and rapid card interactions.
  - Validate energy edge cases (exact-cost usage, insufficient energy).
  - Validate preview data matches actual applied damage/effects for Same, All, and Random targeting modes.
  - Validate random-hit dead-target reroll behavior during usage.
  - Validate CSV sync on representative card rows when changing CardConfigBaseSO, CardAttackStep, CardDamageSO, or effect asset paths.

## Integration Notes

- Upstream dependencies:
  - Turn events from ITurnManager.
  - Pointer and UI coordinate systems from IPointerPositioner and Unity UI.
  - Energy and target systems via IEnergyManager and ITargetsProvider.
  - Card authoring data from ScriptableObjects and, for generated content, the card sheet importer.
- Downstream consumers:
  - Enemy and player damage/status systems through ITarget and damage/effect execution.
  - UI target indicator presenters.
  - Effects system through CardEffectContext and EffectSO.Execute.
- Cross-system coupling risks:
  - CardUsage and CardTargetingPreview both rely on CardResolvedHit planning behavior and must stay synchronized.
  - CardInteractions has direct dependency on hand presenter behavior and the shared drag lock.
  - Card aggregate relies on prefab-local GetComponent composition and required component presence.
  - Card sheet sync writes serialized backing-field names; property renames in runtime ScriptableObjects can break importer updates.
  - Preview damage totals include active incoming-damage modifiers, so Effects and Cards changes can alter preview output together.

## Known Risks and Open Questions

- Known limitations:
  - Effect target selection is bound to the hit target of each attack step; independent effect target policies are not modeled.
  - Hand fill behavior is fixed at START_CARDS_NUMBER each turn and currently draws randomly from test configs.
  - Card visuals are instantiated and destroyed rather than pooled.
  - Card interaction and preview behavior depend on required prefab components and scene UI containers being configured correctly.
- Open design questions:
  - Should cards support effect targets that differ from step damage targets?
  - Should card visuals be pooled instead of destroyed for performance under heavy draw/discard rates?
  - Should the current test-config hand fill become a deck/draw-pile model, and where should that ownership live?
- Suggested follow-up tasks:
  - Add lightweight automated tests for usage gating, random reroll behavior, and target resolution.
  - Add a card system validation checklist for manual QA in combat scenes.
  - Add importer regression tests around Attack parsing and serialized field writes if card schema changes continue.
