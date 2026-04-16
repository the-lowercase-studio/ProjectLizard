---
applyTo: "Assets/{HealthSystem,ShieldSystem,DamageNumbers,Enemies,PlayerParty}/**/*.cs"
description: "Use when: changing health, shield, or damage resolution and related combat feedback behavior."
---

# Health and Damage Instruction

Use this instruction when modifying damage and survivability systems.

## Preserve

- Apply shield before health in all damage flows.
- Keep damage event sequencing stable for UI and VFX listeners.
- Preserve compatibility across both player party and enemy combat abstractions.

## Coordinate With

- .agents/agents/combat-damage-specialist.agent.md
- .agents/agents/ui-feedback-specialist.agent.md
- .agents/agents/performance-specialist.agent.md

## Avoid

- Do not silently change damage semantics without updating docs.
- Do not add allocation-heavy feedback loops in hot combat paths.
