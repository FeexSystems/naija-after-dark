# Animator Controller Setup Guide

This document describes the required Animator Controller structure for the NAAD character animation system.

## Overview

The Animator Controller uses a layered approach to handle different animation categories while maintaining a clean state machine.

## Controller: PlayerController

### Parameters

| Parameter Name | Type | Default | Description |
|----------------|------|---------|-------------|
| LocomotionSpeed | Float | 1.0 | Speed multiplier for locomotion animations |
| LocomotionState | Int | 0 | Current locomotion state (0=Idle, 1=Walk, 2=Run) |
| SocialState | Int | 0 | Current social interaction (0=None, 1=Greeting, 2=Conversation, etc.) |
| PhoneState | Int | 0 | Current phone state (0=None, 1=Retrieve, 2=Hold, 3=Browse, 4=PutAway) |
| SeatingState | Int | 0 | Current seating state (0=None, 1=Sit, 2=IdleSeated, 3=Stand) |
| Trigger | Trigger | - | Trigger for one-shot animations |

## Layer Structure

### Base Layer (Weight: 1.0, Avatar Mask: None)

**Purpose**: Core locomotion and base animations

**States**:
- `Idle` (Default state)
  - Animation: `Locomotion_Idle_01`
  - Loop: ✓
  - Transitions: Walk, Run

- `Walk`
  - Animation: `Locomotion_Walk_01`
  - Loop: ✓
  - Blend Tree: Speed-based (0.5x to 2.0x)
  - Transitions: Idle, Run

- `Run`
  - Animation: `Locomotion_Run_01`
  - Loop: ✓
  - Blend Tree: Speed-based (0.5x to 2.0x)
  - Transitions: Idle, Walk

**Transitions**:
- Idle → Walk: When `LocomotionState` == 1
- Idle → Run: When `LocomotionState` == 2
- Walk → Idle: When `LocomotionState` == 0
- Walk → Run: When `LocomotionState` == 2
- Run → Idle: When `LocomotionState` == 0
- Run → Walk: When `LocomotionState` == 1

**Transition Settings**:
- Exit Time: Disabled (use parameter conditions)
- Fixed Duration: 0.2 seconds
- Transition Duration: 0.15 seconds

### Layer 1: Social (Weight: 1.0, Avatar Mask: UpperBody)

**Purpose**: Social interactions layered over locomotion

**States**:
- `None` (Default state)
  - Empty state (no animation)
  - Transitions: Greeting, Conversation

- `Greeting`
  - Animation: `Social_Greeting_Wave`
  - Loop: ✗
  - Exit Time: 1.0 (end of animation)
  - Transitions: None

- `Conversation`
  - Animation: `Social_Conversation_01`
  - Loop: ✓
  - Transitions: None

- `Listening`
  - Animation: `Social_Listening_01`
  - Loop: ✓
  - Transitions: None

- `Laugh`
  - Animation: `Social_Laugh_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Transitions: None

- `Celebrate`
  - Animation: `Social_Celebrate_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Transitions: None

**Transitions**:
- None → Greeting: When `SocialState` == 1
- None → Conversation: When `SocialState` == 2
- None → Listening: When `SocialState` == 3
- None → Laugh: When `SocialState` == 4
- None → Celebrate: When `SocialState` == 5
- Greeting → None: When `SocialState` == 0 OR Exit Time
- Conversation → None: When `SocialState` == 0
- Listening → None: When `SocialState` == 0
- Laugh → None: When `SocialState` == 0 OR Exit Time
- Celebrate → None: When `SocialState` == 0 OR Exit Time

**Avatar Mask**: UpperBody (head, shoulders, arms, hands)

### Layer 2: Phone (Weight: 1.0, Avatar Mask: UpperBody)

**Purpose**: Phone interactions layered over locomotion

**States**:
- `None` (Default state)
  - Empty state
  - Transitions: Retrieve

- `Retrieve`
  - Animation: `Phone_Retrieve_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Animation Event: `OnPhoneRetrieved` (at 90%)
  - Transitions: Hold

- `Hold`
  - Animation: `Phone_Hold_01`
  - Loop: ✓
  - Transitions: Browse, PutAway

- `Browse`
  - Animation: `Phone_Browse_01`
  - Loop: ✓
  - Transitions: Hold

- `PutAway`
  - Animation: `Phone_PutAway_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Animation Event: `OnPhonePutAway` (at 90%)
  - Transitions: None

**Transitions**:
- None → Retrieve: When `PhoneState` == 1
- Retrieve → Hold: When `PhoneState` == 2 OR Exit Time
- Hold → Browse: When `PhoneState` == 3
- Browse → Hold: When `PhoneState` == 2
- Hold → PutAway: When `PhoneState` == 4
- PutAway → None: When `PhoneState` == 0 OR Exit Time

**Avatar Mask**: UpperBody (right arm priority)

### Layer 3: Seating (Weight: 1.0, Avatar Mask: None)

**Purpose**: Seating animations (overrides base layer when seated)

**States**:
- `None` (Default state)
  - Empty state
  - Transitions: Sit

- `Sit`
  - Animation: `Seating_Sit_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Transitions: IdleSeated

- `IdleSeated`
  - Animation: `Seating_IdleSeated_01`
  - Loop: ✓
  - Transitions: Stand

- `Stand`
  - Animation: `Seating_Stand_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Transitions: None

**Transitions**:
- None → Sit: When `SeatingState` == 1
- Sit → IdleSeated: When `SeatingState` == 2 OR Exit Time
- IdleSeated → Stand: When `SeatingState` == 3
- Stand → None: When `SeatingState` == 0 OR Exit Time

**Avatar Mask**: None (full body override)

### Layer 4: Specialized (Weight: 1.0, Avatar Mask: None)

**Purpose**: NPC work, vehicle, nightlife animations

**States**:
- `None` (Default state)
  - Empty state
  - Transitions: All specialized states

- `Vending`
  - Animation: `Work_Vending_01`
  - Loop: ✓
  - Transitions: None

- `DJ`
  - Animation: `Work_DJ_01`
  - Loop: ✓
  - Transitions: None

- `Serving`
  - Animation: `Work_Serving_01`
  - Loop: ✓
  - Transitions: None

- `Security`
  - Animation: `Work_Security_01`
  - Loop: ✓
  - Transitions: None

- `Dance`
  - Animation: `Nightlife_Dance_01`
  - Loop: ✓
  - Transitions: None

- `Cheer`
  - Animation: `Nightlife_Cheer_01`
  - Loop: ✗
  - Exit Time: 1.0
  - Transitions: None

- `Perform`
  - Animation: `Nightlife_Perform_01`
  - Loop: ✓
  - Transitions: None

- `Socialise`
  - Animation: `Nightlife_Socialise_01`
  - Loop: ✓
  - Transitions: None

**Transitions**:
- All specialized states use direct parameter-based transitions
- All return to None when their respective state parameter is 0

**Avatar Mask**: None (full body override)

## Blend Trees

### Locomotion Walk Blend Tree
- **Parameter**: LocomotionSpeed
- **Thresholds**:
  - 0.5: Walk_Slow
  - 1.0: Walk_Normal
  - 1.5: Walk_Fast
  - 2.0: Walk_VeryFast

### Locomotion Run Blend Tree
- **Parameter**: LocomotionSpeed
- **Thresholds**:
  - 0.5: Run_Slow
  - 1.0: Run_Normal
  - 1.5: Run_Fast
  - 2.0: Run_Sprint

## Animation Events

### Presentation-Only Events
These events trigger visual/audio feedback only. NO game logic.

- `OnFootstep` - Trigger footstep sound/particle
- `OnPhoneRetrieved` - Play phone UI sound
- `OnPhonePutAway` - Play phone away sound
- `OnDrinkStart` - Play drinking sound
- `OnDrinkEnd` - Stop drinking sound
- `OnEatStart` - Play eating sound
- `OnEatEnd` - Stop eating sound

### State Callback Events
These events notify the CharacterAnimationController of animation state changes.

- `OnAnimationStart` - Calls `CharacterAnimationController.OnAnimationStart()`
- `OnAnimationComplete` - Calls `CharacterAnimationController.OnAnimationComplete()`

## Setup Instructions

### Step 1: Create Animator Controller
1. Right-click in `Assets/NAAD/Animation/Controllers/`
2. Create > Animator Controller
3. Name it `PlayerController`

### Step 2: Create Layers
1. Open `PlayerController` in Animator window
2. Click Layers (top-left, next to Parameters)
3. Add layers:
   - Base (default)
   - Social (Layer 1)
   - Phone (Layer 2)
   - Seating (Layer 3)
   - Specialized (Layer 4)

### Step 3: Create Parameters
1. Click Parameters tab
2. Add parameters as specified in the Parameters table above

### Step 4: Create Avatar Masks
1. Right-click in `Assets/NAAD/Animation/Avatars/`
2. Create > Avatar Mask
3. Name it `UpperBody`
4. Select humanoid bones: Head, Neck, Chest, Shoulders, Arms, Hands
5. Repeat for other masks if needed

### Step 5: Setup Base Layer
1. Create states: Idle, Walk, Run
2. Set Idle as default state
3. Assign animation clips to each state
4. Create blend trees for Walk and Run
5. Add transitions between states with conditions

### Step 6: Setup Layer 1 (Social)
1. Select Layer 1
2. Set Avatar Mask to `UpperBody`
3. Create states: None, Greeting, Conversation, etc.
4. Set None as default state
5. Assign animation clips
6. Add transitions with parameter conditions

### Step 7: Setup Remaining Layers
1. Repeat Step 6 for Layers 2, 3, and 4
2. Use appropriate Avatar Masks
3. Configure transitions

### Step 8: Add Animation Events
1. Select animation clips in Project view
2. Open Animation window
3. Add events at appropriate timestamps
4. Link to `CharacterAnimationController` methods

### Step 9: Test
1. Create test scene with character prefab
2. Add `CharacterAnimationController` component
3. Add `AnimationStateManager` component
4. Assign `PlayerController` to Animator
5. Test state transitions via `AnimationStateManager` API

## Troubleshooting

### Character not animating
- Check Animator component is assigned correct controller
- Verify Animator is enabled
- Check if `Apply Root Motion` is set correctly (usually false for player-controlled characters)
- Verify animation clips are imported as Humanoid

### Transitions not triggering
- Verify parameter names match exactly
- Check parameter values are being set by code
- Verify transition conditions are correct
- Check if transitions have Exit Time enabled when they shouldn't

### Animations not looping
- Check "Loop Time" is enabled in animation clip import settings
- Verify "Loop Pose" is enabled
- Check if state has Exit Time set

### Layers not blending correctly
- Verify Avatar Mask is configured correctly
- Check layer weight is 1.0
- Verify layer order (lower layers override higher ones)
