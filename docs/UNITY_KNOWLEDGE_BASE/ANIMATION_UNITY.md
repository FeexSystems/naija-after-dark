# Animation in Unity — Import → Controller → Rigging → Timeline (NAAD-applied)

Source: Animation in Unity (2022 LTS), full pipeline. Pairs with NAAD
`Animation/ANIMATION_MANIFEST.md`, `ANIMATOR_CONTROLLER_SETUP.md`,
`VerifyAnimationAssets.cs`.

## Pipeline (follow in order)
1. External (Maya/Blender) → FBX: bake animation on export, select full
   character, sane units. Prefer Mixamo motion libraries for placeholder beats.
2. Import: Mesh (geometry) → Materials → Skeleton → Avatar → Animation clips.
   Humanoid type for retargeting across characters; Generic only for
   non-humanoid props. Verify avatar muscle mapping before controllers.
3. Animator Controller: Parameters (bool/float/int/trigger) → Layers (base +
   additive/override w/ masks for upper-body) → Blend Trees (locomotion
   Idle/Walk/Dance by speed/energy) → Transitions (exit time, interruption).
4. Character Controller / root motion: EITHER root-motion-driven (clip moves
   capsule; good for dance/procession) OR script-driven (code moves; good for
   tight nav). Never both fighting. Foot IK grounds feet on stage/uneven floor.
5. GPU-neighbor features: Blend Shapes (facial/social emotes), Animation Events
   (footstep SFX, beat hits — incl. read-only clip events), Animation Rigging
   package (runtime hand-on-surface, grip door handles — merge with clips).
6. Timeline + Cinemachine: cutscenes, First Night arrival beat, club show
   sequencing; mock in Unity, FBX-export to DCC for polish if needed.

## Workflow rules
- Sandbox scene + Prefab Mode for authoring; AssetPostprocessor + Presets to
  enforce import standards automatically.
- Custom Editor visualizers for state machines; log state changes via channels.
- Keep clips additive/layered (greeting layer over locomotion) so NPC variety
  doesn't explode controller count.

## Applies to NAAD
- Player + club NPCs: Humanoid, retarget shared dance/idle library.
- Suya vendor: upper-body override layer (serving) over idle.
- Validate headless: `VerifyAnimationAssets` (clips present, avatars humanoid,
  controllers assigned) in the same Batchmode pass as scenes.
