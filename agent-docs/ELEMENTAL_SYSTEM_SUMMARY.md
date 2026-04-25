# Elemental System Documentation

## Purpose

The Elemental system provides the shared element vocabulary used by cards, effects, player-party characters, card visuals, card authoring tools, and modifier-style combat effects.

Primary responsibilities:

- Define the canonical `Elements` enum.
- Let card configs declare the element used for damage modifiers and matching attack animations.
- Let effect configs declare an element for identity, grouping, and future rule hooks.
- Let card presentation select an elemental visual base.
- Let editor card-sheet tools validate element tokens and place generated card assets by element.
- Provide the element input used by incoming damage modifiers such as poisoning.

The system is not currently responsible for:

- A full resistance/weakness table.
- Enemy elemental affinity.
- Shield or health damage resolution.
- Per-element damage formulas outside status-effect modifiers.
- Selecting targets based on element.
- Enforcing card visual themes against the card's element.

## Reading Map

- Primary code locations:
  - `Assets/ElementalSystem/Elements.cs`
  - `Assets/Cards/Base/CardConfigBaseSO.cs`
  - `Assets/Cards/Base/CardElementalVisualBaseSO.cs`
  - `Assets/Cards/Base/CardPresenter.cs`
  - `Assets/Cards/Base/Damage/CardDamage.cs`
  - `Assets/Cards/Base/Usage/CardUsage.cs`
  - `Assets/Cards/TargetingPreview/CardTargetingPreview.cs`
  - `Assets/Effects/Base/EffectSO.cs`
  - `Assets/Effects/StatusEffects/IncomingDamageModifier.cs`
  - `Assets/Effects/StatusEffects/Poisoning/PoisoningEffectSO.cs`
  - `Assets/Effects/StatusEffects/Poisoning/PoisoningStatusEffect.cs`
  - `Assets/PlayerParty/CharacterConfigSO.cs`
  - `Assets/PlayerParty/PartyCharacter.cs`
  - `Assets/Editor/Cards/CardSheetToScriptableObjectSync.cs`
  - `Assets/Editor/Cards/UnsyncedCardsDeletion.cs`
- Related docs:
  - `agent-docs/AGENTS.md`
  - `agent-docs/PROJECT_CODING_STANDARDS.md`
  - `agent-docs/CARDS_SYSTEM_SUMMARY.md`
  - `agent-docs/EFFECTS_SYSTEM_SUMMARY.md`
  - `agent-docs/CARDS_SCRIPTABLE_OBJECTS_FROM_CARD_SHEET_GENERATOR_SUMMARY.md`
  - `agent-docs/PLAYER_PARTY_CHARACTER_SYSTEM_SUMMARY.md`
- Related agents or instructions:
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/agents/mechanics-effects-specialist.agent.md`
  - `.agents/instructions/cards.instructions.md`

## Architecture and Data Flow

- Core components:
  - `Elements` is the shared enum. Current values are `None`, `Physical`, `Fire`, `Acid`, and `Electric`.
  - `CardConfigBaseSO.Element` declares the gameplay element for a card.
  - `CardConfigBaseSO.ElementalVisualBase` points to `CardElementalVisualBaseSO`, which provides title, description, and card background sprites.
  - `EffectSO.Element` stores the configured element for an effect asset.
  - `CharacterConfigSO.Element` declares which element a party character represents for attack-animation matching.
  - `IIncomingDamageModifier` receives the incoming card damage element and can alter damage based on it.
- Key interfaces:
  - `IIncomingDamageModifier.ModifyIncomingDamage(int incomingDamage, Elements damageElement)`.
  - `IPartyCharacter.TryPlayAttackAnimationForElement(Elements cardElement)`.
  - Cross-system contracts: `ITarget`, `IStatusEffectReceiver`, `ICardDamage`, `ICardUsage`.
- Runtime flow:
  1. A card is initialized from `CardConfigBaseSO`.
  2. `CardPresenter` reads `ElementalVisualBase` and applies card background sprites.
  3. `CardUsage.Use` checks energy, then asks party characters to play the first attack animation whose `CharacterConfigSO.Element` matches the card element.
  4. `CardUsage` resolves attack hits and calls `CardDamage.TryApplyDamage` per hit.
  5. `CardDamage` passes `_card.Config.Element` into `GetModifiedDamageByStatusEffects`.
  6. Active status effects implementing `IIncomingDamageModifier` modify the damage in receiver iteration order.
  7. `PoisoningStatusEffect` uses `PoisoningEffectSO.CompatibleElements` to choose the compatible or incompatible scaling factor.
  8. `CardTargetingPreview` uses the same modifier path to show preview damage totals using the card element.

### Authoring Flow

- Card sheet sync:
  - `CardSheetToScriptableObjectSync` reads the `Element` column.
  - The value must parse into `Elements` after trimming, case-insensitively.
  - The parsed element is written to `CardConfigBaseSO.Element`.
  - Generated cards are currently created or updated under `Assets/Cards/CardsLibrary/<Element>/<SanitizedTitle>Config.asset`.
- Unsynced card deletion:
  - `UnsyncedCardsDeletion` rebuilds expected card paths from the CSV `Element` and `Title` columns.
  - Invalid element tokens are skipped and reported as warnings.
- Visual base:
  - `Visual Base` in the card sheet resolves to a `CardElementalVisualBaseSO` asset.
  - The importer does not verify that the visual base matches the parsed `Element`; that consistency is authoring responsibility.

## Rules and Invariants

- Critical behavior rules:
  - `Elements` is the canonical shared enum. New element names must be added there before they can be used in cards, effects, characters, or CSV rows.
  - Card element is the source of truth for card damage modifier checks.
  - Elemental card visuals are data-driven by `CardElementalVisualBaseSO`, not inferred from the enum at runtime.
  - Party attack animation selection is a first-match lookup over party characters by element.
  - Effects can store their own `Element`, but card damage modifiers currently receive the card element, not the effect element.
  - Poisoning never reduces incoming damage; it returns at least the incoming value after scaling.
- Ordering or sequencing guarantees:
  - Attack animation selection happens after energy is deducted and before attack hits execute.
  - Damage modifiers run before the target's `Damageable.TakeDamage` call.
  - Multiple active `IIncomingDamageModifier` effects are applied sequentially in active-effect enumeration order.
  - Targeting preview and actual damage use the same static modifier helper.
- Constraints contributors must preserve:
  - Keep preview damage and runtime damage aligned whenever elemental modifiers change.
  - Keep element parsing compatible with CSV authoring and generated card asset paths.
  - Do not add singleton lookups for elemental rules; route changes through existing card/effect/target contracts or DI-aligned services.
  - Preserve shield-first damage semantics by keeping element modifiers upstream of `TakeDamage`, not inside shield/health internals, unless the combat model is intentionally redesigned.

## Extension Points

- Safe extension areas:
  - Add new enum values to `Elements` when new content families are needed.
  - Add `CardElementalVisualBaseSO` assets for new visual themes.
  - Add or update `CharacterConfigSO` assets so a party character can play attacks for a new element.
  - Add status effects or instant effects that read `Elements` through explicit contracts.
  - Add new `IIncomingDamageModifier` implementations for element-sensitive damage behavior.
  - Add card sheet rows using valid `Elements` enum tokens.
- Required dependencies and contracts:
  - Card damage modifiers need a target with `StatusEffectReceiver`.
  - Party animation matching requires party characters with configured `CharacterConfigSO.Element` and an animation player.
  - Card display requires a non-null `ElementalVisualBase` with the expected sprites assigned.
  - CSV-generated cards require the enum, visual base asset, and front graphic path to be valid.
- Testing implications:
  - Validate every new element in the card sheet importer and deletion tool.
  - Validate card preview damage matches damage applied during card use when modifiers are active.
  - Validate compatible and incompatible poisoning scaling for each relevant element.
  - Validate that a card element has a corresponding party character animation path when player feedback depends on it.
  - Validate new elemental visual bases in card UI prefabs.

## Integration Notes

- Upstream dependencies:
  - Card configs and CSV rows provide the card element.
  - Character configs provide party-character element identity.
  - Effect assets provide effect element metadata and, for poisoning, compatible element lists.
  - Active status effects provide modifier behavior through `IIncomingDamageModifier`.
- Downstream consumers:
  - `CardDamage` consumes card elements for damage modification.
  - `CardTargetingPreview` consumes the same modifier helper for UI predictions.
  - `CardUsage` consumes card elements for party attack animation selection.
  - `CardPresenter` consumes elemental visual bases for card presentation.
  - Editor card tools consume elements for validation and generated asset placement.
- Cross-system coupling risks:
  - Adding or renaming enum values affects serialized assets, CSV rows, generated card paths, party configs, and effect compatibility lists.
  - Damage modifier order is inherited from status-effect receiver ordering; multiple elemental modifiers can compound.
  - `EffectSO.Element` and `CardConfigBaseSO.Element` are separate fields, so effect identity and card damage element can diverge.
  - Enemy config contains commented-out elemental affinity fields, but no runtime enemy affinity behavior is active.
  - Card visual theme consistency is not enforced by code.

## Known Risks and Open Questions

- Known limitations:
  - There is no centralized elemental matchup matrix.
  - `Elements.None` exists, but most runtime behavior assumes card elements are meaningful content values.
  - Enemy elemental resistance and weakness are commented out in `EnemyConfigSO`.
  - Only poisoning currently uses element compatibility for damage scaling.
  - `EffectSO.Element` is metadata today unless individual effect implementations choose to use it.
  - Generated card path behavior in current sync code is `Assets/Cards/CardsLibrary/<Element>/<SanitizedTitle>Config.asset`; older docs may describe a nested `<SanitizedTitle>/<SanitizedTitle>Config.asset` layout.
- Open design questions:
  - Should elemental strengths and weaknesses become a first-class combat system?
  - Should enemy configs own elemental affinity, resistance, or weakness?
  - Should card visual base be automatically derived from `CardConfigBaseSO.Element`?
  - Should `EffectSO.Element` participate in modifier rules, UI grouping, or card-sheet validation?
  - Should `Elements.None` be allowed on playable cards?
- Suggested follow-up tasks:
  - Decide whether to keep the current lightweight enum-and-consumers model or introduce a dedicated elemental rule service.
  - Add validation tooling to flag cards whose element, visual base, and generated folder do not align.
  - Add scenario tests or manual QA checklist entries for poisoning scaling and preview/runtime parity.
  - Reconcile card generator documentation with the current generated asset path if the flat per-element layout is intentional.
