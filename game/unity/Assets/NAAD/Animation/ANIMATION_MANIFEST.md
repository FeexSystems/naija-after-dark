# NAAD Animation Manifest

## Audit Results

**Current State**: No existing character assets found.
- No character prefabs (only placeholder "New Mesh.prefab")
- No FBX models
- No Animator Controllers
- No animation clips
- Characters/, Animation/, and Content/ directories are empty

## Animation Categories

### 1. Locomotion
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `loc_idle` | Idle | Standing idle animation | HIGH | MISSING |
| `loc_walk` | Walk | Walking animation | HIGH | MISSING |
| `loc_run` | Run | Running animation | HIGH | MISSING |
| `loc_start` | Start | Transition from idle to movement | MEDIUM | MISSING |
| `loc_stop` | Stop | Transition from movement to idle | MEDIUM | MISSING |
| `loc_turn_left` | Turn Left | Turning left while standing/moving | MEDIUM | MISSING |
| `loc_turn_right` | Turn Right | Turning right while standing/moving | MEDIUM | MISSING |

### 2. Social
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `soc_greeting` | Greeting | Wave or nod hello | HIGH | MISSING |
| `soc_conversation` | Conversation | Talking/gesturing while standing | HIGH | MISSING |
| `soc_listening` | Listening | Nodding/attentive posture | MEDIUM | MISSING |
| `soc_laugh` | Laugh | Laughing animation | MEDIUM | MISSING |
| `soc_celebrate` | Celebrate | Victory/celebration animation | LOW | MISSING |

### 3. Phone
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `phn_retrieve` | Retrieve | Pull phone from pocket/bag | HIGH | MISSING |
| `phn_hold` | Hold | Holding phone in hand | HIGH | MISSING |
| `phn_browse` | Browse | Scrolling/tapping on phone | HIGH | MISSING |
| `phn_put_away` | Put Away | Return phone to pocket/bag | HIGH | MISSING |

### 4. Food
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `food_order` | Order | Ordering/gesturing to order | MEDIUM | MISSING |
| `food_serve` | Serve | Serving food/drink | MEDIUM | MISSING |
| `food_eat` | Eat | Eating animation | MEDIUM | MISSING |
| `food_drink` | Drink | Drinking animation | MEDIUM | MISSING |

### 5. Nightlife
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `nt_dance` | Dance | Dancing animation | HIGH | MISSING |
| `nt_cheer` | Cheer | Cheering/raising hands | MEDIUM | MISSING |
| `nt_perform` | Perform | Performance animation (DJ, singing, etc.) | MEDIUM | MISSING |
| `nt_socialise` | Socialise | Socializing in group | MEDIUM | MISSING |

### 6. Seating
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `sit_sit` | Sit | Sitting down animation | HIGH | MISSING |
| `sit_idle` | Idle Seated | Sitting idle | HIGH | MISSING |
| `sit_stand` | Stand | Standing up from seat | HIGH | MISSING |

### 7. Vehicle
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `veh_enter` | Enter | Entering vehicle | LOW | MISSING |
| `veh_exit` | Exit | Exiting vehicle | LOW | MISSING |
| `veh_driver` | Driver | Seated driving posture | LOW | MISSING |
| `veh_passenger` | Passenger | Seated passenger posture | LOW | MISSING |

### 8. NPC Work
| Animation ID | Name | Description | Priority | Status |
|--------------|------|-------------|----------|--------|
| `work_vending` | Vending | Operating vending machine | MEDIUM | MISSING |
| `work_dj` | DJ Performance | DJ mixing/performing | MEDIUM | MISSING |
| `work_serving` | Serving | Serving customers (bartender/waiter) | MEDIUM | MISSING |
| `work_security` | Security Patrol | Walking/security patrol | MEDIUM | MISSING |

## Rig Requirements

### Humanoid Rig Specifications
- **Rig Type**: Humanoid
- **Avatar Definition**: Generic humanoid with standard bone naming
- **Required Bones**: Hips, Spine, Chest, Neck, Head, Shoulders, Arms, Hands, Legs, Feet
- **Animation Type**: Generic (for compatibility) or Humanoid (for retargeting)

### Recommended Asset Sources
1. **Unity Asset Store**: Free/low-cost humanoid rigs and animation packs
2. **Mixamo**: Free humanoid animations (Adobe account required)
3. **Unity Starter Assets**: Third-person character controller includes basic locomotion
4. **Custom Production**: Commission or create in-house if budget allows

## Implementation Priority

### Phase 1: Player Rig (MVP)
1. Acquire humanoid character rig
2. Implement Locomotion animations (idle, walk, run)
3. Implement Phone animations (retrieve, hold, browse, put away)
4. Implement Social greeting animation
5. Create base Animator Controller with locomotion blend tree
6. Test locomotion state machine

### Phase 2: Core Interactions
1. Implement remaining Social animations
2. Implement Seating animations
3. Implement Food animations
4. Create interaction-specific Animator layers
5. Test state transitions

### Phase 3: Specialized Content
1. Implement Nightlife animations
2. Implement NPC Work animations
3. Implement Vehicle animations
4. Create NPC-specific Animator Controllers
5. Full integration testing

## Technical Notes

### Animator Controller Structure
- **Base Layer**: Locomotion (idle, walk, run, transitions)
- **Layer 1**: Social interactions (overlay on base)
- **Layer 2**: Phone interactions (overlay on base)
- **Layer 3**: Seating (override base when seated)
- **Layer 4**: Specialized (NPC work, vehicle, etc.)

### State Machine Rules
- All one-shot animations must transition back to a valid locomotion state
- No animation should lock the character indefinitely
- Use exit time or animation events for clean transitions
- Blend trees for smooth parameter-based transitions (speed, direction)

### Animation Events Policy
- Animation events MUST NOT trigger authoritative game logic
- Events can only trigger visual/audio feedback
- All game state changes (money, inventory, relationships) must be handled by separate systems
- Animation events are for presentation layer only
