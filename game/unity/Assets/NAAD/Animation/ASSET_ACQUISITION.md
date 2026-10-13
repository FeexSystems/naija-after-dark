# Animation Asset Acquisition Process

## Recommended Asset Sources

### 1. Mixamo (Free)
**URL**: https://www.mixamo.com
**Cost**: Free (requires Adobe account)
**License**: Free for commercial use
**Pros**:
- Large library of humanoid animations
- Auto-rigger for custom models
- Direct FBX download
- Compatible with Unity humanoid rigs

**Cons**:
- Requires Adobe account
- Limited customization
- Some animations may need cleanup

**Recommended Animations from Mixamo**:
- Locomotion: Idle, Walking, Running, Standing to Walk, Walk to Run
- Social: Waving, Talking, Listening, Laughing
- Phone: Holding phone, Using phone (may need custom)
- Seating: Sitting, Sitting Idle, Standing from Sit

**Process**:
1. Create Adobe account
2. Browse animation library
3. Select humanoid rig or upload custom model
4. Download animations as FBX
5. Import to Unity (Assets > Import New Asset)
6. Configure import settings (Rig Type: Humanoid)
7. Test in Animator preview

### 2. Unity Asset Store
**URL**: https://assetstore.unity.com
**Cost**: Varies (free to paid)
**License**: Per asset
**Pros**:
- Curated quality
- Unity-native formats
- Often include rigs + animations
- Reviews and ratings

**Cons**:
- Cost varies
- License terms vary
- May require purchase

**Recommended Packages**:
- **Starter Assets - Third Person Character** (Free)
  - Includes: Locomotion, basic interactions
  - Rig: Humanoid
  - License: Unity license

- **Generic RPG Character** (Free/Low-cost)
  - Includes: Combat, social, locomotion
  - Rig: Humanoid
  - Check license terms

**Process**:
1. Search for "humanoid animations" or "character controller"
2. Filter by price (free first)
3. Review license terms
4. Download and import via Package Manager
5. Extract animations from package
6. Configure import settings

### 3. Unity Starter Assets (Free)
**Package**: com.unity.starter-assets-thirdperson
**Cost**: Free
**License**: Unity license
**Includes**:
- Basic humanoid character
- Locomotion animations (idle, walk, run, jump)
- Basic blend trees
- Animator controller template

**Process**:
1. Open Package Manager (Window > Package Manager)
2. Select "Unity Registry"
3. Search "Starter Assets - Third Person"
4. Install package
5. Locate assets in Packages/Starter Assets/ThirdPersonCharacter
6. Copy relevant animations to NAAD/Animation/

### 4. Custom Production
**When to Use**:
- Specific cultural animations (Nigerian gestures, dance styles)
- Brand-specific character
- Unique gameplay mechanics
- Budget allows

**Process**:
1. Define animation requirements (reference videos, timing)
2. Commission animator or use in-house
3. Create in animation software (Blender, Maya, etc.)
4. Export as FBX with humanoid rig
5. Import to Unity
6. Configure import settings
7. Test and iterate

## Import Settings Checklist

### FBX Import Settings
- **Scale Factor**: 1.0 (or match project scale)
- **Import Animation**: ✓
- **Animation Compression**: Optimal (or keyframe reduction if needed)
- **Rig Type**: Humanoid
- **Avatar Definition**: Create from this model
- **Humanoid Template**: (select appropriate template)

### Animation Clip Settings
- **Loop Time**: ✓ for idle, walk, run
- **Loop Pose**: ✓ for looping animations
- **Cycle Offset**: 0 (adjust if needed for seamless loops)
- **Root Transform Rotation**: Bake into pose
- **Root Transform Position (Y)**: Bake into pose
- **Root Transform Position (XZ)**: Based on original

### Rig Configuration
- **Avatar**: Create/assign humanoid avatar
- **Humanoid**: Configure bone mapping
- **Muscle Definition**: Default (or customize if needed)
- **Additional Curves**: Add if needed for IK or blend shapes

## Asset Organization

### Directory Structure
```
Assets/NAAD/Animation/
├── Characters/
│   ├── Player/
│   │   ├── PlayerRig.fbx
│   │   ├── PlayerRig.prefab
│   │   └── Meta/
│   ├── NPCs/
│   │   ├── Bartender/
│   │   ├── DJ/
│   │   └── Security/
│   └── Common/
├── Animations/
│   ├── Locomotion/
│   │   ├── Idle.anim
│   │   ├── Walk.anim
│   │   └── Run.anim
│   ├── Social/
│   ├── Phone/
│   ├── Food/
│   ├── Nightlife/
│   ├── Seating/
│   ├── Vehicle/
│   └── NPCWork/
├── Controllers/
│   ├── PlayerController.controller
│   ├── NPCController.controller
│   └── Layers/
└── Avatars/
    ├── PlayerAvatar.asset
    └── CommonAvatar.asset
```

## Naming Convention

### Animation Clips
Format: `[category]_[action]_[variant]`
Examples:
- `Locomotion_Idle_01`
- `Social_Greeting_Wave`
- `Phone_Hold_RightHand`
- `Food_Drink_Bottle`

### Animator Controllers
Format: `[CharacterType]Controller`
Examples:
- `PlayerController`
- `NPCBartenderController`
- `NPCDJController`

### Parameters
Format: `[category][Action]` (camelCase)
Examples:
- `locomotionSpeed`
- `socialState`
- `phoneState`

## Quality Control

### Before Import
- Verify animation matches reference
- Check frame rate (prefer 30 or 60 fps)
- Ensure loop points are clean
- Confirm rig is humanoid-compatible

### After Import
- Test in Animator preview window
- Check for foot sliding
- Verify bone hierarchy
- Test blend trees
- Check transition smoothness
- Verify no clipping with itself

### In-Game Testing
- Test at different game speeds
- Test with different character scales
- Test transitions between states
- Test with other animations layered
- Check performance impact

## License Tracking

### Documentation Required
For each asset, document:
- Source (Mixamo, Asset Store, custom)
- License type (free, commercial, custom)
- Restrictions (attribution, modification limits)
- Purchase date/order number (if applicable)
- Attribution requirements

### Template
```markdown
## Asset: [Animation Name]
- **Source**: [Mixamo/Asset Store/Custom]
- **Author**: [Author name/company]
- **License**: [License type]
- **Date Acquired**: [YYYY-MM-DD]
- **Cost**: [Free/$XX]
- **Restrictions**: [Any restrictions]
- **Attribution**: [Required attribution text]
```

## Phase 1 Acquisition (MVP)

### Required for Player Rig
1. **Humanoid Character Rig**
   - Source: Unity Starter Assets (free) or Mixamo
   - Priority: HIGH
   - Deadline: Week 1

2. **Locomotion Animations**
   - Idle, Walk, Run
   - Source: Starter Assets or Mixamo
   - Priority: HIGH
   - Deadline: Week 1

3. **Phone Animations**
   - Retrieve, Hold, Browse, Put Away
   - Source: Mixamo (may need custom for browse)
   - Priority: HIGH
   - Deadline: Week 2

4. **Social Greeting**
   - Wave or nod
   - Source: Mixamo
   - Priority: HIGH
   - Deadline: Week 2

### Total Estimated Cost
- Free option: $0 (Starter Assets + Mixamo)
- Low-cost option: $0-50 (Asset Store bundles)
- Custom option: $500-2000 (commissioned work)

## Next Steps

1. Acquire humanoid character rig (Phase 1)
2. Import and configure rig
3. Acquire locomotion animations
4. Create base Animator Controller
5. Test locomotion blend tree
6. Acquire phone animations
7. Add phone interaction layer
8. Acquire social greeting
9. Add social interaction layer
10. Full integration test
