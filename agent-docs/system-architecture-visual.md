# Enemy Intention System - Visual Architecture

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Turn Manager                           │
│  ┌───────────────────┐        ┌─────────────────────┐      │
│  │ OnPlayerTurnStart │───────→│  OnEnemyTurnEnd     │      │
│  └───────────────────┘        └─────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
           ↓                               ↓
           ↓                               ↓
┌──────────────────────────┐    ┌──────────────────────────┐
│    SelectIntention()     │    │   ExecuteIntention()     │
│                          │    │                          │
│  ┌────────────────────┐ │    │  ┌────────────────────┐ │
│  │ IntentionSelector  │ │    │  │  IEnemyAction      │ │
│  │  • Weighted Random │ │    │  │   • Execute(enemy) │ │
│  │  • Based on %      │ │    │  └────────────────────┘ │
│  └────────────────────┘ │    │           ↓              │
│           ↓              │    │  ┌────────────────────┐ │
│  ┌────────────────────┐ │    │  │ AttackAction       │ │
│  │ IntentionConfig    │ │    │  │ DefenseAction      │ │
│  │  • Type            │ │    │  │ SpecialAction      │ │
│  │  • Probability     │ │    │  │ CustomActions...   │ │
│  │  • Action          │ │    │  └────────────────────┘ │
│  └────────────────────┘ │    └──────────────────────────┘
└──────────────────────────┘
```

## Component Relationships

```
┌────────────────────────────────────────────────────────────┐
│                        EnemyBase                           │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  • Health                                            │ │
│  │  • Config (EnemyConfigSO)                           │ │
│  │  • IntentionSelector                                │ │
│  │  • CurrentIntention                                 │ │
│  │  • IntentionIndicator (optional)                    │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
                    ↓                        ↓
        ┌──────────────────────┐   ┌──────────────────────┐
        │   EnemyConfigSO      │   │ IntentionIndicator   │
        │  ┌────────────────┐  │   │  ┌────────────────┐  │
        │  │ Name           │  │   │  │ Show/Hide      │  │
        │  │ Health         │  │   │  │ Icons          │  │
        │  │ BaseDamage     │  │   │  │ Colors         │  │
        │  │ Sprite         │  │   │  │ Text           │  │
        │  │ Intentions []  │  │   │  └────────────────┘  │
        │  └────────────────┘  │   └──────────────────────┘
        └──────────────────────┘
                    ↓
        ┌──────────────────────┐
        │  IntentionConfig []  │
        │  ┌────────────────┐  │
        │  │ Config 1       │  │
        │  │  • Attack 50%  │  │
        │  │  • Action →    │  │
        │  ├────────────────┤  │
        │  │ Config 2       │  │
        │  │  • Defense 30% │  │
        │  │  • Action →    │  │
        │  ├────────────────┤  │
        │  │ Config 3       │  │
        │  │  • Special 20% │  │
        │  │  • Action →    │  │
        │  └────────────────┘  │
        └──────────────────────┘
```

## Execution Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    GAME TURN CYCLE                          │
└─────────────────────────────────────────────────────────────┘

1. PLAYER TURN START
   ┌──────────────────────────────────────────────────┐
   │ For each alive enemy:                            │
   │   ├─→ Calculate total probability weight         │
   │   ├─→ Generate random number (0 to total)        │
   │   ├─→ Select intention based on weight           │
   │   ├─→ Store in _currentIntention                 │
   │   └─→ Show visual indicator (if available)       │
   └──────────────────────────────────────────────────┘
                         ↓
   ┌──────────────────────────────────────────────────┐
   │ Player takes their turn                          │
   │ - Plays cards                                    │
   │ - Uses abilities                                 │
   │ - Sees enemy intentions                          │
   │ - Makes strategic decisions                      │
   └──────────────────────────────────────────────────┘
                         ↓
2. PLAYER TURN END
   ┌──────────────────────────────────────────────────┐
   │ Transition to enemy turn                         │
   └──────────────────────────────────────────────────┘
                         ↓
3. ENEMY TURN END
   ┌──────────────────────────────────────────────────┐
   │ For each alive enemy:                            │
   │   ├─→ Get _currentIntention                      │
   │   ├─→ Execute associated action                  │
   │   ├─→ Apply effects (damage/heal/buff/etc)       │
   │   ├─→ Clear _currentIntention                    │
   │   └─→ Hide visual indicator                      │
   └──────────────────────────────────────────────────┘
                         ↓
4. NEXT TURN
   ┌──────────────────────────────────────────────────┐
   │ Increment turn counter                           │
   │ Process status effects                           │
   │ Return to step 1                                 │
   └──────────────────────────────────────────────────┘
```

## Action Type Decision Tree

```
                    Enemy selects intention
                            │
        ┌───────────────────┼───────────────────┐
        ↓                   ↓                   ↓
    ATTACK              DEFENSE             SPECIAL
    (Offensive)         (Defensive)         (Unique)
        │                   │                   │
        ↓                   ↓                   ↓
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ AttackAction │    │DefenseAction │    │SpecialAction │
├──────────────┤    ├──────────────┤    ├──────────────┤
│• Find target │    │• Check alive │    │• Find targets│
│• Deal damage │    │• Heal self   │    │• High damage │
│• Play VFX    │    │• Add shield  │    │• AoE option  │
└──────────────┘    └──────────────┘    └──────────────┘
```

## Configuration Example: Balanced Enemy

```
Enemy Config: "Goblin Warrior"
├─ Health: 50
├─ Base Damage: 10
└─ Intentions:
    ├─ [0] Attack (50%)
    │   └─→ AttackAction(damage: 12)
    │       • 50% chance
    │       • Deals 12 damage to random player
    │
    ├─ [1] Defense (30%)
    │   └─→ DefenseAction(heal: 8)
    │       • 30% chance
    │       • Heals self for 8 HP
    │
    └─ [2] Special (20%)
        └─→ SpecialAction(damage: 20, aoe: false)
            • 20% chance
            • Deals 20 damage to random player

Probability Calculation:
    Total Weight: 50 + 30 + 20 = 100
    Random(0-99):
        0-49  → Attack  (50%)
        50-79 → Defense (30%)
        80-99 → Special (20%)
```

## Data Flow Diagram

```
┌───────────────┐      ┌──────────────┐      ┌─────────────┐
│ Unity Editor  │─────→│ EnemyConfigSO│─────→│  EnemyBase  │
│  (Inspector)  │      │  (ScriptableO)│      │ (MonoBehaviour)│
└───────────────┘      └──────────────┘      └─────────────┘
       │                      │                      │
       │ Configure            │ Read at              │ Execute at
       │ Intentions           │ Runtime              │ Runtime
       ↓                      ↓                      ↓
┌─────────────────────────────────────────────────────────┐
│              IntentionConfig Data                       │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Type: Attack | Probability: 50 | Action: → → →  │   │
│  └─────────────────────────────────────────────────┘   │
│                                           ↓             │
│                               ┌────────────────────┐    │
│                               │  AttackAction      │    │
│                               │  • damage: 15      │    │
│                               └────────────────────┘    │
└─────────────────────────────────────────────────────────┘
                                    ↓
                        ┌───────────────────────┐
                        │  Action Execution     │
                        │  • Find targets       │
                        │  • Calculate damage   │
                        │  • Apply effects      │
                        │  • Log results        │
                        └───────────────────────┘
```

## Class Hierarchy

```
┌─────────────────────────────────────────────────────┐
│                  IEnemyAction                       │
│                   (Interface)                       │
│  ┌───────────────────────────────────────────────┐ │
│  │ void Execute(EnemyBase enemy)                │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
                     ↑
        ┌────────────┼────────────┬──────────────┐
        │            │            │              │
┌───────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐
│  Attack   │ │ Defense  │ │ Special  │ │   Custom     │
│  Action   │ │ Action   │ │ Action   │ │   Actions... │
└───────────┘ └──────────┘ └──────────┘ └──────────────┘
```

## Inspector Layout

```
┌─────────────────────────────────────────────────────┐
│ Enemy Config SO                                     │
├─────────────────────────────────────────────────────┤
│ Name: "Goblin Warrior"                              │
│ Description: "A fierce goblin fighter..."           │
├─────────────────────────────────────────────────────┤
│ Max Health: 50                                      │
│ Base Damage: 10                                     │
├─────────────────────────────────────────────────────┤
│ Sprite: [Goblin_Sprite]                             │
├─────────────────────────────────────────────────────┤
│ ▼ Intentions & Actions                              │
│   ┌───────────────────────────────────────────────┐ │
│   │ Size: 3                                       │ │
│   ├───────────────────────────────────────────────┤ │
│   │ Element 0                                     │ │
│   │   Intention Type: Attack                      │ │
│   │   Probability: 50                             │ │
│   │   Action: AttackAction                        │ │
│   │     Damage Amount: 12                         │ │
│   ├───────────────────────────────────────────────┤ │
│   │ Element 1                                     │ │
│   │   Intention Type: Defense                     │ │
│   │   Probability: 30                             │ │
│   │   Action: DefenseAction                       │ │
│   │     Heal Amount: 8                            │ │
│   ├───────────────────────────────────────────────┤ │
│   │ Element 2                                     │ │
│   │   Intention Type: Special                     │ │
│   │   Probability: 20                             │ │
│   │   Action: SpecialAction                       │ │
│   │     Damage Amount: 20                         │ │
│   │     Is AoE: false                             │ │
│   └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```
