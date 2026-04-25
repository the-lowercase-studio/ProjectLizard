# AGENTS.md - ProjectLizard

## Purpose

This file gives coding agents a concise map of ProjectLizard so they can make safe, architecture-aligned changes.

Operational customization files are maintained in .agents/.

## Documentation-First Policy

When official documentation is available for a technology used in this project, consult that documentation first.

Rules:

1. Use technology-documentation.md as the source for official docs links.
2. Prefer official docs over memory or generic model knowledge.
3. If docs and code behavior differ, trust the actual project code and ask the user to clarify intended behavior.
4. In responses and implementation notes, mention which official source was used when relevant.

## Project Snapshot

ProjectLizard is a Unity turn-based combat project centered around a card-driven player turn and enemy intention-driven actions.

Core gameplay loop:

1. Player turn starts.
2. Player spends energy and uses cards.
3. Enemy intentions are selected and displayed.
4. Enemy actions execute on enemy turn.
5. Damage is resolved with shield-first logic.
6. Turn advances and repeats.

## Architecture Overview

### Composition and Dependency Management

- The project uses dependency injection via Reflex.
- Scene-level bindings are configured through a scene installer.
- Runtime dependencies are consumed through interfaces rather than singleton globals.

Known service registration entry point:

- Assets/Installers/SceneInstaller.cs

### Turn Flow

- Turn sequencing is managed by a turn manager abstraction.
- Events drive phase transitions for player and enemy turns.
- Enemy intention selection and execution are connected to turn events.

Primary flow component:

- Assets/Turns/TurnManager.cs

### Combat Domain

- Targets implement combat interfaces for damage and effects.
- Damage resolution applies shield first, then health.
- Both player party and enemies participate in the same combat abstractions.

Representative components:

- Assets/Enemies/Base/EnemyBase.cs
- Assets/PlayerParty/PlayerParty.cs
- Assets/ShieldSystem/

### Card and Effect Pipeline

- Card usage checks energy cost before executing.
- Cards execute a step-based attack flow where each step has damage, optional effect, and per-step chance.
- Usage applies damage per resolved hit, then executes effect logic only when step chance passes.
- Target resolution is routed through target-provider abstractions.

Representative component:

- Assets/Cards/Base/Usage/CardUsage.cs

### Enemy Intention System

- Enemies choose from configurable intentions with weighted probabilities.
- Selected intention can be shown to the player through indicator UI.
- Actions are modular and extensible through action interfaces.

Representative folders:

- Assets/Enemies/Intentions/
- Assets/Enemies/Actions/
- Assets/Enemies/UI/

### Feedback and Presentation

- Damage numbers are spawned as UI feedback during combat.
- VFX and audio components are integrated into combat entities.

Representative folders:

- Assets/DamageNumbers/
- Assets/VFX/
- Assets/Audio/

## Technology Stack

### Engine and Runtime

- Unity (C# scripts, MonoBehaviour-based gameplay)
- ScriptableObject-driven configuration for gameplay data

### Patterns and Libraries

- Reflex 14.1.0 for dependency injection
- Interface-first contracts for manager/service communication
- Event-driven turn and combat flow

### UI and Visuals

- Unity UI-based runtime interfaces
- TextMeshPro appears to be available in project packages
- VFX integration for damage and combat feedback

## Data and Configuration Model

- Enemy behavior is configured via enemy config assets and intention lists.
- Action tuning is inspector-driven where possible.
- Combat values and effects are primarily data-configured, then executed by runtime logic.

## Agent Working Rules for This Repository

1. Do not reintroduce singleton access patterns where DI interfaces already exist.
2. Prefer extending existing interfaces and systems over duplicating logic.
3. Keep gameplay changes compatible with turn-event sequencing.
4. Preserve shield-first damage semantics unless explicitly asked to rebalance mechanics.
5. Keep inspector workflows intact for designers.
6. Treat docs in root summaries as high-level guidance; verify behavior in code before major changes.
7. If a mechanic is unclear, ask the user before implementing assumptions.
8. Never edit `.prefab` files directly. Any prefab changes must be done by the user in the Unity Editor.

## AI-Assisted Development Guidance

- Use AI suggestions as a draft, not as a source of truth.
- Validate gameplay logic against turn flow, shields, and intention execution.
- Follow the detailed guidance in ai-game-dev-best-practices.md for implementation, testing, and content workflows.

## Markdown Document Naming

- AI-facing project documentation under `agent-docs/` uses kebab-case markdown filenames.
- Date-prefixed implementation notes use `yyyy-mm-dd-description.md`.
- Reserved operational filenames keep their established uppercase or conventional names, including `AGENTS.md`, `README.md`, and `SKILL.md`.
- New documentation links should use the exact on-disk filename and relative path.

## Project Coding Standards

Use these standards for all new code and refactors in touched files.

1. Interface colocation: define interface and implementation in the same file, with the interface above the class.
2. Naming for constants: use UPPER_SNAKE_CASE for const fields.
3. Interface naming: prefix interfaces with I (for example ITarget, ITurnManager).
4. Member ordering in MonoBehaviour classes: [Inject] fields, then [SerializeField] fields, then private fields.
5. Private fields: use \_camelCase.
6. Public members and methods: use PascalCase.
7. Events: use OnX naming (for example OnPlayerTurnStart).
8. Namespaces: keep Assets.<Domain> style namespace hierarchy consistent with folder structure.
9. Inspector workflow: prefer [SerializeField] private fields over new public mutable fields.
10. Fixing legacy style: when editing files with older style, migrate changed lines toward these standards without unrelated large rewrites.

For full details and examples, see project-coding-standards.md.

## Known Guidance Documents for AI Agents

Use this section as a reading map before major changes. Prefer project-authored docs over package/cache docs.

Recommended reading order:

1. AGENTS.md
2. technology-documentation.md
3. ai-game-dev-best-practices.md
4. system-architecture-visual.md
5. player-party-character-system-summary.md
6. ../.agents/README.md

Project docs and when to use them:

- README.md: Entry-level project overview.
- changelog.md: Recent change history and migration context.
- refactoring-summary.md: DI migration decisions and architectural constraints.
- system-architecture-visual.md: High-level architecture and flow diagrams.
- elemental-system-summary.md: Shared element vocabulary, card/effect/party consumers, and elemental modifier boundaries.
- turns-system-summary.md: Turn sequencing, event order, and turn-event integration boundaries.
- enemy-intention-system-summary.md: Enemy intention/action design and integration notes.
- damage-numbers-system-summary.md: Damage popup feedback behavior, shield/health split display, and UI ownership boundaries.
- player-party-character-system-summary.md: Player party model, character data flow, damage/death behavior, and extension constraints.
- shield-implementation-complete.md: Shield-first combat behavior and related implementation details.
- shield-system-update.md: Follow-up shield updates and setup notes.
- ai-game-dev-best-practices.md: AI-assisted implementation, testing, and review guardrails.
- project-coding-standards.md: Project-specific code style and architecture conventions for contributors and agents.
- technology-documentation.md: Official documentation links for project technologies and usage priority guidance.

Note for agents:

- Ignore third-party or generated markdown under `Library/`, `Packages/`, `Temp/`, and similar cache/build directories unless the user explicitly asks for those sources.

## Operational Agent Layer

Use these folders for agent runtime guidance and workflow orchestration:

- ../.agents/agents/: specialist agent definitions.
- ../.agents/instructions/: file-scoped instructions with applyTo patterns.
- ../.agents/skills/: reusable workflows such as architecture-review, balance-analysis, and document-system.

Compatibility note:

- .github/ agent files are compatibility mirrors or pointers only. Edit .agents/ first.

## Open Clarifications to Confirm With User

These are intentionally listed for future refinement.

1. Target Unity editor/version used for production builds.
2. Exact rendering pipeline and package constraints expected by gameplay/UI changes.
3. Test strategy and minimum validation steps expected for agent-made edits.
4. Any strict coding style rules beyond current conventions.

## Collaboration Note

If any architecture or behavior detail here is incomplete, ask the user and update this file rather than guessing.
