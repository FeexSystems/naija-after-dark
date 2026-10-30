# Humanoid / Animation Asset Audit

**Repository:** `FeexSystems/naija-after-dark`  
**Branch:** `chore/production-readiness-foundation`  
**Latest inspected commit:** `1a82e4b`  
**Unity project:** `game/unity`  
**Audit date:** 2026-10-29

## Executive finding

The Unity `Assets` directory contains a substantial library of environment and character model source files, including a plausible player FBX. However, the assets required by the strict Gate C humanoid/Animator contract are not yet assembled or wired.

The most relevant discovery is:

`game/unity/Assets/NAAD/Content/Models/STD/CHR_Player_STD_v001.fbx`

This is a valid Kaydara FBX file and its embedded node names include Mixamo-style humanoid bones such as `mixamorigHips`, `mixamorigSpine`, `mixamorigHead`, arms, forearms, upper legs, and lower legs. It is a strong candidate source rig, but repository inspection alone cannot prove that Unity imports it as a valid Humanoid Avatar. Its current `.meta` file contains only the basic GUID header and does not contain an explicit serialized `ModelImporter` humanoid configuration.

## Inventory summary

| Asset category | Observed count / result | Assessment |
|---|---:|---|
| Unity `.meta` files | 542 | Metadata validator passes. |
| FBX files | 61 | Includes 9 character-named FBX files and many environment/prop models. |
| GLB files | 62 | Includes 9 character-named GLB files and many environment/prop models. |
| Unity animation clips (`.anim`) | 0 | No Gate C animation clips are committed. |
| Animator controllers (`.controller`) | 0 | No `PlayerController.controller` is committed. |
| Avatar masks (`.mask`) | 0 | No `UpperBodyMask.mask` is committed. |
| Gate C player prefab | 0 | No `PlayerRig.prefab` exists at the required path. |
| Gate C standalone Avatar asset | 0 | No `PlayerAvatar.asset` exists at the required path. |
| Generic prefabs | 7 | All inspected prefabs are environment prefabs or unrelated utility assets; no player rig prefab was found. |

## Character-model assets found

The repository contains these character-like model pairs:

- `Content/Models/STD/CHR_Player_STD_v001.fbx` and `.glb`
- `Content/Models/ML/CHR_MamaSeyi_ML_v001.fbx` and `.glb`
- `Content/Models/ML/CHR_Mallam_ML_v001.fbx` and `.glb`
- `Content/Models/BCH/CHR_BeachVendor_BCH_v001.fbx` and `.glb`
- `Content/Models/NCL/CHR_Bouncer_NCL_v001.fbx` and `.glb`
- `Content/Models/NCL/CHR_DJ_NCL_v001.fbx` and `.glb`
- `Content/Models/STR/CHR_Crowd01_STR_v001.fbx` and `.glb`
- `Content/Models/STR/CHR_Crowd02_STR_v001.fbx` and `.glb`
- `Content/Models/STR/CHR_Crowd03_STR_v001.fbx` and `.glb`

The player FBX is approximately 575 KB and is recognized by the `file` utility as a valid FBX 7400 file. The matching GLB is approximately 129 KB and is recognized as glTF binary version 2.

## Gate C contract comparison

The strict verifier expects assets under `Assets/NAAD/Animation/`:

| Required Gate C asset | Status | Notes |
|---|---|---|
| `Characters/Player/PlayerRig.fbx` | **Missing** | A candidate source exists at `Content/Models/STD/CHR_Player_STD_v001.fbx`, but it has not been promoted/configured at the expected path. |
| `Characters/Player/PlayerRig.prefab` | **Missing** | Must be created and wired with Animator, movement, and animation components. |
| `Controllers/PlayerController.controller` | **Missing** | No Animator Controller files exist in the project Assets tree. |
| `Avatars/PlayerAvatar.asset` | **Missing** | Could be generated/embedded by Unity from a valid humanoid import, then saved as a stable asset if required by the project contract. |
| `Avatars/UpperBodyMask.mask` | **Missing** | Must be created and assigned to a non-base Animator layer. |
| `Animations/Locomotion/Idle.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Locomotion/Walk.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Locomotion/Run.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Phone/Retrieve.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Phone/Hold.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Phone/Browse.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Phone/PutAway.anim` | **Missing** | No `.anim` files exist. |
| `Animations/Social/Greeting.anim` | **Missing** | No `.anim` files exist. |

The verifier also requires typed Animator parameters, controller references to all required clips, an upper-body masked overlay layer, and a prefab containing `Animator`, `CharacterAnimationController`, `AnimationStateManager`, `CharacterController`, and `PlayerLocomotionController`. None of those serialized prefab/controller contracts can currently be proven because the required controller and prefab do not exist.

## Metadata and tracking checks

- `tools/validate-unity-meta.py game/unity` passes across all 542 `.meta` files.
- Every inspected source model has a paired `.meta` file.
- The candidate player FBX is Git-tracked along with its `.meta` file.
- No suspicious Gate C files are present under a different filename such as `.controller`, `.anim`, or `.mask`.
- The `.glb` files provide useful source/reference assets, but the Gate C verifier is written around Unity-imported FBX, Animator Controller, AvatarMask, AnimationClip, and prefab objects.

## Technical assessment of the player FBX

The player FBX is promising because its embedded names show a conventional humanoid hierarchy:

- Root: `mixamorigRoot`
- Pelvis: `mixamorigHips`
- Spine chain: `mixamorigSpine`, `mixamorigSpine1`, `mixamorigSpine2`
- Head: `mixamorigHead`
- Arms: left/right arm and forearm nodes
- Legs: left/right upper-leg and lower-leg nodes

This supports using it as the starting source for Gate C. It does **not** by itself establish that Unity’s `ModelImporter` has `animationType: Human`, that the avatar is valid, or that the rig contains production-quality animation data. Those properties must be configured and verified inside Unity.

## Recommended next steps

1. Open `CHR_Player_STD_v001.fbx` in Unity and configure its Rig tab to `Humanoid` with a valid Avatar definition.
2. Confirm the Avatar mapping and save the imported asset configuration.
3. Promote or duplicate the configured FBX into the exact Gate C path: `Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx`, preserving stable metadata.
4. Acquire or author the eight required clips. Mixamo or Unity Starter Assets are documented project options; verify licensing before committing them.
5. Create `PlayerController.controller` with the required typed parameters and states.
6. Create `UpperBodyMask.mask` and assign it to a non-base overlay layer.
7. Build `PlayerRig.prefab` with the required Animator and movement/animation components.
8. Run the strict `NAAD.Editor.VerifyAnimationAssets.RunVerification` job in Unity CI.
9. Follow the headless pass with an actual playback test for locomotion, phone interactions, and social greeting.

## Conclusion

The asset audit changes the earlier conclusion from “no humanoid source exists” to the more precise finding: **a likely humanoid player source exists, but the Gate C production asset pipeline is unfinished**. The missing work is not merely file acquisition; it includes Unity humanoid import configuration, animation acquisition, Animator authoring, AvatarMask setup, prefab composition, stable `.meta` tracking, and CI verification.
