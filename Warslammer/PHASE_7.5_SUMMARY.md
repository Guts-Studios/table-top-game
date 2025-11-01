# Phase 7.5: Combat Card System - Implementation Complete ✅

**Status:** Core implementation complete, ready for testing
**Date Completed:** October 17, 2025
**Files Created:** 5 new files
**Files Modified:** 2 files
**Compilation Status:** ✅ All errors resolved

---

## What Was Built

### New Components

#### 1. CardData.cs
**Location:** `Assets/_Project/Scripts/Combat/CardData.cs`

ScriptableObject that defines all card properties:
- **Basic Info:** Card name, type (Attack/Defense/Special), timing (when playable)
- **Combat Modifiers:** Attack bonus, defense bonus, bonus dice
- **Damage Modifiers:** Bonus damage, damage multiplier, ignore armor
- **Special Effects:** Reroll ones/failed, auto-successes, prevent damage
- **Restrictions:** Faction-specific, one-use-per-turn, one-use-per-battle

```csharp
[CreateAssetMenu(fileName = "NewCard", menuName = "Warslammer/Combat/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public CardTiming timing;
    public FactionType faction;
    public int attackBonus;
    public int defenseBonus;
    // ... more properties
}
```

#### 2. CardManager.cs
**Location:** `Assets/_Project/Scripts/Combat/CardManager.cs`

Manages each player's 10-card fixed hand:
- Stores 3 standard attack cards (+1/+2/+3)
- Stores 3 standard defense cards (+1/+2/+3)
- Stores 4 faction-specific cards
- Tracks used cards (per turn and per battle)
- Validates card play eligibility
- Provides turn/battle reset functionality

**Key Methods:**
- `CanPlayCard(CardData card)` - Check if card can be played
- `PlayCard(CardData card)` - Play a card and mark it used
- `OnTurnStart()` - Reset cards for new turn
- `OnBattleStart()` - Reset all cards for new battle

#### 3. CombatCardIntegration.cs
**Location:** `Assets/_Project/Scripts/Combat/CombatCardIntegration.cs`

Singleton that integrates cards into combat resolution:
- References both players' CardManagers
- Applies attack card modifiers to dice pools
- Applies defense card modifiers to dice pools
- Applies damage modifiers from cards
- Tracks last played attack/defense cards
- Provides card manager access by player index or unit

**Integration Points:**
- `ApplyAttackCardModifiers(Unit, DicePool)` - Modify attack rolls
- `ApplyDefenseCardModifiers(Unit, DicePool)` - Modify defense rolls
- `ApplyDamageModifiers(int damage)` - Modify final damage
- `ShouldIgnoreArmor()` - Check if armor should be bypassed

#### 4. CardTestController.cs
**Location:** `Assets/_Project/Scripts/Input/CardTestController.cs`

Keyboard-based testing interface for cards:
- Supports both attacker and defender card plays
- Q/W/E/A/S/D for attacker attack/defense cards
- I/O/P/J/K/L for defender attack/defense cards
- Z/X/C/V for attacker faction cards
- N/M/,/. for defender faction cards
- SPACEBAR to trigger combat
- R to reset cards
- H to display all cards

#### 5. CreateCardAssets.cs
**Location:** `Assets/_Project/Scripts/Editor/CreateCardAssets.cs`

Editor utility to auto-generate card assets:
- Creates 3 attack cards (+1/+2/+3 attack bonus)
- Creates 3 defense cards (+1/+2/+3 defense bonus)
- Creates 4 faction placeholder cards:
  - Reroll 1s
  - +2 Bonus Damage
  - Ignore Armor (one-use)
  - Prevent All Damage (one-use)
- Accessible via menu: **Warslammer > Setup > Create Default Cards**

### Modified Files

#### GameEnums.cs
**Location:** `Assets/_Project/Scripts/Core/GameEnums.cs`

Added three new enums:

```csharp
public enum CardType
{
    Attack,
    Defense,
    Special
}

public enum CardTiming
{
    AttackPhase,
    DefensePhase,
    DamagePhase,
    Anytime
}

public enum FactionType
{
    None,      // Universal cards
    Empire,    // Placeholder faction
    Chaos,     // Placeholder faction
    Eldar,     // Placeholder faction
    Orks,      // Placeholder faction
    Necrons,   // Placeholder faction
    Tau        // Placeholder faction
}
```

#### TODO.md
Updated Phase 7.5 checklist with completed tasks.

---

## Card System Design

### Fixed Hand System
Each player has a **fixed hand of 10 cards** (not drawn randomly):
- Always have access to all 10 cards
- Cards reset each turn (except one-use-per-battle cards)
- No deck/draw/discard mechanics
- Simple and predictable for tactical decisions

### Standard Cards (6 total, 3 attack + 3 defense)
| Card Name | Type | Effect | Use Restriction |
|-----------|------|--------|-----------------|
| Attack +1 | Attack | +1 attack bonus | Once per turn |
| Attack +2 | Attack | +2 attack bonus | Once per turn |
| Attack +3 | Attack | +3 attack bonus | Once per turn |
| Defense +1 | Defense | +1 defense bonus | Once per turn |
| Defense +2 | Defense | +2 defense bonus | Once per turn |
| Defense +3 | Defense | +3 defense bonus | Once per turn |

### Faction Cards (4 total, placeholders for now)
| Card Name | Effect | Use Restriction |
|-----------|--------|-----------------|
| Reroll 1s | Reroll all 1s in dice pool | Once per turn |
| Damage Boost | +2 bonus damage | Once per turn |
| Armor Pierce | Ignore defender's armor | Once per battle |
| Perfect Defense | Prevent all damage | Once per battle |

### Card Flow in Combat
1. **Pre-Combat Phase**
   - Both players can play attack/defense cards
   - Cards are marked as "played"
2. **Attack Roll Phase**
   - Attack card modifiers apply to attacker's dice pool
   - Dice are rolled with bonuses
3. **Defense Roll Phase**
   - Defense card modifiers apply to defender's dice pool
   - Dice are rolled with bonuses
4. **Damage Phase**
   - Damage modifiers from cards apply
   - Special effects (ignore armor, prevent damage) resolve
5. **Post-Combat Phase**
   - Played cards are cleared
   - One-use cards remain used

---

## Technical Implementation

### Architecture Pattern
Following the existing ScriptableObject data-driven pattern:
- `CardData` (like `AbilityData`, `WeaponData`, `UnitData`)
- `CardManager` (like `AbilityController`)
- Integration singleton (like other combat systems)

### Integration Points with Existing Systems
- **DicePool:** Uses `AddDice()` and `AddModifier()` methods
- **Combat Resolution:** Hooks into existing combat flow
- **Unit System:** Uses `Unit.OwnerPlayerIndex` for card manager access
- **Turn System:** Resets cards on turn start

### Memory & Performance
- All cards are ScriptableObject assets (shared references, not instances)
- Minimal runtime allocations (lists created once)
- No dynamic card generation or pooling needed
- Fixed hand size = predictable memory footprint

---

## Errors Fixed During Implementation

### Error 1: FactionType Missing
**Problem:** `CardData.cs` and `CardManager.cs` referenced non-existent `FactionType` enum
**Solution:** Added `FactionType` enum to `GameEnums.cs` with 6 placeholder factions
**Files Modified:** `GameEnums.cs`

### Error 2: Wrong DicePool API
**Problem:** Called `DicePool.AddBonusDice()` which doesn't exist
**Solution:** Changed to `DicePool.AddDice()` (actual method name)
**Files Modified:** `CombatCardIntegration.cs` (lines 106, 129)

### Error 3: Unused Field Warning
**Problem:** `AbilityTestController.instructions` field was unused
**Solution:** Removed the unused field
**Files Modified:** `AbilityTestController.cs`

**Compilation Status:** ✅ All errors resolved, clean build

---

## Testing Setup Instructions

### Quick Start
1. **Generate Cards:**
   - Menu: `Warslammer > Setup > Create Default Cards`
   - Creates 10 card assets in `Assets/_Project/Data/Cards/`

2. **Set Up Scene:**
   - Open `CombatTest.unity` scene
   - Create "CardSystem" GameObject
   - Add `CombatCardIntegration` component
   - Create 2 child GameObjects with `CardManager` components
   - Assign card managers to integration component

3. **Configure Managers:**
   - Assign 10 card assets to each CardManager
   - Set player indices (0 and 1)
   - Set factions (any for testing)

4. **Add Test Controller:**
   - Create "CardTestController" GameObject
   - Add `CardTestController` component
   - Assign attacker and defender units

5. **Test:**
   - Press Play in Unity
   - Use Q/W/E/A/S/D to play attacker cards
   - Use SPACEBAR to trigger combat
   - Watch console for card effects

**Detailed Setup:** See `CARD_SYSTEM_SETUP.md`

---

## Next Steps

### Immediate (Phase 7.5 Completion)
- [ ] Test card effects in CombatTest scene
- [ ] Verify one-use-per-turn restrictions work
- [ ] Verify one-use-per-battle restrictions work
- [ ] Confirm damage/defense modifiers apply correctly
- [ ] Test card reset on turn start

### Future Enhancements
- [ ] Build visual card UI (drag-to-play interface)
- [ ] Design unique faction-specific cards (4 per faction)
- [ ] Add card play animations/VFX
- [ ] Implement reroll mechanics (currently placeholder)
- [ ] Add auto-success mechanics (currently placeholder)
- [ ] Balance card effects with combat system

### Phase 7: AI Opponent (Next Major Phase)
As requested by user: **"Let's do option 1 and then go option two afterwards!"**
- Option 1 (Phase 7.5 - Cards) = ✅ Complete
- Option 2 (Phase 7 - AI) = 🔜 Next

---

## Files Summary

### Created (5 files)
- `Assets/_Project/Scripts/Combat/CardData.cs` (150 lines)
- `Assets/_Project/Scripts/Combat/CardManager.cs` (253 lines)
- `Assets/_Project/Scripts/Combat/CombatCardIntegration.cs` (315 lines)
- `Assets/_Project/Scripts/Input/CardTestController.cs` (305 lines)
- `Assets/_Project/Scripts/Editor/CreateCardAssets.cs` (290 lines)

**Total New Code:** ~1,313 lines

### Modified (2 files)
- `Assets/_Project/Scripts/Core/GameEnums.cs` (+30 lines)
- `Warslammer/TODO.md` (updated Phase 7.5 checklist)

---

## Key Design Decisions

1. **Fixed Hand vs Draw Deck:** Chose fixed hand for simplicity and predictability
2. **Faction Cards vs Unit Cards:** Chose faction-level cards (not unit-specific)
3. **10-Card Hand:** 3 attack + 3 defense + 4 faction = balanced tactical options
4. **ScriptableObject Pattern:** Consistent with existing data architecture
5. **Integration Singleton:** Easy global access for combat resolution
6. **Keyboard Testing First:** Faster iteration than building full UI

---

## User Requirements Met ✅

✅ 10 cards per player
✅ 3 attack cards (+1/+2/+3)
✅ 3 defense cards (+1/+2/+3)
✅ 4 faction-specific cards
✅ Fixed hand (not random draw)
✅ Play one card per combat turn
✅ Faction-specific (not unit-specific)
✅ Visual list selection (via test controller, UI pending)

---

## Documentation Created

- `CARD_SYSTEM_SETUP.md` - Detailed setup and testing guide
- `PHASE_7.5_SUMMARY.md` - This file (implementation summary)

---

**Phase 7.5 Status: CORE IMPLEMENTATION COMPLETE ✅**
**Ready for:** User testing and feedback
**Next Phase:** Phase 7 - AI Opponent (after testing)
