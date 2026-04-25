# Damage Numbers System Documentation

## Purpose

The Damage Numbers system owns transient combat damage popup feedback. It converts resolved damage events into UI popups, chooses visual appearance by damage type and threshold, projects world or UI target positions into the damage-number canvas, animates popup movement and text fade, and reports popup lifetime completion.

The system is not responsible for:

- Calculating card, enemy, or effect damage.
- Applying shield-first damage semantics.
- Mutating health, shield, status effects, or turn state.
- Owning VFX/audio hit feedback beyond the damage-number text itself.

## Reading Map

- Primary code locations:
  - `Assets/DamageNumbers/DamageNumbers2DSpawner.cs`
  - `Assets/DamageNumbers/DamageNumber2D.cs`
  - `Assets/DamageNumbers/DamageNumberAppearance.cs`
  - `Assets/DamageNumbers/Constants/DamageNumberConstants.cs`
  - `Assets/Cards/Constants/DamageNumberConstants.cs`
  - `Assets/Enemies/Base/EnemyBase.cs`
  - `Assets/PlayerParty/PlayerParty.cs`
  - `Assets/Installers/SceneInstaller.cs`
- Related docs:
  - `agent-docs/AGENTS.md`
  - `agent-docs/PROJECT_CODING_STANDARDS.md`
  - `agent-docs/CARDS_SYSTEM_SUMMARY.md`
  - `agent-docs/ENEMIES_SYSTEM_SUMMARY.md`
- Related agents or instructions:
  - `.agents/skills/document-system/SKILL.md`
  - `.agents/instructions/ui-feedback.instructions.md`
  - `.agents/instructions/healthsystem.instructions.md`
  - `.agents/agents/ui-feedback-specialist.agent.md`
  - `.agents/agents/combat-damage-specialist.agent.md`
  - `.agents/agents/performance-specialist.agent.md`

## Architecture and Data Flow

- Core components:
  - `DamageNumbers2DSpawner`: DI-bound scene service that spawns `DamageNumber2D` instances under a configured UI container.
  - `DamageNumbers2DSpawnerConfig`: immutable spawn request containing damage value, damage type, spawn pattern, and optional forced movement angle.
  - `DamageNumber2D`: popup view component that applies text appearance and owns the font-size/fade lifetime sequence.
  - `DamageNumberAppearance`: serializable appearance tuple for font size, grow multiplier, and text color.
  - `DamageNumberConstants`: domain constants for random angle spacing, movement angle ranges, and animation speeds.
- Key interfaces:
  - `IDamageNumbers2DSpawner`: consumed by combat targets for visual feedback without depending on the concrete spawner.
- Runtime flow:
  1. `SceneInstaller` binds the scene `DamageNumbers2DSpawner` as `IDamageNumbers2DSpawner`.
  2. `EnemyBase` and `PlayerParty` receive the spawner through Reflex injection.
  3. During `TakeDamage`, combat targets resolve shield-first damage locally.
  4. If shield absorbs any damage, the target spawns a shield damage number.
  5. If health takes any remaining damage, the target reduces health and spawns a health damage number.
  6. `DamageNumbers2DSpawner.SpawnAtTarget` resolves the target visual center, chooses a projection camera, converts the position into local canvas coordinates, chooses appearance thresholds, and instantiates one popup per requested count.
  7. Each `DamageNumber2D` initializes text, color, and size, then runs a DOTween sequence that grows, fades, shrinks, and invokes `OnLifeEnd`.
  8. The spawner listens for popup completion, decrements `CurrentlySpawnedObjectsCount`, destroys the popup GameObject, and emits `OnSpawnedEntityReleased`.

## Rules and Invariants

- Critical behavior rules:
  - Damage numbers are visual feedback only; health and shield mutation must stay in combat targets or their owned systems.
  - Shield damage and health damage use separate `DamageNumberType` values so each can use distinct inspector-configured thresholds.
  - Shield threshold configuration falls back to health thresholds when shield-specific thresholds are absent.
  - If both shield and health are damaged by one hit, popups are split into left and right upper-half movement angles to reduce overlap.
  - A disabled spawner, missing prefab, missing spawn container, or failed coordinate projection prevents spawning without changing combat state.
- Ordering or sequencing guarantees:
  - Shield popup spawning happens after shield reduction and before health damage handling.
  - Health popup spawning happens after `Health.DecreaseHealth`.
  - Popup lifetime completion is driven by `DamageNumber2D.OnLifeEnd`, not by turn events.
- Constraints contributors must preserve:
  - Keep damage-number dependencies injected through `IDamageNumbers2DSpawner`.
  - Keep combat state mutations out of `Assets/DamageNumbers`.
  - Preserve shield-first split semantics in both enemy and player-party damage flows.
  - Do not edit `DamageNumber.prefab` directly outside Unity Editor workflows.
  - Avoid allocation-heavy changes in hot combat paths unless pooling or profiling support is added intentionally.

## Extension Points

- Safe extension areas:
  - Add new `DamageNumberType` values when another combat feedback category needs distinct styling.
  - Add new `DamageNumberSpawnPattern` values for new movement distributions.
  - Tune appearance thresholds, colors, font sizes, movement distance, and visibility duration through the spawner inspector.
  - Add popup pooling behind `IDamageNumbers2DSpawner` without changing combat target call sites.
- Required dependencies and contracts:
  - Scene setup must assign `_spawnContainer`, `_damageNumberPrefab`, and at least one health appearance threshold.
  - Damage number prefabs must include `TextMeshProUGUI` and `CanvasGroup` references for `DamageNumber2D`.
  - Non-overlay canvases need a valid UI or world camera path for coordinate conversion.
- Testing implications:
  - Validate shield-only, health-only, and shield-plus-health damage on enemies and player party.
  - Validate missing shield thresholds fall back to health thresholds.
  - Validate popups spawn correctly for world-space targets, UI `RectTransform` targets, and missing/null target transforms.
  - Validate `CurrentlySpawnedObjectsCount` increments on spawn and decrements after popup lifetime completion.
  - Validate disabling the spawner suppresses visual feedback without blocking damage resolution.

## Integration Notes

- Upstream dependencies:
  - `EnemyBase` and `PlayerParty` decide when to request shield or health popups after damage resolution.
  - `SceneInstaller` provides the spawner binding and main camera dependency.
  - Unity UI and TextMeshPro provide rendering components.
  - DOTween provides popup font-size and alpha animation.
- Downstream consumers:
  - Combat readability depends on distinct shield/health styling and split popup movement.
  - Performance or test helpers may observe `CurrentlySpawnedObjectsCount` and `OnSpawnedEntityReleased`.
- Cross-system coupling risks:
  - The system uses two `DamageNumberConstants` classes in different namespaces: movement/randomization constants are under `Assets.DamageNumbers.Constants`, while resize/fade animation speeds are currently under `Assets.Cards.Constants`.
  - Enemy and player-party damage methods duplicate shield/health popup split logic; future semantic changes should keep both paths synchronized.
  - Popup spawning currently instantiates and destroys GameObjects for each number, so high-volume multi-hit effects can create allocation pressure.

## Known Risks and Open Questions

- Known limitations:
  - Damage number objects are not pooled despite `UnityEngine.Pool` being imported in `DamageNumbers2DSpawner`.
  - `DamageNumber2D` references animation constants from `Assets.Cards.Constants`, which makes a UI feedback component depend on a card namespace for timing values.
  - `DamageNumbers2DSpawner.Spawn` has a `count` parameter, but current combat call sites spawn one popup per shield/health portion.
  - The spawner logs an error when thresholds are missing; repeated missing setup during combat could produce noisy logs.
- Open design questions:
  - Should animation speed constants move into `Assets/DamageNumbers/Constants` to keep ownership local?
  - Should shield/health split popup creation be centralized to remove duplication between enemies and player party?
  - Should popup pooling be added before introducing larger multi-hit or area-damage encounters?
- Suggested follow-up tasks:
  - Move resize/fade constants into the DamageNumbers namespace and update `DamageNumber2D`.
  - Add a small helper for shield/health split popup requests shared by enemy and player-party damage flows.
  - Add pooling or reuse support if profiler data shows popup allocation spikes.
