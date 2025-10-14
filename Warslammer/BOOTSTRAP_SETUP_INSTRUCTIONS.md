# Bootstrap Scene Setup Instructions

## Overview
This document provides instructions for setting up the Bootstrap scene in Unity. The Bootstrap scene initializes all core game systems and should be the first scene loaded when the game starts.

## Setup Steps

### 1. Create the Bootstrap Scene
1. In Unity, go to `File > New Scene`
2. Create an empty scene
3. Save it as `Bootstrap.unity` in `Assets/_Project/Scenes/`

### 2. Set Up the GameManager
**Option A - Automatic Setup:**
1. Create an empty GameObject in the scene (Right-click in Hierarchy > Create Empty)
2. Rename it to "BootstrapSetup"
3. Add the `BootstrapSetup` component to it
4. The GameManager will be automatically created when the scene starts

**Option B - Manual Setup:**
1. Create an empty GameObject in the scene
2. Rename it to "GameManager"
3. Add the following components:
   - `GameManager`
   - `TurnManager`
   - `PhaseManager`
   - `SaveManager`
4. The GameManager component will automatically wire up the references

### 3. Configure Build Settings
1. Go to `File > Build Settings`
2. Add the Bootstrap scene to the build (it should be index 0)
3. Ensure it's set as the first scene in the build order

### 4. Test the Setup
1. Enter Play Mode in Unity
2. Check the Console for initialization messages:
   - `[GameManager] Initializing game systems...`
   - `[GameManager] Game systems initialized successfully`
3. You can also use the BootstrapSetup component's context menu:
   - Right-click the BootstrapSetup component
   - Select "Test System Initialization"
   - Check Console for test results

## Scene Structure
After setup, your Bootstrap scene should look like this:

```
Bootstrap Scene
├── Main Camera (default)
├── Directional Light (default)
└── GameManager
    ├── GameManager (component)
    ├── TurnManager (component)
    ├── PhaseManager (component)
    └── SaveManager (component)
```

## What Gets Initialized
The Bootstrap scene initializes:
- **GameManager**: Core game state management
- **TurnManager**: IGOUGO turn structure
- **PhaseManager**: Move/Action/End phase management
- **SaveManager**: Save/load functionality

All managers persist between scenes via `DontDestroyOnLoad`.

## Next Steps
After the Bootstrap scene is set up:
1. Create additional scenes for gameplay (Battle, MainMenu, etc.)
2. Use `GameManager.Instance.ChangeGameState()` to transition between states
3. Build out the battlefield, unit, and combat systems
4. Create UI scenes and managers

## Troubleshooting

### GameManager not found
- Ensure the GameManager GameObject exists in the scene
- Check that the GameManager component is attached
- Verify the scene is saved properly

### Managers not wired up
- Check the GameManager inspector to see if references are set
- Run the scene and check for initialization logs
- Use the BootstrapSetup "Setup Game Manager" context menu option

### Multiple GameManagers
- Only one GameManager should exist at a time
- The singleton pattern will destroy duplicates automatically
- Check that you don't have GameManagers in multiple scenes

## Testing Checklist
- [ ] Bootstrap scene created and saved
- [ ] GameManager GameObject exists with all components
- [ ] Scene enters Play Mode without errors
- [ ] Console shows successful initialization
- [ ] GameManager.Instance is accessible
- [ ] All manager references are properly wired
- [ ] Scene is added to Build Settings at index 0