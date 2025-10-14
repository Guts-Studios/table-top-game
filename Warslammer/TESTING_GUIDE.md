# Phase 2 Movement & Battlefield Testing Guide

## Setup Instructions

### 1. Create MovementTest Scene

1. In Unity, create new scene: `Assets/_Project/Scenes/Testing/MovementTest.unity`
2. Save the scene

### 2. Setup Battlefield

1. Create empty GameObject named "Battlefield"
2. Add these components:
   - BattlefieldManager
   - TerrainManager  
   - DeploymentZoneManager
   - LineOfSightManager
   - MovementValidator
   - MeasurementTool

3. Configure BattlefieldManager:
   - Set battlefield size (48" x 48")
   - Adjust center if needed

### 3. Setup Game Manager

1. Create empty GameObject named "GameManager"
2. Add GameManager component
3. Add TurnManager component
4. Add PhaseManager component
5. Link references in GameManager inspector

### 4. Setup Input System

1. Create empty GameObject named "InputManager"
2. Add these components:
   - InputManager
   - UnitSelector
   - CameraController

### 5. Setup Camera

1. Create Camera (or use Main Camera)
2. Position at: (0, 25, -15)
3. Rotation: (50, 0, 0) for isometric view
4. Link camera to InputManager and CameraController

### 6. Create Test Units

For each test unit (create 3-4):

1. Create empty GameObject named "TestUnit_01"
2. Add components:
   - Unit
   - UnitStats
   - UnitMovement
   - UnitVisuals
   - CapsuleCollider (radius 0.5)
   - SpriteRenderer

3. Configure Unit:
   - Create UnitData ScriptableObject in `Assets/_Project/Data/Units/`
   - Set base stats (HP: 10, Movement: 6", Attack: 3, etc.)
   - Assign to Unit component

4. Set sprite for visual representation
5. Position units on battlefield (Y = 0)
6. Set different OwnerPlayerIndex (0 and 1)

### 7. Create Ground Plane

1. Create 3D Plane GameObject
2. Scale to match battlefield size
3. Position at Y = 0
4. Add material for visibility

## Testing Checklist

### Unit Selection
- [ ] Click on unit selects it (shows highlight)
- [ ] Click on empty space deselects unit
- [ ] Hover over unit shows hover effect
- [ ] Only one unit can be selected at a time

### Movement System
- [ ] Selected unit can be dragged
- [ ] Ruler appears during drag showing distance
- [ ] Ruler is green when within movement range
- [ ] Ruler is red when beyond movement range
- [ ] Ghost sprite appears at cursor position
- [ ] Ghost is green for valid position, red for invalid

### Movement Validation
- [ ] Unit cannot move beyond its movement range
- [ ] Unit cannot overlap other units
- [ ] Unit cannot move outside battlefield bounds
- [ ] Invalid moves snap back to original position
- [ ] Valid moves animate smoothly to target

### Camera Controls
- [ ] WASD keys pan camera
- [ ] Mouse wheel zooms in/out
- [ ] Q/E keys rotate camera
- [ ] Edge scrolling works (mouse near screen edge)
- [ ] Camera respects bounds

### Turn Management
- [ ] Press Next Phase button to advance phases
- [ ] Turn Start -> Movement -> Action -> Turn End
- [ ] Units can only move during Movement phase
- [ ] Movement resets each turn
- [ ] Correct player's turn is indicated

## Common Issues & Solutions

### Units Don't Select
- Check unit has collider
- Verify unit layer is correct in InputManager
- Ensure camera has proper raycast layers

### Movement Doesn't Work
- Verify unit has all required components
- Check BattlefieldManager is properly referenced
- Ensure turn/phase system is active

### Ruler Not Showing
- Check MeasurementTool has LineRenderer
- Verify measurement tool is attached to Battlefield GameObject
- Check if unit movement range is > 0

### Camera Not Responding
- Ensure InputManager is in scene
- Check CameraController has camera reference
- Verify input event connections

## Debug Commands

Add these to test console commands:

```csharp
// Example debug commands to add to GameManager

[ContextMenu("Start Battle")]
void DebugStartBattle() {
    GameManager.Instance.StartBattle();
}

[ContextMenu("Next Phase")]
void DebugNextPhase() {
    PhaseManager.Instance.NextPhase();
}

[ContextMenu("Heal All Units")]
void DebugHealAll() {
    foreach(Unit unit in BattlefieldManager.Instance.ActiveUnits) {
        unit.Heal(100);
    }
}
```

## Performance Testing

- Monitor FPS with multiple units (test with 10-20 units)
- Check object pooling is working for measurement marks
- Verify no memory leaks during gameplay

## Next Steps After Testing

1. Fix any compilation errors
2. Adjust values (speeds, colors, ranges) for feel
3. Add placeholder sprites if needed
4. Test on different resolutions
5. Test touch input on mobile device
6. Document any bugs found

## Notes

- All systems are designed to work together
- Events provide loose coupling between systems
- TODO comments mark Phase 3 features
- Code compiles in Unity 2021.3 or later