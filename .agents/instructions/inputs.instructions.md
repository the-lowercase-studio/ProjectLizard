---
applyTo: "Assets/{Inputs,Input,Systems,UI}/**/*.cs"
description: "Use when: changing drag and drop, hover, click, pointer targeting, and interaction safety rules."
---

# Input and Interaction Instruction

Use this instruction when modifying user interaction systems.

## Preserve

- Keep interaction flow deterministic for turn-based gameplay.
- Preserve safety checks for invalid targets and blocked interactions.
- Keep input concerns decoupled from core combat resolution logic.

## Coordinate With

- .agents/agents/input-interaction-specialist.agent.md
- .agents/agents/card-system-specialist.agent.md

## Avoid

- Do not bypass existing interaction guards.
- Do not couple low-level input handlers directly to high-level game state mutations.
