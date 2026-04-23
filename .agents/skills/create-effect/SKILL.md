---
name: create-effect
description: "Use when: creating or implementing a new card effect or status effect in the Effects System."
---

# Create Effect Skill

Use this skill to implement a new effect in the game, guided by the Effects System documentation (`agent-docs/EFFECTS_SYSTEM_SUMMARY.md`) and the `Assets/Effects/Base/EffectSO.cs` base class.

## Trigger

- When the user asks to "create a new effect", "implement an effect", or specifically invokes this skill.

## Required Properties

The user prompt must specify or allow inference for all needed properties defined in `EffectSO.cs`:
- `EffectName` (string)
- `Description` (string)
- `TurnDuration` (int)
- `CanStackValue` (bool)
- `EffectType` (EffectType enum)
- `ExecutionState` (TurnExecutionState enum)
- The execution logic for `Execute(CardEffectContext context)`.
- If the effect is a persistent status effect, logic for its `StatusEffectBase` implementation.

## Workflow

1. **Analyze Request & Identify Missing Properties:**
   - Review the user's prompt to identify which of the required properties are provided.
   - If any properties cannot be detected, **you must propose filling in their places** based on the context of the requested effect.

2. **Draft the Implementation Plan:**
   - Define the proposed values for all properties (`EffectName`, `Description`, `TurnDuration`, `CanStackValue`, `EffectType`, `ExecutionState`).
   - Describe the planned logic for the `EffectSO` derivative (and `StatusEffectBase` if it's a persistent effect).
   - Outline any additional files that will be modified (e.g., adding to `Assets/Effects/Base/EffectType.cs`).

3. **Present and Request Acceptance (STOP HERE):**
   - **Always first show the user what effect you will implement and ask for their acceptance or refinements.**
   - Do not write any code or make file modifications until the user explicitly approves the proposed effect properties and implementation plan.

4. **Implement the Effect (After User Approval):**
   - Create the new `EffectSO` implementation script in the appropriate directory (e.g., `Assets/Effects/StatusEffects/[EffectName]/`).
   - If the effect is persistent, create the corresponding `StatusEffectBase` implementation.
   - Update `Assets/Effects/Base/EffectType.cs` to include the new enum value.
   - Remind the user to update Unity assets like `EffectTypeSpriteMappingSO` and create the actual `ScriptableObject` instances in the Unity Editor.
   - Ensure the implementation follows the guidelines in `agent-docs/EFFECTS_SYSTEM_SUMMARY.md` (e.g., chance gating should be in `CardAttackStep`, correct expiration handling, etc.).

5. **Document the Implementation:**
   - When you end implementation, you must create a summary of the effect implementation in `agent-docs/implementation-summaries/effects/`.
