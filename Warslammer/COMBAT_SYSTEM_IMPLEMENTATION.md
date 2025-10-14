# Combat System Implementation - Phase 3 Complete

## Overview
The dice-based combat system for Project SOVL-Like has been fully implemented. This document provides instructions for creating weapon assets and testing the combat system.

---

## Files Created

### Combat Core Classes (Assets/_Project/Scripts/Combat/)
1. **DicePool.cs** - Represents a pool of dice with modifiers
2. **DiceRoller.cs** - Performs dice rolls with visualization options
3. **CombatResolver.cs** - Main combat flow coordinator
4. **DamageCalculator.cs** - Calculates final damage after armor/modifiers
5. **ModifierStack.cs** - Manages stacked modifiers from various sources

### Combat Results
6. **CombatResult.cs** - Stores combat outcome data
7. **DiceRollResult.cs** - Individual dice roll results

### Weapon System
8. **WeaponData.cs** - ScriptableObject for weapon stats
9. **RangeCalculator.cs** - Calculates weapon range validity

### Status Effects
10. **StatusEffect.cs** - Base class for status effects (Bleed, Rooted, Shaken, Guard, etc.)
11. **StatusEffectManager.cs** - Manages status effects on units

### Morale System
12. **MoraleSystem.cs** - Handles morale checks and Shaken status

### Unit Combat
13. **UnitCombat.cs** - Unit combat actions component (in Assets/_Project/Scripts/Units/)

### Combat UI
14. **CombatLog.cs** - Scrolling text log for combat events (in Assets/_Project/Scripts/UI/)
15. **DamagePopup.cs** - Floating damage numbers (in Assets/_Project/Scripts/UI/)

---

## Creating WeaponData Assets

### Step 1: Create Weapon Assets Folder
In Unity Editor:
1. Navigate to `Assets/_Project/Data/`
2. Create a new folder called `Weapons` if it doesn't exist

### Step 2: Create Sword (Melee Weapon)
1. Right-click in `Assets/_Project/Data/Weapons/`
2. Select `Create > Warslammer > Combat > Weapon Data`
3. Name it `Weapon_Sword`
4. Set the following properties:
   - **Weapon Name**: "Sword"
   - **Range Type**: Melee
   - **Min Range Inches**: 0
   - **Max Range Inches**: 1
   - **Attack Dice**: 2
   - **To Hit Target**: 4
   - **Base Damage**: 3
   - **Armor Penetration**: 0
   - **Damage Source**: Physical
   - **Allows Criticals**: ✓ (checked)

### Step 3: Create Bow (Ranged Weapon)
1. Right-click in `Assets/_Project/Data/Weapons/`
2. Select `Create > Warslammer > Combat > Weapon Data`
3. Name it `Weapon_Bow`
4. Set the following properties:
   - **Weapon Name**: "Bow"
   - **Range Type**: Ranged
   - **Min Range Inches**: 3
   - **Max Range Inches**: 24
   - **Attack Dice**: 2
   - **To Hit Target**: 4
   - **Base Damage**: 2
   - **Armor Penetration**: 0
   - **Damage Source**: Physical
   - **Allows Criticals**: ✓ (checked)

### Step 4: Create Spear (Reach Weapon)
1. Right-click in `Assets/_Project/Data/Weapons/`
2. Select `Create > Warslammer > Combat > Weapon Data`
3. Name it `Weapon_Spear`
4. Set the following properties:
   - **Weapon Name**: "Spear"
   - **Range Type**: Melee
   - **Min Range Inches**: 0
   - **Max Range Inches**: 2
   - **Attack Dice**: 1
   - **To Hit Target**: 4
   - **Base Damage**: 2
   - **Armor Penetration**: 0
   - **Damage Source**: Physical
   - **Allows Criticals**: ✓ (checked)
   - **Has Bonus Vs Type**: ✓ (checked)
   - **Bonus Vs Unit Type**: Cavalry
   - **Bonus Damage Amount**: 1

---

## Setting Up the Combat Test Scene

### Step 1: Create Test Scene
1. In Unity, create a new scene: `File > New Scene`
2. Save it as `Assets/_Project/Scenes/Testing/CombatTest.unity`
3. Create the Testing folder if it doesn't exist

### Step 2: Set Up Scene Objects

#### Create Ground Plane
1. Right-click in Hierarchy > `3D Object > Plane`
2. Name it "Ground"
3. Reset transform (Position: 0, 0, 0)
4. Scale it to (5, 1, 5) for a larger battlefield

#### Create Combat Manager
1. Create Empty GameObject: `GameObject > Create Empty`
2. Name it "CombatManager"
3. Add components:
   - DiceRoller
   - DamageCalculator
   - RangeCalculator
   - CombatResolver
   - MoraleSystem

#### Create Battlefield Manager
1. Create Empty GameObject: `GameObject > Create Empty`
2. Name it "BattlefieldManager"
3. Add components:
   - BattlefieldManager (from existing scripts)
   - LineOfSightManager
   - TerrainManager
   - DeploymentZoneManager

### Step 3: Create Test Units

#### Create Unit 1 (Attacker)
1. Create Empty GameObject: `GameObject > Create Empty`
2. Name it "TestUnit_Attacker"
3. Position: (0, 0.5, -2)
4. Add components:
   - Unit
   - UnitStats
   - UnitMovement
   - UnitVisuals
   - UnitCombat
   - StatusEffectManager
   - CapsuleCollider (radius: 0.5)
5. Add a 3D Object > Cube as child for visualization
   - Scale: (0.5, 1, 0.5)
   - Add material, color it Red

#### Unit 1 Configuration:
- In Unit component:
   - Create or assign a UnitData ScriptableObject
   - Set Owner Player Index: 0
- In UnitCombat component:
   - Drag `Weapon_Sword` to Primary Weapon

#### Create Unit 2 (Defender)
1. Duplicate "TestUnit_Attacker"
2. Rename to "TestUnit_Defender"
3. Position: (0, 0.5, 2)
4. Change cube color to Blue
5. In Unit component:
   - Set Owner Player Index: 1
- In UnitCombat component:
   - Drag `Weapon_Bow` to Primary Weapon

### Step 4: Create UI

#### Create Canvas
1. Right-click in Hierarchy > `UI > Canvas`
2. Set Canvas to "Screen Space - Overlay"

#### Create Combat Log
1. Right-click on Canvas > `UI > Panel`
2. Name it "CombatLogPanel"
3. Position it on the left side of screen
4. Add UI > ScrollView as child
5. In the ScrollView's Content:
   - Add UI > Text component
   - Set Text alignment to Top-Left
   - Enable Rich Text
6. Add `CombatLog` component to ScrollView object
7. Assign references:
   - Log Text: the Text component in Content
   - Scroll Rect: the ScrollView's ScrollRect

#### Create Combat Buttons (Optional)
1. Right-click on Canvas > `UI > Button`
2. Name it "Button_Attack"
3. Position at bottom of screen
4. Change button text to "Attack Target"

### Step 5: Create Camera
1. Position Main Camera at: (0, 10, -5)
2. Rotate to look down at the battlefield: (60, 0, 0)

---

## Testing the Combat System

### Manual Testing (No Code Required)

#### Test 1: Basic Attack
1. Enter Play Mode
2. Select the Attacker unit in Hierarchy
3. In the Inspector, find the UnitCombat component
4. Expand the "OnAttackPerformed" event
5. Add a listener that targets Console to see results
6. Alternatively, use the Script Execution Order debug approach below

#### Test 2: Using Unity Events
Create a simple test script `CombatTestController.cs`:

```csharp
using UnityEngine;
using Warslammer.Units;
using Warslammer.Combat;

public class CombatTestController : MonoBehaviour
{
    public Unit attacker;
    public Unit defender;

    void Update()
    {
        // Press Space to attack
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformTestAttack();
        }
    }

    void PerformTestAttack()
    {
        if (attacker == null || defender == null)
        {
            Debug.LogError("Assign attacker and defender in Inspector!");
            return;
        }

        UnitCombat combat = attacker.GetComponent<UnitCombat>();
        if (combat != null)
        {
            CombatResult result = combat.Attack(defender);
            if (result != null)
            {
                Debug.Log(result.GetCombatLogString());
            }
        }
    }
}
```

Add this script to an empty GameObject and assign the test units.

### Testing Checklist

- [ ] Units spawn correctly
- [ ] Combat log displays in UI
- [ ] Attack button triggers combat
- [ ] Dice rolls show correct values
- [ ] Damage calculation works (base + successes - blocks - armor)
- [ ] Critical hits add extra successes (rolling 6)
- [ ] Defender blocks reduce damage
- [ ] Units take damage and HP updates
- [ ] Units die when HP reaches 0
- [ ] Combat log shows color-coded messages
- [ ] Morale checks trigger at 25% HP loss
- [ ] Shaken status applies on morale failure

---

## Integration with Existing Systems

The combat system integrates with:
1. **BattlefieldManager** - Gets unit lists, checks positions
2. **LineOfSightManager** - Validates LOS for ranged attacks
3. **TurnManager** - Tracks turn order
4. **PhaseManager** - Checks if combat is allowed (Action phase)
5. **Unit** - Implements ICombatant interface
6. **UnitStats** - Provides HP, armor, defense values

---

## Dice Rolling System

### How It Works:
1. **Attack Roll**: Roll weapon's ATT dice (e.g., 2d6)
2. **Success Check**: Each die ≥ target number (default 4+) = 1 success
3. **Critical**: Rolling 6 = +1 extra success + triggers Switch effects
4. **Defense Roll**: Defender rolls DEF dice to block
5. **Damage Calculation**: Base damage + attack successes - defense blocks - armor

### Example Combat:
```
Attacker: Sword (2 ATT dice, 3 base damage)
Defender: 2 DEF dice, 2 armor

Attack Roll: [5, 6] = 3 successes (6 is critical +1)
Defense Roll: [4, 2] = 1 block
Damage: 3 (base) + 3 (successes) - 1 (blocks) - 2 (armor) = 3 final damage
```

---

## Status Effects Reference

- **Bleed**: Lose 1 HP at end of turn (stackable)
- **Rooted**: Cannot move
- **Shaken**: -1 die on all rolls (from morale failure, permanent until rallied)
- **Guard**: +1 DEF die vs next attack
- **Stunned**: Cannot take actions
- **Poisoned**: Take poison damage at start of turn

---

## Morale System

### Trigger Conditions:
- Unit loses ≥25% max HP in one attack

### Morale Check:
- Roll 2d6 vs Leadership (default: 7)
- Fail = Apply Shaken status
- Shaken = -1 die penalty on all rolls

### Rally:
- Use Rally action
- Roll 2d6 vs Leadership
- Success = Remove Shaken

---

## Next Steps (Phase 4)

These systems are ready for Phase 4 expansion:
1. Ability system (Active, Passive, Reaction)
2. AI combat decision-making
3. Advanced modifiers (terrain, weather)
4. Special weapon rules (Blast, Cleave, etc.)
5. Unit formations and tactics
6. Campaign progression

---

## Troubleshooting

### Common Issues:

**"NullReferenceException: Object reference not set"**
- Ensure all singleton managers are in the scene
- Check weapon assignments in UnitCombat

**"No line of sight to target"**
- Check LineOfSightManager layer mask settings
- Ensure no colliders blocking vision

**"Target out of range"**
- Verify weapon max range is sufficient
- Check MeasurementUtility scaling (1 Unity unit = 1 inch)

**"Unit cannot attack"**
- Check CanAttackThisTurn flag
- Verify unit is not Stunned
- Confirm it's the Action phase

---

## Performance Notes

- DiceRoller uses System.Random for speed
- Combat resolution is synchronous (no coroutines)
- Status effects check only on turn start/end
- Object pooling recommended for DamagePopups (already structured)

---

## File Structure Summary

```
Assets/_Project/
├── Scripts/
│   ├── Combat/
│   │   ├── DicePool.cs
│   │   ├── DiceRoller.cs
│   │   ├── DiceRollResult.cs
│   │   ├── CombatResolver.cs
│   │   ├── CombatResult.cs
│   │   ├── DamageCalculator.cs
│   │   ├── ModifierStack.cs
│   │   ├── WeaponData.cs
│   │   ├── RangeCalculator.cs
│   │   ├── StatusEffect.cs
│   │   ├── StatusEffectManager.cs
│   │   └── MoraleSystem.cs
│   ├── Units/
│   │   └── UnitCombat.cs
│   └── UI/
│       ├── CombatLog.cs
│       └── DamagePopup.cs
├── Data/
│   └── Weapons/
│       ├── Weapon_Sword.asset
│       ├── Weapon_Bow.asset
│       └── Weapon_Spear.asset
└── Scenes/
    └── Testing/
        └── CombatTest.unity
```

---

## End of Implementation Summary

All Phase 3 combat systems are now implemented and ready for testing!