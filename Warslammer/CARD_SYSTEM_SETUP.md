# Combat Card System - Setup & Testing Guide

## Phase 7.5: Combat Card System - COMPLETE ✓

The combat card system has been fully implemented and is ready for testing!

## What Was Built

### Core Systems
1. **CardData.cs** - ScriptableObject definition for combat cards
2. **CardManager.cs** - Manages each player's 10-card hand
3. **CombatCardIntegration.cs** - Integrates cards into combat resolution
4. **CardTestController.cs** - Keyboard-based testing interface
5. **CreateCardAssets.cs** - Editor script to generate card assets

### Card Structure (Per Player)
- 3 Attack Cards: +1, +2, +3 Attack Bonus
- 3 Defense Cards: +1, +2, +3 Defense Bonus
- 4 Faction-Specific Special Cards:
  - Reroll 1s
  - +2 Bonus Damage
  - Ignore Armor (one-use per battle)
  - Prevent All Damage (one-use per battle)

## Setup Instructions

### Step 1: Generate Card Assets
1. Open Unity Editor
2. Go to menu: **Warslammer > Setup > Create Default Cards**
3. This creates 10 card assets in `Assets/_Project/Data/Cards/`

### Step 2: Set Up Test Scene
1. Open the CombatTest scene
2. Create an empty GameObject named "CardSystem"
3. Add the **CombatCardIntegration** component to it
4. Create two child GameObjects:
   - "Player1_CardManager" - Add **CardManager** component
   - "Player2_CardManager" - Add **CardManager** component
5. In CombatCardIntegration Inspector:
   - Assign Player1_CardManager to "Player 1 Card Manager"
   - Assign Player2_CardManager to "Player 2 Card Manager"

### Step 3: Configure Card Managers
For each CardManager (Player 1 and Player 2):

**Standard Attack Cards (3):**
- Slot 0: Card_Attack_Plus1
- Slot 1: Card_Attack_Plus2
- Slot 2: Card_Attack_Plus3

**Standard Defense Cards (3):**
- Slot 0: Card_Defense_Plus1
- Slot 1: Card_Defense_Plus2
- Slot 2: Card_Defense_Plus3

**Faction Cards (4):**
- Slot 0: Card_Faction_Placeholder1 (Reroll 1s)
- Slot 1: Card_Faction_Placeholder2 (+2 Damage)
- Slot 2: Card_Faction_Placeholder3 (Ignore Armor)
- Slot 3: Card_Faction_Placeholder4 (Prevent Damage)

**Settings:**
- Player Index: 0 for Player 1, 1 for Player 2
- Faction: Choose any faction (currently placeholders)

### Step 4: Add Test Controller
1. Create an empty GameObject named "CardTestController"
2. Add the **CardTestController** component
3. In Inspector:
   - Assign an attacker unit (from scene)
   - Assign a defender unit (from scene)

## Testing Controls

### Attacker Card Controls
- **Q** - Play Attack +1 card
- **W** - Play Attack +2 card
- **E** - Play Attack +3 card
- **A** - Play Defense +1 card
- **S** - Play Defense +2 card
- **D** - Play Defense +3 card
- **Z** - Play Faction Card 1 (Reroll 1s)
- **X** - Play Faction Card 2 (+2 Damage)
- **C** - Play Faction Card 3 (Ignore Armor)
- **V** - Play Faction Card 4 (Prevent Damage)

### Defender Card Controls
- **I** - Play Attack +1 card
- **O** - Play Attack +2 card
- **P** - Play Attack +3 card
- **J** - Play Defense +1 card
- **K** - Play Defense +2 card
- **L** - Play Defense +3 card
- **N** - Play Faction Card 1
- **M** - Play Faction Card 2
- **,** - Play Faction Card 3
- **.** - Play Faction Card 4

### Combat Controls
- **SPACEBAR** - Trigger combat between attacker and defender
- **R** - Reset all cards (start fresh turn)
- **H** - Display all cards in console

## How It Works

### Card Flow
1. **Before Combat**: Players select which cards to play (if any)
2. **Attack Phase**: Attack card modifiers apply to attacker's dice pool
3. **Defense Phase**: Defense card modifiers apply to defender's dice pool
4. **Damage Phase**: Damage modifiers from cards apply
5. **After Combat**: Cards are marked as used and cleared for next combat

### Card Effects
- **Attack Bonus**: Added as modifier to attack dice pool
- **Defense Bonus**: Added as modifier to defense dice pool
- **Bonus Dice**: Extra dice added to the pool
- **Bonus Damage**: Flat damage added after rolls
- **Damage Multiplier**: Multiplies final damage
- **Ignore Armor**: Attack card bypasses defender's armor
- **Prevent Damage**: Defense card negates all damage
- **Reroll Effects**: (Placeholder - to be implemented)

### Card Restrictions
- Each card can only be played once per turn
- Some cards are one-use-per-battle (Ignore Armor, Prevent Damage)
- Cards must match the timing (Attack Phase vs Defense Phase)
- Faction cards require matching faction (currently disabled for testing)

## Testing Checklist

- [ ] Cards generate correctly via editor menu
- [ ] CardManagers show all 10 cards in Inspector
- [ ] Playing attack cards modifies combat rolls
- [ ] Playing defense cards modifies defense rolls
- [ ] Damage modifiers apply correctly
- [ ] Cards can only be used once per turn
- [ ] One-use cards can only be used once per battle
- [ ] Reset (R key) restores all cards
- [ ] Both players can play cards independently
- [ ] Console shows clear feedback for card plays

## Next Steps After Testing

Once the card system is tested and working:
1. **Phase 7: AI Opponent** - Implement AI decision-making
2. **Card UI**: Build visual drag-and-drop card interface
3. **Faction Balance**: Design unique faction-specific cards
4. **Card VFX**: Add visual effects for card plays

## Troubleshooting

**Cards not applying modifiers?**
- Check that CombatCardIntegration is in the scene
- Verify CardManager references are assigned
- Check console for card play confirmations

**Can't play cards?**
- Ensure units are assigned in CardTestController
- Check that card timing matches combat phase
- Verify cards haven't been used already this turn

**Missing card assets?**
- Run: Warslammer > Setup > Create Default Cards
- Check Assets/_Project/Data/Cards/ folder

## Files Reference

**Card System Core:**
- [CardData.cs](Assets/_Project/Scripts/Combat/CardData.cs)
- [CardManager.cs](Assets/_Project/Scripts/Combat/CardManager.cs)
- [CombatCardIntegration.cs](Assets/_Project/Scripts/Combat/CombatCardIntegration.cs)

**Testing:**
- [CardTestController.cs](Assets/_Project/Scripts/Input/CardTestController.cs)
- [CreateCardAssets.cs](Assets/_Project/Scripts/Editor/CreateCardAssets.cs)

**Related:**
- [GameEnums.cs](Assets/_Project/Scripts/Core/GameEnums.cs) - Added FactionType, CardType, CardTiming
- [DicePool.cs](Assets/_Project/Scripts/Combat/DicePool.cs) - AddDice() method used by cards
