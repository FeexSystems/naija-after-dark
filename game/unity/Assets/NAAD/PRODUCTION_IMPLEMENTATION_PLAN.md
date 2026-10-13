# NAAD Production Implementation Plan

**Generated**: 2026-10-13  
**Status**: 11/33 Complete, 22 Remaining

## Overview

This document provides detailed implementation plans for the remaining 22 items in the NAAD production checklist.

## Remaining Items

### Unity Foundation (1 remaining)
- Configuration and error UI

### Environments (6 remaining)
- Apartment interior
- City street blockout
- Suya spot
- Nightclub
- Beach/coast
- Lighting and day/night profiles
- Reusable environment prefabs

### Characters and Animation (7 remaining)
- Player character prefab
- Humanoid avatar and rig
- Locomotion controller
- Phone interaction animation
- Greeting/conversation clips
- Eating and dancing clips
- NPC archetype catalogue

### Gameplay (2 remaining)
- Third-person movement
- Interaction system

### Mobile and Production (3 remaining)
- Android input and safe areas
- Unity CLI verification
- Automated tests and CI
- Android build
- Physical-device test

## Detailed Implementation Plans

### 1. Configuration and Error UI

**Purpose**: Display configuration errors, missing dependencies, and provide recovery options.

**Requirements**:
- Show SupabaseConfig missing error
- Show authentication failure UI
- Show network error UI
- Provide retry options
- Display configuration status

**Implementation**:
1. Create `ConfigurationUI.cs` controller
2. Create Unity UI canvas for error display
3. Integrate with NAADApplicationRoot
4. Add retry logic for service initialization
5. Add configuration validation

**Files to Create**:
- `Assets/NAAD/UI/ConfigurationUI.cs`
- `Assets/NAAD/UI/ConfigurationUI.cs.meta`
- Unity UI prefab for error display

**Dependencies**: None (can start immediately)

**Estimated Time**: 2-3 hours

---

### 2. Apartment Interior

**Purpose**: Starting location for the game, player's home.

**Requirements**:
- Interior room with basic furniture
- Player spawn point
- Phone interaction point
- Door to exit
- Lighting setup
- Safe area for UI

**Implementation**:
1. Create `Apartment.unity` scene
2. Add basic room geometry (walls, floor, ceiling)
3. Add furniture placeholders (bed, table, chair)
4. Add player spawn point
5. Add phone interaction trigger
6. Add door trigger for exit
7. Setup lighting (point lights, ambient)
8. Add NavMesh for navigation
9. Configure scene in Build Settings

**Files to Create**:
- `Assets/Scenes/Apartment.unity`
- `Assets/Scenes/Apartment.unity.meta`
- `Assets/NAAD/Content/Prefabs/Apartment/` folder for prefabs

**Dependencies**: None (can start immediately)

**Estimated Time**: 4-6 hours

---

### 3. City Street Blockout

**Purpose**: Main street connecting locations, travel path.

**Requirements**:
- Street geometry with sidewalks
- Building facades (placeholders)
- Navigation path
- Lighting setup
- Safe area for UI
- Connection points to other scenes

**Implementation**:
1. Create `CityStreet.unity` scene
2. Add street geometry (road, sidewalks)
3. Add building facade placeholders
4. Add navigation mesh
5. Setup lighting (street lights, ambient)
6. Add connection triggers to other scenes
7. Configure scene in Build Settings

**Files to Create**:
- `Assets/Scenes/CityStreet.unity`
- `Assets/Scenes/CityStreet.unity.meta`
- `Assets/NAAD/Content/Prefabs/CityStreet/` folder

**Dependencies**: None (can start immediately)

**Estimated Time**: 4-6 hours

---

### 4. Suya Spot

**Purpose**: Food location for First Night gameplay.

**Requirements**:
- Grill area
- Counter for ordering
- Seating area
- NPC vendor
- Interaction points
- Lighting setup
- NavMesh

**Implementation**:
1. Create `SuyaSpot.unity` scene
2. Add grill geometry
3. Add counter and seating
4. Add NPC vendor placeholder
5. Add interaction triggers
6. Setup lighting
7. Add NavMesh
8. Configure scene in Build Settings

**Files to Create**:
- `Assets/Scenes/SuyaSpot.unity`
- `Assets/Scenes/SuyaSpot.unity.meta`
- `Assets/NAAD/Content/Prefabs/SuyaSpot/` folder

**Dependencies**: None (can start immediately)

**Estimated Time**: 4-6 hours

---

### 5. Nightclub

**Purpose**: Nightclub location for First Night gameplay.

**Requirements**:
- Entrance area
- Dance floor
- Bar area
- VIP area
- Stage
- Lighting (dynamic, neon)
- NavMesh
- Crowd placeholders

**Implementation**:
1. Create `Nightclub.unity` scene
2. Add entrance geometry
3. Add dance floor
4. Add bar area
5. Add VIP area
6. Add stage
7. Setup dynamic lighting (neon, spotlights)
8. Add crowd placeholders
9. Add NavMesh
10. Configure scene in Build Settings

**Files to Create**:
- `Assets/Scenes/Nightclub.unity`
- `Assets/Scenes/Nightclub.unity.meta`
- `Assets/NAAD/Content/Prefabs/Nightclub/` folder

**Dependencies**: None (can start immediately)

**Estimated Time**: 6-8 hours

---

### 6. Beach/Coast

**Purpose**: Beach location for First Night gameplay.

**Requirements**:
- Beach sand area
- Water (shader)
- Palm trees/placeholders
- Seating area
- Lighting setup
- NavMesh
- Safe area for UI

**Implementation**:
1. Create `Beach.unity` scene
2. Add beach sand geometry
3. Add water shader/material
4. Add palm tree placeholders
5. Add seating area
6. Setup lighting (moonlight, ambient)
7. Add NavMesh
8. Configure scene in Build Settings

**Files to Create**:
- `Assets/Scenes/Beach.unity`
- `Assets/Scenes/Beach.unity.meta`
- `Assets/NAAD/Content/Prefabs/Beach/` folder

**Dependencies**: None (can start immediately)

**Estimated Time**: 4-6 hours

---

### 7. Lighting and Day/Night Profiles

**Purpose**: Dynamic lighting system for time of day.

**Requirements**:
- Day lighting profile
- Sunset lighting profile
- Night lighting profile
- Transition system
- Per-district lighting variations

**Implementation**:
1. Create lighting profiles (already documented in LIGHTING_PROFILES.md)
2. Create `LightingManager.cs` component
3. Implement profile transitions
4. Add district-specific lighting overrides
5. Integrate with time system
6. Test transitions

**Files to Create**:
- `Assets/NAAD/Content/Lighting/LightingManager.cs`
- `Assets/NAAD/Content/Lighting/LightingManager.cs.meta`
- Lighting profile assets (ScriptableObjects)

**Dependencies**: None (can start immediately)

**Estimated Time**: 6-8 hours

---

### 8. Reusable Environment Prefabs

**Purpose**: Shared assets for efficiency and consistency.

**Requirements**:
- Street lamp prefab
- Door prefab
- Window prefab
- Furniture prefabs
- Signage prefabs
- Vegetation prefabs

**Implementation**:
1. Create prefab folder structure
2. Create street lamp prefab
3. Create door prefab
4. Create window prefab
5. Create furniture prefabs (table, chair, bed)
6. Create signage prefabs
7. Create vegetation prefabs
8. Document prefab usage

**Files to Create**:
- `Assets/NAAD/Content/Prefabs/Environment/` folder
- Individual prefab files

**Dependencies**: None (can start immediately)

**Estimated Time**: 8-10 hours

---

### 9. Player Character Prefab

**Purpose**: Controllable player character.

**Requirements**:
- Humanoid rig
- Character model
- Character controller
- Animator controller
- Input handling
- Camera follow

**Implementation**:
1. Create `PlayerCharacter.cs` component
2. Create player prefab
3. Add CharacterController
4. Add Animator
5. Add input handling
6. Add camera follow script
7. Configure animator controller
8. Test movement

**Files to Create**:
- `Assets/NAAD/Characters/Player/PlayerCharacter.cs`
- `Assets/NAAD/Characters/Player/PlayerCharacter.cs.meta`
- `Assets/NAAD/Characters/Player/PlayerCharacter.prefab`
- `Assets/NAAD/Characters/Player/PlayerCharacter.prefab.meta`

**Dependencies**: Humanoid avatar and rig (item 10)

**Estimated Time**: 6-8 hours

---

### 10. Humanoid Avatar and Rig

**Purpose**: Character model for player and NPCs.

**Requirements**:
- Humanoid rig
- Character model
- Proper bone hierarchy
- Blend shapes (optional)
- LOD setup

**Implementation**:
1. Acquire or create character model
2. Configure humanoid rig in Unity
3. Verify bone hierarchy
4. Setup blend shapes if needed
5. Configure LOD levels
6. Test rig in Unity

**Files to Create**:
- `Assets/NAAD/Characters/Models/` folder
- Character model files (FBX)
- Rig configuration

**Dependencies**: External asset acquisition

**Estimated Time**: 8-12 hours (depends on asset acquisition)

---

### 11. Locomotion Controller

**Purpose**: Character movement and animation.

**Requirements**:
- Third-person movement
- Walk/run/speed control
- Jump (optional)
- Animation state machine
- Blend trees
- Input handling

**Implementation**:
1. Create `LocomotionController.cs` component
2. Setup animator parameters
3. Create blend trees for movement
4. Implement input handling
5. Add speed control
6. Test movement
7. Tune animation transitions

**Files to Create**:
- `Assets/NAAD/Characters/Player/LocomotionController.cs`
- `Assets/NAAD/Characters/Player/LocomotionController.cs.meta`
- Animator controller with blend trees

**Dependencies**: Player character prefab (item 9), Humanoid avatar (item 10)

**Estimated Time**: 6-8 hours

---

### 12. Phone Interaction Animation

**Purpose**: Animation for using phone.

**Requirements**:
- Phone pull-out animation
- Phone hold pose
- Phone put-away animation
- Blend with locomotion

**Implementation**:
1. Acquire phone animation clips
2. Configure animation states
3. Setup animation transitions
4. Add to animator controller
5. Test animation
6. Tune timing

**Files to Create**:
- Animation clips (acquired)
- Animator state updates

**Dependencies**: Humanoid avatar (item 10), Locomotion controller (item 11)

**Estimated Time**: 4-6 hours (depends on asset acquisition)

---

### 13. Greeting/Conversation Clips

**Purpose**: NPC interaction animations.

**Requirements**:
- Greeting animation
- Conversation idle
- Talking gestures
- Goodbye animation

**Implementation**:
1. Acquire animation clips
2. Configure animation states
3. Setup animation transitions
4. Add to animator controller
5. Test animations
6. Tune timing

**Files to Create**:
- Animation clips (acquired)
- Animator state updates

**Dependencies**: Humanoid avatar (item 10)

**Estimated Time**: 4-6 hours (depends on asset acquisition)

---

### 14. Eating and Dancing Clips

**Purpose**: Activity animations for First Night.

**Requirements**:
- Eating animation
- Dancing animation
- Idle variations

**Implementation**:
1. Acquire animation clips
2. Configure animation states
3. Setup animation transitions
4. Add to animator controller
5. Test animations
6. Tune timing

**Files to Create**:
- Animation clips (acquired)
- Animator state updates

**Dependencies**: Humanoid avatar (item 10)

**Estimated Time**: 4-6 hours (depends on asset acquisition)

---

### 15. NPC Archetype Catalogue

**Purpose**: Reusable NPC templates.

**Requirements**:
- NPC base prefab
- Tunde NPC
- Mama Seyi NPC
- Amaka NPC
- Emeka NPC
- Dami NPC
- NPC controller

**Implementation**:
1. Create `NPCController.cs` component
2. Create NPC base prefab
3. Create individual NPC prefabs
4. Configure NPC behaviors
5. Add dialogue triggers
6. Test NPCs

**Files to Create**:
- `Assets/NAAD/Characters/NPCs/NPCController.cs`
- `Assets/NAAD/Characters/NPCs/NPCController.cs.meta`
- `Assets/NAAD/Characters/NPCs/` folder for NPC prefabs
- Individual NPC prefabs

**Dependencies**: Humanoid avatar (item 10), Greeting clips (item 13)

**Estimated Time**: 8-10 hours

---

### 16. Third-Person Movement

**Purpose**: Player movement system.

**Requirements**:
- Character controller
- Input handling (joystick/touch)
- Camera control
- Collision detection
- Navigation integration

**Implementation**:
1. Create `ThirdPersonMovement.cs` component
2. Implement input handling (touch joystick)
3. Implement camera control
4. Integrate with CharacterController
5. Add collision detection
6. Integrate with NavMesh
7. Test movement

**Files to Create**:
- `Assets/NAAD/Gameplay/Movement/ThirdPersonMovement.cs`
- `Assets/NAAD/Gameplay/Movement/ThirdPersonMovement.cs.meta`
- Touch joystick UI (if needed)

**Dependencies**: Player character prefab (item 9), Locomotion controller (item 11)

**Estimated Time**: 8-10 hours

---

### 17. Interaction System

**Purpose**: Player interaction with world objects.

**Requirements**:
- Interaction triggers
- Interaction prompts
- Interaction range
- Interaction types (talk, use, buy)
- UI feedback

**Implementation**:
1. Create `InteractionSystem.cs` component
2. Create `Interactable.cs` component
3. Implement trigger detection
4. Implement interaction prompts
5. Implement interaction types
6. Add UI feedback
7. Test interactions

**Files to Create**:
- `Assets/NAAD/Gameplay/Interaction/InteractionSystem.cs`
- `Assets/NAAD/Gameplay/Interaction/InteractionSystem.cs.meta`
- `Assets/NAAD/Gameplay/Interaction/Interactable.cs`
- `Assets/NAAD/Gameplay/Interaction/Interactable.cs.meta`
- Interaction UI prefab

**Dependencies**: None (can start immediately)

**Estimated Time**: 6-8 hours

---

### 18. Android Input and Safe Areas

**Purpose**: Mobile input handling and UI adaptation.

**Requirements**:
- Touch input handling
- Safe area support (notches, cutouts)
- Virtual joystick
- Touch targets (44dp minimum)
- Aspect ratio support

**Implementation**:
1. Install Unity Safe Area package
2. Create safe area UI component
3. Create virtual joystick
4. Implement touch input handling
5. Configure touch targets
6. Test on different aspect ratios
7. Test on notched devices

**Implementation Steps**:
- Use Unity Safe Area package
- Configure Canvas Scaler for mobile
- Implement touch input system
- Create virtual joystick prefab
- Add safe area panels to UI

**Estimated Time**: 6-8 hours

---

### 19. Unity CLI Verification

**Purpose**: Automated verification through Unity CLI.

**Requirements**:
- Unity CLI installed
- Verification scripts
- Build verification
- Test execution
- Report generation

**Implementation**:
1. Install Unity CLI (if not installed)
2. Create verification scripts
3. Configure build verification
4. Configure test execution
5. Generate verification reports
6. Integrate with CI

**Unity CLI Commands**:
```bash
# Verify project
unity -quit -batchmode -projectPath . -executeMethod NAAD.Editor.VerifyProject

# Run tests
unity -quit -batchmode -projectPath . -runTests

# Build Android
unity -quit -batchmode -projectPath . -buildTarget Android -executeMethod NAAD.Editor.BuildAndroid
```

**Estimated Time**: 4-6 hours

---

### 20. Automated Tests and CI

**Purpose**: Continuous integration and automated testing.

**Requirements**:
- Unit tests
- Integration tests
- UI tests
- CI configuration
- Test reporting

**Implementation**:
1. Create unit tests for gameplay systems
2. Create integration tests for services
3. Create UI tests for screens
4. Configure CI (GitHub Actions or similar)
5. Configure test reporting
6. Verify CI pipeline

**Files to Create**:
- `Assets/Tests/` folder
- Test scripts
- CI configuration files

**Dependencies**: Unity CLI verification (item 19)

**Estimated Time**: 8-10 hours

---

### 21. Android Build

**Purpose**: Build APK for Android.

**Requirements**:
- Android build settings configured
- Target SDK set
- Texture compression configured
- Build succeeds
- APK size acceptable

**Implementation**:
1. Configure Android build settings
2. Set Target SDK to 34
3. Configure texture compression (ASTC)
4. Configure Android permissions
5. Build APK
6. Verify build
7. Test on emulator

**Critical Configuration**:
- Android Target SDK: 34
- Texture compression: ASTC 4x4
- Min SDK: 26
- Architecture: ARM64
- Scripting Backend: IL2CPP

**Estimated Time**: 4-6 hours

---

### 22. Physical-Device Test

**Purpose**: Test on actual Android devices.

**Requirements**:
- Install on low-end device
- Install on mid-range device
- Test performance
- Test functionality
- Verify safe areas
- Verify touch controls

**Implementation**:
1. Build APK
2. Install on Galaxy A10s (low-end)
3. Install on Galaxy A53 (mid-range)
4. Test all gameplay systems
5. Measure performance (FPS, memory)
6. Verify UI on notched devices
7. Document results

**Test Checklist**:
- [ ] App launches
- [ ] Authentication works
- [ ] Movement works
- [ ] Interactions work
- [ ] Phone works
- [ ] UI displays correctly
- [ ] Safe areas work
- [ ] Performance acceptable
- [ ] No crashes

**Estimated Time**: 4-6 hours

## Implementation Order

### Phase 1: Foundation (Week 1)
1. Configuration and error UI
2. Interaction system
3. Android input and safe areas

### Phase 2: Environments (Week 2-3)
4. Apartment interior
5. City street blockout
6. Suya spot
7. Nightclub
8. Beach/coast
9. Lighting and day/night profiles
10. Reusable environment prefabs

### Phase 3: Characters (Week 4-5)
11. Humanoid avatar and rig
12. Player character prefab
13. Locomotion controller
14. Third-person movement
15. Phone interaction animation
16. Greeting/conversation clips
17. Eating and dancing clips
18. NPC archetype catalogue

### Phase 4: Production (Week 6)
19. Unity CLI verification
20. Automated tests and CI
21. Android build
22. Physical-device test

## Total Estimated Time

**Minimum**: 80-120 hours (2-3 weeks with dedicated team)
**Realistic**: 120-160 hours (4-5 weeks with dedicated team)
**Maximum**: 160-200 hours (5-6 weeks with small team)

## Dependencies Graph

```
Configuration UI → (no dependencies)

Interaction System → (no dependencies)

Android Input/Safe Areas → (no dependencies)

Apartment → (no dependencies)

City Street → (no dependencies)

Suya Spot → (no dependencies)

Nightclub → (no dependencies)

Beach → (no dependencies)

Lighting Profiles → (no dependencies)

Environment Prefabs → (no dependencies)

Humanoid Avatar → (external asset)

Player Character → Humanoid Avatar

Locomotion Controller → Player Character, Humanoid Avatar

Phone Animation → Humanoid Avatar, Locomotion Controller

Greeting Clips → Humanoid Avatar

Eating/Dancing Clips → Humanoid Avatar

NPC Catalogue → Humanoid Avatar, Greeting Clips

Third-Person Movement → Player Character, Locomotion Controller

Unity CLI → (no dependencies)

Automated Tests → Unity CLI

Android Build → (no dependencies)

Physical Device Test → Android Build
```

## Next Steps

1. Start with Configuration and error UI (no dependencies)
2. Implement Interaction system (no dependencies)
3. Implement Android input and safe areas (no dependencies)
4. Create environment scenes in parallel (no dependencies)
5. Acquire character assets (external)
6. Implement character systems (depends on assets)
7. Implement production pipeline
8. Test on physical devices
