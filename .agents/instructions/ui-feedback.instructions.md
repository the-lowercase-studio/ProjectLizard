---
applyTo: "Assets/{UI,DamageNumbers,Enemies/UI,VFX}/**/*.cs"
description: "Use when: improving combat readability, HUD feedback, damage number behavior, and intention visuals."
---

# UI and Feedback Instruction

Use this instruction when modifying visual feedback systems.

## Preserve

- Keep gameplay logic separate from visual-only concerns.
- Maintain readability and timing consistency for intention and damage feedback.
- Keep integrations with existing UI and VFX hooks stable.

## Coordinate With

- .agents/agents/ui-feedback-specialist.agent.md
- .agents/agents/enemy-ai-intention-specialist.agent.md

## Avoid

- Do not move gameplay state mutations into UI classes.
- Do not edit prefab assets directly outside Unity Editor workflows.
