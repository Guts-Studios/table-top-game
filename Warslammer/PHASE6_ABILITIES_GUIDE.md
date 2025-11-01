# Phase 6: Unit Abilities & Commander System - Implementation Guide

## Overview
Phase 6 implements a flexible ability system with 6 basic spells and a commander aura system that provides passive buffs to nearby units.

**Status:** ✅ Complete (Core systems implemented, ready for testing)

---

## Files Created

### Core Ability System
1. **AbilityController.cs** (`Assets/_Project/Scripts/Units/`)
   - Manages ability usage, cooldowns, and action points
   - Validates targeting and range
   - Executes ability effects on targets
   - Integrates with existing combat systems

2. **CommanderAura.cs** (`Assets/_Project/Scripts/Units/`)
   - Provides passive buffs to nearby allied units
   - Supports stat bonuses (Attack, Defense, Armor, Movement)
   - Visual feedback with gizmo sphere
   - Automatic buff application/removal

### Editor Tools
3. **CreateAbilityAssets.cs** (`Assets/_Project/Scripts/Editor/`)
   - Menu item: **Warslammer > Setup > Create Default Abilities**
   - Automatically creates 6 basic spell assets

### Data Assets (Created via Editor Script)
4. **Ability_Fireball.asset** - AOE damage nuke
5. **Ability_Shield.asset** - Defense/armor buff
6. **Ability_Slow.asset** - Movement debuff
7. **Ability_Heal.asset** - HP restoration
8. **Ability_Summon.asset** - Unit summoning (placeholder)
9. **Ability_Wall.asset** - Create barrier (placeholder)

---

## Architecture

### Ability System Flow

```
AbilityData (ScriptableObject)
    ↓
AbilityController (Component on Unit)
    ↓
Validates:
  - Action Points
  - Cooldown
  - Range
  - Line of Sight
  - Target validity
    ↓
Executes:
  - Apply damage/healing
  - Apply stat modifiers
  - Apply status effects
  - Spawn VFX
  - Play sounds
    ↓
Updates:
  - Cooldowns
  - Action points
  - Events (OnAbilityUsed)
```

### Commander Aura Flow

```
CommanderAura (Component on Commander Unit)
    ↓
Update Loop (0.5s interval):
  - Find all units in radius
  - Check faction matching
  - Compare with previous frame
    ↓
For units entering aura:
  - Apply stat bonuses
  - Grant special effects
    ↓
For units leaving aura:
  - Remove stat bonuses
  - Remove special effects
```

---

## Using the Ability System

### Step 1: Create Ability Assets

**Option A: Use Editor Script (Recommended)**
1. In Unity, go to **Warslammer > Setup > Create Default Abilities**
2. This creates all 6 basic abilities in `Assets/_Project/Data/Abilities/`

**Option B: Create Manually**
1. Right-click in `Assets/_Project/Data/Abilities/`
2. Select **Create > Warslammer > Ability Data**
3. Configure the ability properties in the Inspector

### Step 2: Add AbilityController to Units

**In Editor:**
1. Select a unit GameObject
2. Add Component > **Ability Controller**
3. Configure:
   - **Max Abilities**: How many abilities this unit can have (default: 4)
   - **Max Action Points**: Action points per turn (default: 1)

**Via Script:**
```csharp
var abilityController = unit.gameObject.AddComponent<AbilityController>();
```

### Step 3: Assign Abilities to Units

**In Editor:**
1. Select the unit
2. In **Ability Controller** component, expand **Abilities** list
3. Set size to desired number
4. Drag ability assets into the slots

**Via Script:**
```csharp
AbilityData fireball = // Load from Resources or assign reference
unit.AbilityController.AddAbility(fireball);
```

### Step 4: Use Abilities in Code

```csharp
// Get unit's ability controller
AbilityController controller = unit.AbilityController;

// Get specific ability
AbilityData fireball = controller.GetAbilityByID("fireball");

// Check if can use
if (controller.CanUseAbility(fireball, targetUnit, out string failureReason))
{
    // Use the ability
    controller.UseAbility(fireball, targetUnit);
}
else
{
    Debug.Log($"Cannot use ability: {failureReason}");
}
```

---

## 6 Basic Spells Reference

### 1. Fireball (Nuke)
- **Type:** Active, Action phase
- **Range:** 12 inches (Ranged)
- **AOE:** 3 inch radius
- **Effect:** 5 Fire damage + 1" knockback
- **Cooldown:** 3 turns
- **Cost:** 1 action point
- **Special:** Cannot be used in melee, ends activation

### 2. Shield
- **Type:** Active, Action phase
- **Range:** 6 inches (Ranged, targets allies)
- **Duration:** 2 turns
- **Effect:** +3 Defense, +2 Armor
- **Cooldown:** 4 turns
- **Cost:** 1 action point
- **AI:** Uses when target below 50% HP

### 3. Slow
- **Type:** Active, Action phase
- **Range:** 12 inches (Ranged, targets enemies)
- **Duration:** 3 turns
- **Effect:** -3" Movement
- **Cooldown:** 2 turns
- **Cost:** 1 action point

### 4. Heal
- **Type:** Active, Action phase
- **Range:** 6 inches (Ranged, targets allies)
- **Effect:** Restore 6 HP
- **Cooldown:** 1 turn
- **Cost:** 1 action point
- **AI:** High priority (9), uses when ally below 50% HP

### 5. Summon Minion (Placeholder)
- **Type:** Active, Action phase
- **Range:** Self
- **Effect:** Would summon a unit (not implemented yet)
- **Cooldown:** 5 turns
- **Cost:** 2 action points
- **Note:** Logic for spawning units not in Phase 6 scope

### 6. Stone Wall (Placeholder)
- **Type:** Active, Action phase
- **Range:** 6 inches
- **Duration:** 3 turns
- **Effect:** Would create barrier (not implemented yet)
- **Cooldown:** 4 turns
- **Cost:** 1 action point
- **Note:** Logic for spawning obstacles not in Phase 6 scope

---

## Commander Aura System

### Adding a Commander Aura

1. Select a commander unit GameObject
2. Add Component > **Commander Aura**
3. Configure aura properties:

```
Aura Configuration:
- Aura Radius: 6" (range of buff effect)
- Same Factions Only: ✓ (only buff allies)

Stat Bonuses:
- Attack Bonus: +1
- Defense Bonus: +1
- Armor Bonus: 0
- Movement Bonus: 0"

Special Effects:
- Provides Reroll Morale: ✓
- Provides Ignore First Damage: ☐
- Provides Extra Action: ☐

Visual Feedback:
- Aura Color: Gold (adjustable)
- Show Aura Visualization: ✓
```

### How Auras Work

**Automatic Updates:**
- Aura checks nearby units every 0.5 seconds
- When a unit enters aura range, buffs are applied
- When a unit leaves aura range, buffs are removed
- Buffs are removed if commander dies

**Stacking:**
- Multiple auras can stack (add bonuses together)
- Same aura from same commander doesn't stack

**Visual Feedback:**
- Yellow sphere shows aura radius in Scene view
- Lines connect commander to buffed units (when selected)
- Gizmos visible in Play mode (configurable)

---

## Integration with Existing Systems

### Turn Management
The AbilityController integrates with the existing turn system:

```csharp
// In Unit.cs OnTurnStart():
_abilityController?.OnTurnStart();  // Resets action points, ticks cooldowns

// In Unit.cs OnTurnEnd():
_abilityController?.OnTurnEnd();    // Cleanup/per-turn effects
```

### Combat System
Abilities use the same damage/healing systems as weapons:
- `Unit.TakeDamage()` for damage abilities
- `Unit.Heal()` for healing abilities
- `StatusEffectManager` for debuffs/buffs

### Status Effects
Abilities can apply existing status effects:
- Stun (via `StatusEffectManager.ApplyStun()`)
- Poison (via `StatusEffectManager.ApplyPoison()`)
- Custom stat modifiers (via `UnitStats.AddXModifier()`)

### Line of Sight
Abilities respect LOS:
- Uses `LineOfSightManager.HasLineOfSight()`
- Can be disabled per ability with `requiresLineOfSight = false`

---

## Testing the Ability System

### Creating a Test Scene

1. **Create a new scene** or use existing CombatTest scene
2. **Add AbilityController** to test units
3. **Assign abilities** to units
4. **Create a commander** with CommanderAura component

### Testing Abilities via Code

Create a simple test script:

```csharp
using UnityEngine;
using Warslammer.Units;
using Warslammer.Data;

public class AbilityTester : MonoBehaviour
{
    public Unit caster;
    public Unit target;
    public AbilityData abilityToTest;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestAbility();
        }
    }

    void TestAbility()
    {
        if (caster == null || caster.AbilityController == null)
        {
            Debug.LogError("Caster not set up!");
            return;
        }

        bool success = caster.AbilityController.UseAbility(abilityToTest, target);
        Debug.Log($"Ability used: {success}");
    }
}
```

### Testing Commander Aura

1. Create a commander with `CommanderAura` component
2. Create several allied units around the commander
3. Enter Play mode
4. Select the commander in Hierarchy
5. In Scene view, you'll see:
   - Yellow sphere showing aura radius
   - Lines to units receiving buffs
6. Move units in/out of range to see buffs apply/remove

---

## API Reference

### AbilityController

**Key Methods:**
```csharp
// Ability Management
bool AddAbility(AbilityData ability)
bool RemoveAbility(AbilityData ability)
bool HasAbility(AbilityData ability)
AbilityData GetAbilityByID(string abilityID)

// Ability Usage
bool UseAbility(AbilityData ability, Unit target = null)
bool CanUseAbility(AbilityData ability, Unit target, out string failureReason)

// Cooldowns
bool IsAbilityOnCooldown(AbilityData ability)
int GetAbilityCooldown(AbilityData ability)
void TickCooldowns()

// Action Points
void ResetActionPoints()
bool ConsumeActionPoints(int amount)
void AddActionPoints(int amount)
```

**Properties:**
```csharp
List<AbilityData> Abilities { get; }
int ActionPoints { get; }
int MaxActionPoints { get; }
```

**Events:**
```csharp
UnityEvent<AbilityData, Unit> OnAbilityUsed
UnityEvent<AbilityData, string> OnAbilityCastFailed
```

### CommanderAura

**Key Methods:**
```csharp
bool IsUnitInAura(Unit unit)
bool CanRerollMorale(Unit unit)
bool HasIgnoreFirstDamage(Unit unit)
List<Unit> GetUnitsInAura()

void SetAuraRadius(float radius)
void SetAttackBonus(int bonus)
void SetDefenseBonus(int bonus)
void SetArmorBonus(int bonus)
void SetMovementBonus(float bonus)
```

**Properties:**
```csharp
float AuraRadius { get; }
int AttackBonus { get; }
int DefenseBonus { get; }
int ArmorBonus { get; }
float MovementBonus { get; }
int UnitsInAura { get; }
```

---

## Extending the System

### Creating Custom Abilities

1. Create a new AbilityData asset
2. Configure via Inspector or script:

```csharp
AbilityData customAbility = ScriptableObject.CreateInstance<AbilityData>();
customAbility.abilityName = "Lightning Bolt";
customAbility.abilityID = "lightning_bolt";
customAbility.damage = 8;
customAbility.damageType = DamageSource.Lightning;
customAbility.range = 18f;
customAbility.cooldownTurns = 2;
// ... configure other properties
AssetDatabase.CreateAsset(customAbility, "Assets/_Project/Data/Abilities/Ability_Lightning.asset");
```

### Custom Ability Logic

For abilities that need special logic beyond the standard effects, you can:

1. **Extend AbilityController:**
```csharp
public class CustomAbilityController : AbilityController
{
    protected override bool ExecuteAbility(AbilityData ability, Unit target)
    {
        if (ability.abilityID == "custom_spell")
        {
            // Custom logic here
            return true;
        }

        return base.ExecuteAbility(ability, target);
    }
}
```

2. **Use Special Rules field:**
   - The `specialRules` string field can be parsed for custom behavior
   - Example: "SUMMON:Goblin" could spawn a goblin unit

3. **Event-based approach:**
```csharp
void Start()
{
    abilityController.OnAbilityUsed.AddListener(HandleAbilityUsed);
}

void HandleAbilityUsed(AbilityData ability, Unit target)
{
    if (ability.abilityID == "teleport")
    {
        // Custom teleport logic
    }
}
```

---

## Performance Considerations

**Commander Aura:**
- Updates every 0.5 seconds (configurable via `_updateInterval`)
- Uses HashSet for O(1) lookups
- Only iterates through units once per update
- Automatically cleans up when commander dies

**Ability System:**
- Cooldown tracking uses Dictionary (O(1) lookups)
- Validation happens before execution (fail fast)
- VFX spawning only when prefab is assigned

---

## Known Limitations & Future Work

### Phase 6 Scope
✅ **Implemented:**
- Ability execution system
- Targeting validation
- Cooldown management
- Action point system
- AOE abilities
- Stat modifier abilities
- Commander auras
- 6 basic spell types

❌ **Not Implemented (Future Phases):**
- Unit summoning logic (Summon spell is placeholder)
- Obstacle spawning (Wall spell is placeholder)
- Reaction abilities (triggers)
- Passive abilities (always-on effects)
- Ability upgrades/leveling
- Combo system (chaining abilities)
- Resource system beyond action points (mana, energy)

### Placeholder Abilities
Two abilities are marked as placeholders:
- **Summon Minion**: Would require unit spawning system
- **Stone Wall**: Would require obstacle/terrain modification system

These can be fully implemented in future phases when those systems are added.

---

## Troubleshooting

### "Cannot use ability: Not enough action points"
- Check unit's `MaxActionPoints` in AbilityController
- Ensure `OnTurnStart()` is being called to reset points
- Verify ability's `actionPointCost` is <= max action points

### "Cannot use ability: Target out of range"
- Check ability's `range` property
- Verify distance calculation (1 Unity unit = 1 inch)
- Ensure target isn't moving during cast

### "Cannot use ability: Ability on cooldown"
- Check cooldown with `GetAbilityCooldown(ability)`
- Ensure `TickCooldowns()` is called each turn
- Verify `OnTurnStart()` integration

### Aura not affecting units
- Check `_auraRadius` matches desired range
- Verify `_sameFactionsOnly` setting
- Ensure units have `UnitStats` component
- Check `_updateInterval` (may be slow to update)

### Buffs not removing when leaving aura
- Verify `OnDisable()` and `OnDestroy()` are called
- Check that units aren't destroyed before buff removal
- Ensure commander isn't being destroyed improperly

---

## Integration Checklist

When adding abilities to your units:

- [ ] Unit has `AbilityController` component
- [ ] Abilities are assigned in Inspector or via `AddAbility()`
- [ ] `MaxActionPoints` is set appropriately
- [ ] Unit's `OnTurnStart()` calls `abilityController.OnTurnStart()`
- [ ] Unit's `OnTurnEnd()` calls `abilityController.OnTurnEnd()`
- [ ] Ability assets exist in `Assets/_Project/Data/Abilities/`
- [ ] Range and AOE values are appropriate for your game scale

For commanders:

- [ ] Commander has `CommanderAura` component
- [ ] Aura radius is set
- [ ] Stat bonuses are configured
- [ ] Faction matching is enabled if needed
- [ ] Visualization is enabled for testing

---

## Next Steps (Phase 7+)

With the ability system in place, you can now:

1. **Phase 7: AI Opponent**
   - AI can evaluate and use abilities
   - Ability priority system already in AbilityData
   - Health thresholds configured for AI decision-making

2. **Phase 8: Army Building**
   - Units can be assigned abilities during army creation
   - Commanders with auras become strategic choices

3. **Phase 9: Campaign**
   - Unlock new abilities as rewards
   - Upgrade abilities between battles

4. **Phase 10: UI/UX**
   - Create ability buttons in action bar
   - Show cooldowns/action points in HUD
   - Display aura radius on battlefield

---

## File Structure Summary

```
Assets/_Project/
├── Scripts/
│   ├── Units/
│   │   ├── AbilityController.cs          ✅ NEW
│   │   ├── CommanderAura.cs              ✅ NEW
│   │   ├── Unit.cs                       ✅ UPDATED (integrated AbilityController)
│   │   ├── UnitStats.cs                  (Existing, used by abilities)
│   │   └── UnitCombat.cs                 (Existing, used by abilities)
│   ├── Data/
│   │   └── AbilityData.cs                (Existing, from Phase 2)
│   └── Editor/
│       └── CreateAbilityAssets.cs        ✅ NEW
├── Data/
│   └── Abilities/                        ✅ NEW FOLDER
│       ├── Ability_Fireball.asset        ✅ NEW
│       ├── Ability_Shield.asset          ✅ NEW
│       ├── Ability_Slow.asset            ✅ NEW
│       ├── Ability_Heal.asset            ✅ NEW
│       ├── Ability_Summon.asset          ✅ NEW (placeholder)
│       └── Ability_Wall.asset            ✅ NEW (placeholder)
└── Scenes/
    └── Testing/
        └── AbilityTest.unity             (Optional test scene)
```

---

## End of Phase 6 Implementation

**Status:** ✅ **COMPLETE**

All core ability systems are implemented and ready for testing!

- Ability execution: ✅
- Cooldown management: ✅
- Action points: ✅
- Targeting validation: ✅
- Commander auras: ✅
- 6 basic spells: ✅
- Integration with combat: ✅
- Editor tools: ✅
- Documentation: ✅
