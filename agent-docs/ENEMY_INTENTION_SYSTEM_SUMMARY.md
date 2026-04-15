# Enemy Intention System - Implementation Summary

## What Was Created

A complete, modular enemy intention and action system has been implemented for your Unity game. The system allows enemies to choose and execute actions based on configurable probabilities.

## New Files Created

### Core System
1. **Assets/Enemies/Intentions/IntentionType.cs** - Enum defining intention types (Attack, Defense, Special)
2. **Assets/Enemies/Intentions/IEnemyAction.cs** - Interface for all enemy actions
3. **Assets/Enemies/Intentions/IntentionConfig.cs** - Configuration class for intentions with probability and action
4. **Assets/Enemies/Intentions/IntentionSelector.cs** - Handles weighted random selection of intentions

### Built-in Actions
5. **Assets/Enemies/Actions/AttackAction.cs** - Basic attack action that deals damage
6. **Assets/Enemies/Actions/DefenseAction.cs** - Defense action that heals the enemy
7. **Assets/Enemies/Actions/SpecialAction.cs** - Special action with enhanced damage and optional AoE
8. **Assets/Enemies/Actions/BerserkAttackAction.cs** - Example custom action with complex logic

### UI Components
9. **Assets/Enemies/UI/IntentionIndicator.cs** - Visual indicator to show enemy's current intention

### Editor Tools
10. **Assets/Editor/Enemies/IntentionConfigDrawer.cs** - Custom property drawer for better Inspector UI

### Documentation
11. **Assets/Enemies/Intentions/README.md** - Comprehensive documentation
12. **Assets/Enemies/QUICK_SETUP_GUIDE.md** - Quick start guide with examples

## Modified Files

### Assets/Enemies/Base/EnemyConfigSO.cs
- Added `List<IntentionConfig> Intentions` field
- Allows configuring intentions per enemy type in the Inspector

### Assets/Enemies/Base/EnemyBase.cs
- Added intention selection logic (triggered on player turn start)
- Added intention execution logic (triggered on enemy turn end)
- Integrated with TurnManager events
- Added support for IntentionIndicator component
- Added public `CurrentIntention` property for external access

## How It Works

### Flow Diagram
```
Player Turn Start
    ↓
Enemy.SelectIntention()
    ↓
[Weighted Random Selection based on probabilities]
    ↓
Intention Selected & Indicator Shown
    ↓
... Player takes their turn ...
    ↓
Enemy Turn End
    ↓
Enemy.ExecuteIntention()
    ↓
Action.Execute() called
    ↓
Indicator Hidden
    ↓
Next Turn...
```

### Key Features

✅ **Configurable per Enemy Type** - Each EnemyConfigSO can have different intentions
✅ **Probability-Based Selection** - Weighted random selection using configurable probabilities
✅ **Modular Actions** - Easy to add new custom actions by implementing IEnemyAction
✅ **Optional Visual Feedback** - IntentionIndicator shows players what enemies will do
✅ **Turn-Based Integration** - Automatically hooks into existing TurnManager
✅ **Flexible Configuration** - Not all enemies need all action types
✅ **Zero Code Setup** - Configure entirely through Unity Inspector

## Usage Example

### In Unity Inspector (EnemyConfigSO):
```
Intentions & Actions:
  Intention 0:
    Intention Type: Attack
    Probability: 50
    Action: AttackAction
      - Damage Amount: 15

  Intention 1:
    Intention Type: Defense
    Probability: 30
    Action: DefenseAction
      - Heal Amount: 10

  Intention 2:
    Intention Type: Special
    Probability: 20
    Action: SpecialAction
      - Damage Amount: 25
      - Is AoE: false
```

### In Code (Creating Custom Action):
```csharp
using Assets.Enemies.Intentions;
using System;
using UnityEngine;

[Serializable]
public class MyCustomAction : IEnemyAction
{
    [SerializeField] private int _myParameter;

    public void Execute(EnemyBase enemy)
    {
        // Your custom logic here
        Debug.Log($"{enemy.Name} executes custom action!");
    }
}
```

## Integration Points

### With Existing Systems
- **TurnManager**: Subscribes to `OnPlayerTurnStart` and `OnEnemyTurnEnd` events
- **Health System**: Uses existing `IHealth` interface for healing actions
- **Damage System**: Uses existing `IDamageable` interface for attack actions
- **Target System**: Uses existing `ITarget` interface for target selection

### Extensibility Points
- **Custom Actions**: Implement `IEnemyAction` interface
- **Custom Selection Logic**: Extend or replace `IntentionSelector`
- **Custom UI**: Extend or replace `IntentionIndicator`
- **Validation**: Add `CanExecute()` method to actions for conditional execution

## Testing Checklist

✅ Project builds successfully
✅ All new files compile without errors
✅ EnemyBase integrates with TurnManager
✅ Intentions can be configured in Inspector
✅ Actions execute during enemy turn

## Next Steps (Optional Enhancements)

1. **Add UI Icons**: Create sprites for attack, defense, and special icons for IntentionIndicator
2. **Add Sound Effects**: Play audio when intentions are selected or executed
3. **Add Animations**: Trigger animations based on action type
4. **Add Status Effects**: Create actions that apply buffs/debuffs
5. **Add Targeting Logic**: Implement smart target selection (lowest HP, highest threat, etc.)
6. **Add Conditional Actions**: Implement actions that only execute under certain conditions
7. **Add Action Chains**: Allow multiple actions to be executed in sequence
8. **Add Action Cooldowns**: Prevent certain actions from being used every turn

## Support

For detailed documentation, see:
- **Assets/Enemies/Intentions/README.md** - Full system documentation
- **Assets/Enemies/QUICK_SETUP_GUIDE.md** - Quick setup guide with examples

For custom implementations, examine:
- **Assets/Enemies/Actions/BerserkAttackAction.cs** - Example of a complex custom action
