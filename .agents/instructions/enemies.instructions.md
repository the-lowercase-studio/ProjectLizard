---
applyTo: "Assets/Enemies/**/*.cs"
description: "Use when: editing enemy AI, intention selection, intention execution, or enemy combat behavior."
---

# Enemies Instruction

Use this instruction when modifying enemy systems.

## Preserve

- Keep enemy intention flow deterministic and event-driven.
- Keep enemy changes compatible with turn sequencing.
- Respect split ownership for EnemyBase sections between combat and intention logic.

## Coordinate With

- .agents/agents/enemy-ai-intention-specialist.agent.md
- .agents/agents/combat-damage-specialist.agent.md

## Avoid

- Do not bypass existing interfaces with singleton shortcuts.
- Do not introduce hidden coupling between enemy UI and enemy logic.
