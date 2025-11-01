# Project SOVL-Like - Development TODO List

**Last Updated:** October 14, 2024

## Project Overview
Turn-based fantasy wargame for Unity (mobile-first: Android/iOS/Windows/Mac)
- Free measurement-based movement system
- D6 dice pool combat
- Roguelite campaign with node map
- 2 factions (Kingdom & Warborn) at MVP

---

## ✅ COMPLETED PHASES (1-5)

### Phase 1: Architecture & Planning ✅
- [x] Technical architecture document created
- [x] System interaction diagrams
- [x] Mobile optimization strategy
- [x] Folder structure designed

### Phase 2: Core Foundation ✅
- [x] Complete project folder structure (~30 files)
- [x] 7 ScriptableObject data classes (UnitData, FactionData, AbilityData, etc.)
- [x] 5 core interfaces (IDamageable, ISelectable, IMovable, ICombatant, ISaveData)
- [x] 4 manager classes (GameManager, TurnManager, PhaseManager, SaveManager)
- [x] Game enums and utility classes
- [x] Bootstrap setup instructions

### Phase 3: Movement & Battlefield ✅
- [x] Battlefield management system (17 files)
- [x] Free movement with measurement tool (visual ruler)
- [x] Drag-and-drop unit movement with ghost preview
- [x] Collision detection (circle colliders)
- [x] Unit selection system with highlights
- [x] Isometric camera controller (pan/zoom/rotate)
- [x] Line of sight calculations
- [x] Deployment zone management
- [x] Touch gesture support for mobile

### Phase 4: Turn Management & Selection ✅
- [x] IGOUGO turn structure
- [x] Phase management (Move/Action/End)
- [x] Unit selection via raycasts
- [x] Player controller for unit ownership

### Phase 5: Combat System ✅
- [x] D6 dice pool system (15 files)
- [x] Combat flow: range → LOS → dice → damage
- [x] Critical hits on 6s (extra success + Switch)
- [x] Weapon system with WeaponData ScriptableObjects
- [x] Status effects (Bleed, Rooted, Shaken, Guard, Stunned, Poisoned)
- [x] Morale system (2d6 checks)
- [x] Combat log with color-coded messages
- [x] Damage popup system
- [x] Modifier stacking

### Bug Fixes ✅
- [x] Fixed ICombatant/Unit duplicate Attack definitions (renamed to AttackValue)
- [x] Fixed ObjectPool coroutine issue (added CoroutineHost singleton)
- [x] Fixed MeasurementTool ObjectPool initialization (created MeasurementMarker component)
- [x] Replaced deprecated FindObjectOfType with FindFirstObjectByType (12 files)
- [x] Fixed unused _touchDragThreshold in InputManager

**Status:** ~65 C# scripts implemented, ~12,000+ lines of code. All compilation errors and warnings resolved.

---

## 🔄 CURRENT PHASE: Unity Editor Setup & Testing

### Unity Scene Setup (IN PROGRESS)
- [x] Open project in Unity Editor (Unity 2022 LTS or newer)
- [x] Follow BOOTSTRAP_SETUP_INSTRUCTIONS.md:
  - [x] Create Bootstrap.unity scene
  - [x] Add GameManager GameObject with all components
  - [x] Verify all managers initialize (✅ All systems working)
- [x] Follow TESTING_GUIDE.md:
  - [x] Create MovementTest.unity scene
  - [x] Setup battlefield, camera, input systems
  - [x] Create 3-4 test units
  - [x] Test selection and movement (✅ Drag-to-move working!)
- [x] Follow COMBAT_SYSTEM_IMPLEMENTATION.md:
  - [x] Create CombatTest.unity scene
  - [x] Create WeaponData assets (Sword, Bow, Spear)
  - [x] Test combat with dice rolls
  - [x] Verify damage, status effects, morale (✅ Combat system working!)

---

## 📋 PENDING PHASES (6-12)

### Phase 6: Unit Abilities & Commander System ✅
- [x] Implement ability execution system
- [x] Create AbilityController for units
- [x] Build commander aura system (buffs nearby units)
- [x] Design 6 basic spells (nuke, shield, slow, heal, summon, wall)
- [x] Create AbilityData assets for each ability
- [x] Test ability targeting and effects (Ready for testing)
- [x] Integrate abilities with combat system

### Phase 7.5: Combat Card System 🔄
- [x] Design card system architecture
- [x] Create CardData ScriptableObject
- [x] Implement CardManager component (10-card fixed hand)
- [x] Create standard cards (3 attack +1/+2/+3, 3 defense +1/+2/+3)
- [x] Create faction-specific card placeholders (4 cards)
- [x] Integrate cards into combat resolution (CombatCardIntegration)
- [x] Create test controller for cards (keyboard testing)
- [ ] Build visual card selection UI (drag-to-play)
- [ ] Test full card-based combat flow

### Phase 7: AI Opponent (Utility-Based)
- [ ] Build UtilityAIBrain system
- [ ] Implement AIAction base class
- [ ] Create 5+ AI actions:
  - [ ] MoveTowardObjectiveAction
  - [ ] AttackWeakestUnitAction
  - [ ] SeekCoverAction
  - [ ] UseAbilityAction
  - [ ] ChargeEnemyAction
- [ ] Implement AIConsideration system (scoring)
- [ ] Create 3 difficulty levels (Easy, Normal, Hard)
- [ ] Test AI vs player in skirmish mode

### Phase 8: Army Building & Deployment
- [ ] Create ArmyBuilder UI scene
- [ ] Build unit selection interface
- [ ] Implement point calculation system
- [ ] Add army validation (commander required, unit limits)
- [ ] Create save/load army lists
- [ ] Design 3 preset army templates per faction
- [ ] Test army building flow

### Phase 9: Roguelite Campaign Structure
- [ ] Implement NodeMap generation (8 layers, branching paths)
- [ ] Create CampaignManager system
- [ ] Build node types (Combat, Elite, Shop, Event, Boss)
- [ ] Design reward system (units, equipment, abilities)
- [ ] Create campaign progression tracking
- [ ] Implement meta-currency for unlocks
- [ ] Build campaign UI (node map, selection)
- [ ] Test full campaign run (~45-60 minutes)

### Phase 10: UI/UX Elements
- [ ] Create main menu scene
- [ ] Build in-game HUD:
  - [ ] Turn order display
  - [ ] Unit stat tooltips
  - [ ] Action button panel
  - [ ] Phase indicator
- [ ] Enhance combat log (expandable panel)
- [ ] Create settings menu:
  - [ ] Graphics quality
  - [ ] Audio volume
  - [ ] Controls (mouse/touch toggle)
- [ ] Add tutorial system (basic movement/combat)
- [ ] Implement pause menu

### Phase 11: Data Asset Creation
- [ ] Create 2 factions:
  - [ ] Kingdom faction:
    - [ ] 1 Commander (Knight Lord)
    - [ ] 5 units (Swordsman, Spearman, Archer, Knight, Cleric)
  - [ ] Warborn faction:
    - [ ] 1 Commander (Warlord)
    - [ ] 5 units (Berserker, Raider, Skirmisher, Beast Rider, Shaman)
- [ ] Design unit stats and balance
- [ ] Create equipment data (armor, weapons)
- [ ] Build ability data for each unit
- [ ] Create 3 terrain types (forest, hills, ruins)
- [ ] Design 8 campaign missions

### Phase 12: Polish, Optimization & Testing
- [ ] Audio integration:
  - [ ] Background music (2 tracks)
  - [ ] SFX library (attack, hit, crit, move, UI)
- [ ] VFX polish:
  - [ ] Attack effects (slashes, projectiles)
  - [ ] Damage hit effects
  - [ ] Status effect indicators
- [ ] Performance optimization:
  - [ ] Profile on mid-tier Android/iOS devices
  - [ ] Target 60 FPS
  - [ ] Optimize draw calls (<100)
  - [ ] Memory profiling
- [ ] Balance testing:
  - [ ] Unit point costs
  - [ ] Weapon damage values
  - [ ] Ability cooldowns
  - [ ] AI difficulty curves
- [ ] Bug fixing & QA:
  - [ ] 20 canned battle scenarios
  - [ ] Edge case testing
  - [ ] Mobile touch controls refinement
- [ ] Build & deployment:
  - [ ] Android APK build
  - [ ] iOS TestFlight build
  - [ ] Windows/Mac standalone builds

---

## 📚 Documentation Files

- `BOOTSTRAP_SETUP_INSTRUCTIONS.md` - Unity scene setup guide
- `TESTING_GUIDE.md` - Movement testing checklist
- `COMBAT_SYSTEM_IMPLEMENTATION.md` - Combat setup and testing
- `IMPLEMENTATION_SUMMARY.md` - Complete feature inventory
- `TODO.md` - This file

---

## 🎯 Acceptance Criteria (MVP)

- [ ] Playable skirmish mode (1 battle, player vs AI)
- [ ] Campaign mode (6-8 encounters, ~45-60 minutes)
- [ ] 2 factions with balanced armies
- [ ] Functional army builder
- [ ] Mobile touch controls working
- [ ] 60 FPS on mid-tier devices
- [ ] No critical bugs or soft-locks

---

## 📝 Notes

- **Current Code Status:** ~65 C# scripts, compiles cleanly
- **Unity Version:** 2022 LTS or newer
- **Target Platforms:** Android, iOS, Windows, Mac (mobile-first)
- **Architecture:** Event-driven, modular, data-driven (ScriptableObjects)
- **Next Immediate Step:** Complete Unity scene setup and testing (Phases 1-5)

---

## 🔗 Quick Links

- Project Path: `/home/caldwejf/github/table-top-game/Warslammer`
- Scripts: `Assets/_Project/Scripts/`
- Data Assets: `Assets/_Project/Data/`
- Scenes: `Assets/_Project/Scenes/`