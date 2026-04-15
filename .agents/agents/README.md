# ProjectLizard Agent Catalog

This folder contains custom agents for project-scoped specialization.

This is the canonical operational agent catalog for ProjectLizard.

## Agent list

- projectlizard-orchestrator.agent.md
- combat-damage-specialist.agent.md
- enemy-ai-intention-specialist.agent.md
- mechanics-effects-specialist.agent.md
- card-system-specialist.agent.md
- balance-specialist.agent.md
- ui-feedback-specialist.agent.md
- input-interaction-specialist.agent.md
- audio-integration-specialist.agent.md
- performance-specialist.agent.md
- testing-validation-specialist.agent.md
- documentation-standards-specialist.agent.md

## Intended usage

1. Start with ProjectLizard Orchestrator for task triage.
2. Let the orchestrator assign one lead specialist.
3. Add supporting specialists only when cross-domain changes are required.
4. Return to the orchestrator for integration checks and completion criteria.

Read this folder together with:

- ../instructions/
- ../skills/
- ../README.md

## Ownership hotspots

- EnemyBase split ownership:
  - Combat and Damage Specialist owns damage and health handling sections.
  - Enemy AI and Intention Specialist owns intention selection and execution sections.
- DamageNumbers split ownership:
  - UI and Feedback Specialist owns visuals and readability behavior.
  - Performance Specialist owns pooling and allocation optimizations.

## Suggested workflow templates

Feature work

- Orchestrator -> lead specialist -> supporting specialists -> Testing Specialist -> Documentation and Standards Specialist -> Orchestrator signoff.

Bugfix work

- Orchestrator -> responsible specialist -> Testing Specialist regression -> Orchestrator signoff.

Performance work

- Orchestrator -> Performance Specialist -> owning specialist verification -> Testing Specialist -> Orchestrator signoff.
