# Cards ScriptableObjects From Card Sheet Generator

## Purpose

This document describes how `Assets/Cards/CardsLibrary/CardsSheet.csv` is consumed by the manual generator that creates or updates card ScriptableObjects.

The generator is implemented as a Unity Editor menu command:

- `Tools/Cards/Sync Cards From Sheet`
- `Tools/Cards/Delete Unsynced Cards`

Editor implementation files:

- sync tool: `Assets/Editor/Cards/CardSheetToScriptableObjectSync.cs`
- deletion tool: `Assets/Editor/Cards/UnsyncedCardsDeletion.cs`

Primary generated/runtime targets:

- `CardConfigBaseSO`
- `CardAttackStep` entries inside `CardConfigBaseSO.AttackSteps`
- `CardDamageSO` per reusable damage tuple
- `EffectSO` references resolved from existing shared assets

## Source File

- CSV source: `Assets/Cards/CardsLibrary/CardsSheet.csv`
- Current header:
  - `Element,Title,Description,Start Energy Cost,Attack,Front Graphic,Visual Base,Implemented,ChangeDate`

## Card Output Convention

Each imported card is created or updated at:

- `Assets/Cards/CardsLibrary/<Element>/<SanitizedTitle>/<SanitizedTitle>Config.asset`

Sanitization rules:

- Trim leading/trailing whitespace.
- Split on whitespace and punctuation.
- Remove invalid path characters.
- Join remaining tokens without separators.

Examples:

- `Flame Sword` -> `FlameSword/FlameSwordConfig.asset`
- `Great Ball of Fire` -> `GreatBallOfFire/GreatBallOfFireConfig.asset`

The importer is non-destructive:

- Existing generated assets at the target path are updated.
- Missing generated assets are created.
- Generated `CardConfigBaseSO` assets that are no longer represented by the CSV are not deleted by this tool.
- Cleanup of stale generated cards happens through the separate `Tools/Cards/Delete Unsynced Cards` tool.

## Attack Convention

Each non-empty line in the `Attack` cell becomes one `CardAttackStep`.

Supported step formats:

- `DamageValuexAttackCount_StartPosition_TargetMode_StatusEffectName`
- `DamageValuexAttackCount_StartPosition_TargetMode_StatusEffectName%ChancePercent`

Examples:

- `6x1_Start_Same_None`
- `6x1_Start_Same_Burning%50`
- `4x1_Start_Same_Burning`
  `2x2_End_Random_None`

### Tokens and Valid Values

- `DamageValue`: non-negative integer mapped to `CardDamageSO.DamageValue`
- `AttackCount`: non-negative integer mapped to `CardDamageSO.AttackCount`
- `StartPosition`: mapped to `CardDamageSO.StartPosition`
  - Supported: `Start`, `Center`, `End`
- `TargetMode`: mapped to `CardDamageSO.TargetMode`
  - Supported: `Same`, `All`, `Random`
- `StatusEffectName`: used to resolve `EffectSO` (can be a Status Effect or Instant Effect)
  - Supported examples: `None`, `Burning`, `Paralysis`, `Bleeding`, `Poisoning`, `ExtendParalysis`
- `ChancePercent`: optional integer `0..100`
  - Mapped to `CardAttackStep.EffectChance` as `0f..1f`
  - Omitted values default to `1.0`

Notes:

- `None` means `CardAttackStep.Effect = null`.
- If a chance token is present while effect is `None`, the importer ignores the chance and emits a warning.
- Conditional mechanics like "double damage if bleeding" are not represented by this format.

## Column Mapping

### `Element`

- Maps to `CardConfigBaseSO.Element`
- Must match the `Elements` enum after trimming

### `Title`

- Maps to `CardConfigBaseSO.Title`
- Also drives the generated folder and asset name after sanitization

### `Description`

- Maps to `CardConfigBaseSO.Description`
- May describe extra mechanics, but only encoded `Attack` data is imported into runtime fields

### `Start Energy Cost`

- Maps to `CardConfigBaseSO.StartEnergyCost`
- Must be numeric and within `0..9`

### `Attack`

- Maps to `CardConfigBaseSO.AttackSteps`
- Each parsed line provides:
  - `CardDamageSO`
  - optional `EffectSO`
  - `CardAttackStep.EffectChance`

### `Front Graphic`

- Maps to `CardConfigBaseSO.FrontGraphic`
- Expected as a Unity asset path
- The importer supports extensionless sheet paths and resolves the matching sprite asset in that folder

Example:

- `Assets/Cards/CardsLibrary/Fire/Images/BurningFlame`

### `Visual Base`

- Maps to `CardConfigBaseSO.ElementalVisualBase`
- Expected as a Unity asset path
- The importer supports extensionless sheet paths and resolves the matching `.asset`

Example:

- `Assets/Cards/Base/DefaultCardVisualBase`

### `Implemented`

- Pipeline metadata only
- Not written into `CardConfigBaseSO`

### `ChangeDate`

- Pipeline metadata only
- Not written into `CardConfigBaseSO`

## Shared Asset Resolution

### Damage assets

`CardDamageSO` assets are reused by value tuple:

- `DamageValue`
- `AttackCount`
- `StartPosition`
- `TargetMode`

The importer first scans existing assets in:

- `Assets/Cards/Base/Damage`

If a matching tuple already exists, that asset is reused.

If no match exists, the importer creates a new asset using:

- `Assets/Cards/Base/Damage/<DamageValue>x<AttackCount>_<StartPosition>_<TargetMode>.asset`

Example:

- `8x2_Start_Random.asset`

Compatibility note:

- Legacy compact names like `6x1ss.asset` are treated as old generated assets.
- When such an asset is reused by the importer, it is repaired and moved to the explicit naming format.

### Effect assets

Effects are never created by the importer. They are resolved from existing shared assets by searching the following locations in order:

1. `Assets/Effects/StatusEffects/<EffectName>/<EffectName>.asset`
2. `Assets/Effects/InstantEffects/<EffectName>/<EffectName>.asset`

Examples:

- `Burning` -> `Assets/Effects/StatusEffects/Burning/Burning.asset`
- `Paralysis` -> `Assets/Effects/StatusEffects/Paralysis/Paralysis.asset`
- `Bleeding` -> `Assets/Effects/StatusEffects/Bleeding/Bleeding.asset`
- `Poisoning` -> `Assets/Effects/StatusEffects/Poisoning/Poisoning.asset`
- `ExtendParalysis` -> `Assets/Effects/InstantEffects/ExtendParalysis/ExtendParalysis.asset`

If a non-`None` effect asset cannot be resolved, that row fails and the importer continues with the next row.

## Validation and Warning Rules

The importer validates:

- required fields: `Element`, `Title`, `Start Energy Cost`, `Attack`, `Front Graphic`, `Visual Base`
- `Start Energy Cost` range `0..9`
- non-negative `DamageValue` and `AttackCount`
- valid enum tokens for `Element`, `StartPosition`, and `TargetMode`
- valid `ChancePercent` range `0..100`
- resolvable assets for `Front Graphic`, `Visual Base`, and any non-`None` effect

Warnings are emitted when:

- the description appears to mention unsupported mechanics not encoded in `Attack`
- a chance token is provided for a `None` effect
- duplicate `CardDamageSO` assets already exist for the same tuple
- a new damage asset needs a unique fallback name because the convention path is already occupied

## Example

Single-step card:

- `6x1_Start_Same_None`

Single-step card with effect chance:

- `6x1_Start_Same_Burning%50`

Two-step card in one cell:

- `4x1_Start_Same_Burning`
  `2x2_End_Random_None`

This becomes two `CardAttackStep` entries inside `CardConfigBaseSO.AttackSteps`.
