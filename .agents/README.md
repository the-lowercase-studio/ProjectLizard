# ProjectLizard Agent Operations (.agents)

This folder is the vendor-neutral operational source of truth for agent customization files in this repository.

## Structure

- agents/: custom specialist agents used for orchestration and task routing.
- instructions/: file-scoped instructions with applyTo patterns.
- skills/: reusable multi-step workflows with templates.

## Discovery Order

1. Root AGENTS.md
2. agent-docs/AGENTS.md
3. .agents/README.md
4. .agents/agents/, .agents/instructions/, .agents/skills/

## Source of Truth Policy

- Edit agent operational files in .agents first.
- .github content is compatibility-only and should point to .agents.
- Keep long-form architecture and coding guidance in agent-docs/.

## Maintenance

- When introducing a new specialist domain, add/update the corresponding .agent.md file under .agents/agents/.
- Keep instruction applyTo globs narrow to avoid unnecessary context loading.
- Keep skill descriptions explicit with "Use when:" trigger phrases for better discovery.
