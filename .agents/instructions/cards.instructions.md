---
applyTo: "Assets/Cards/**/*.cs"
description: "Use when: implementing card behavior, hand and deck flow, card targeting, or card execution rules."
---

# Cards Instruction

Use this instruction when modifying card systems.

## Preserve

- Enforce energy-cost checks before card effect execution.
- Keep target resolution routed through existing target-provider abstractions.
- Maintain deterministic card effect execution order.

## Coordinate With

- .agents/agents/card-system-specialist.agent.md
- .agents/agents/mechanics-effects-specialist.agent.md

## Avoid

- Do not duplicate card pipeline logic in ad-hoc utility classes.
- Do not hardcode target assumptions that bypass card targeting rules.
