# NAAD Gate C — Player Rig & Animation

**Status:** Planned (next gate after First Night runtime / Gate B).  
**Date:** 2026-10-14 · **Branch:** `feat/audit-gate-recovery-2026-10-14`  
**Editor:** `6000.6.5f1` · **Target:** StandaloneWindows64 (CI: StandaloneLinux64)

## Objective

Prove the character animation **contract** end-to-end: a real humanoid player rig
that moves through the Night District with authoritative-command-driven animation,
and a **verifiable** Animator contract CI can gate on — the way `NAADBuild.Verify`
gates Gate 22 scenes. Gate C is NOT complete until `VerifyAnimationAssets` passes
against **real assets** in a clean headless run, not from source inspection.

## Current state (verified)

Animation **code + docs are complete**; **assets are missing (0/37)**.

Already built (compile-clean, in `Assets/NAAD/Animation/`):
- `AnimationCommand.cs` — authoritative command struct + `AnimationCommandType` enum (37 types: Locomotion/Social/Phone/Food/Nightlife/Seating/Vehicle/NPCWork).
- `CharacterAnimationController.cs` — `[RequireComponent(Animator)]` bridge: command queue → Animator params (`LocomotionState`, `SocialState`, `PhoneState`, `SeatingState`, `Trigger`). `OnAnimationStart/Complete` are **presentation-only**.
- `AnimationStateManager.cs` — high-level API (`SetIdle/Walk/Run`, phone, `Sit/Stand`, `EnterVehicle`…), state tracking + `OnStateChanged`.
- `VerifyAnimationAssets.cs` (menu: `NAAD > Verify Animation Assets`) and `AnimationTestComponent.cs` (MVP test driver).
- Docs: `ANIMATION_MANIFEST.md`, `ANIMATOR_CONTROLLER_SETUP.md`, `ASSET_ACQUISITION.md`, `VERIFICATION_REPORT.md`.

### Governing architecture rule (must hold)
> Animation **events MUST NOT** trigger authoritative game logic. Commands flow
> **game logic → AnimationStateManager → CharacterAnimationController → Animator**
> (one direction). Money/relationships/location stay server-authoritative via
> `ICommandService`/`naad_execute_command` — animations are presentation only.
> Mirrors the First Night pattern verified in Gate B.

## Scope (in)

- Humanoid **PlayerRig** (FBX + prefab) at `Assets/NAAD/Animation/Characters/Player/`.
- **MVP locomotion** (Idle/Walk/Run) + **Phone** (Retrieve/Hold/Browse/PutAway) + **Social greeting**.
- **PlayerController** Animator Controller: base locomotion blend tree + additive upper-body overlay layers (per `ANIMATOR_CONTROLLER_SETUP.md`).
- **UpperBodyMask** avatar mask for the phone/social overlay layer.
- Player prefab wired with `Animator` + `CharacterAnimationController` + `AnimationStateManager`.
- `VerifyAnimationAssets` extended to assert the contract, wired into CI as a new job mirroring `verify-unity`.

## Scope (out / deferred)

- Phase 2 (Seating, Food, full Social) and Phase 3 (Nightlife, Vehicle, NPC Work) — manifest-scoped, not required for the player-rig MVP.
- Root-motion vs script-driven locomotion decision (see C4).
- Blend shapes / facial emotes / Animation Rigging — deferred.

## Implementation plan (phased)

### Phase C1 — Acquire + import the rig
1. Source a humanoid rig — **free path: Mixamo** (or Unity Starter Assets). Prefer Mixamo motion library for placeholder beats (per `ANIMATION_UNITY.md`).
2. Import to `Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx`.
3. Set **Rig = Humanoid**, configure Avatar, verify muscle definitions.
4. Commit the FBX + generated `.meta` (GUID-stable). **Do not** commit license/credential files.

### Phase C2 — Acquire MVP clips
- Locomotion: `Idle`, `Walk`, `Run` → `Animations/Locomotion/`.
- Phone: `Retrieve`, `Hold`, `Browse`, `PutAway` → `Animations/Phone/`.
- Social: `Greeting` → `Animations/Social/`.
- Match the exact IDs/names in `ANIMATION_MANIFEST.md` so `VerifyAnimationAssets` resolves them.

### Phase C3 — Animator Controller + mask
1. Build `Controllers/PlayerController.controller` per `ANIMATOR_CONTROLLER_SETUP.md`: base layer (Idle/Walk/Run, speed blend tree, `LocomotionState` int transitions) + overlay layers Social/Phone with **UpperBodyMask**, additive, returning to locomotion on completion.
2. Create `Avatars/UpperBodyMask.mask`.
3. Verify every controller parameter matches the names `CharacterAnimationController` writes (`LocomotionSpeed`, `LocomotionState`, `SocialState`, `PhoneState`, `SeatingState`, `Trigger`). **Mismatch here is the #1 silent-failure mode** — code compiles but nothing animates.

### Phase C4 — Player prefab + wiring
- Assemble `PlayerRig.prefab`: `Animator` (controller + avatar assigned), `CharacterAnimationController`, `AnimationStateManager`, capsule/character controller for movement.
- **Locomotion decision (design fork):** choose **script-driven** (code moves the capsule; tight nav, matches the scene anchor/travel model) OR **root-motion** (clip drives). Recommendation: **script-driven** — `FirstNightController` already drives travel authoritatively; root motion would fight it.

### Phase C5 — Verify + CI
1. Extend `VerifyAnimationAssets` to assert the contract (exit criteria).
2. Add a `verify-animation` job to `.github/workflows/naad-unity.yml` mirroring `verify-unity` (same licensing preflight, `buildMethod: NAAD.Editor.VerifyAnimationAssets`).
3. Re-run headless locally (single-instance, kill-lock pattern used for Gate 22).

## Exit criteria (measurable)

Gate C is **PASS** only when all hold in a **clean headless run** (`exit 0`):

1. `VerifyAnimationAssets` exits 0: rig present + **Humanoid avatar valid**, all MVP clips present (Idle/Walk/Run + 4 Phone + Greeting), `PlayerController` assigned to the player prefab's Animator, and **controller parameters match the code's expected names** (no orphans).
2. `AnimationTestComponent` MVP sequence passes: Idle → Walk → Run → Stop → PhoneRetrieve → Hold → Browse → PutAway → Greeting, each returning to a valid locomotion state (no lock).
3. Locomotion drives `LocomotionState`; blend tree responds to `LocomotionSpeed` (speed-scaled walk/run).
4. Green CI run records the run URL + SHA here (same pattern as Gate 22 / First Night).
5. No animation event path mutates authoritative state (grep guard: no `ICommandService`/wallet/relationship calls reachable from `OnAnimationStart/Complete`).

## Risks / watch-items

- **Parameter-name drift** between controller and `CharacterAnimationController` (silent no-op) — mitigated by the contract assertion (criterion 1).
- **Humanoid retarget** mismatches on a non-standard Mixamo skeleton — validate muscle definitions at import (C1.3).
- **Asset binary size** — FBX/clips are small and safe to commit (no 100 MB cap concern, unlike vendor environment packs).
- **Animation events policy** — the invariant that must not be violated as NPC animation is added later (Phase 3).

## Dependencies

- Gate B (First Night runtime) green — **done locally, pending CI**.
- Mixamo/Adobe account (free) or Unity Starter Assets for C1/C2.
- Existing `AnimationStateManager`/`CharacterAnimationController` — done.

## Definition of done

- [ ] `PlayerRig.fbx` (Humanoid) + `PlayerRig.prefab` committed.
- [ ] MVP clips (Idle/Walk/Run, 4 Phone, Greeting) committed.
- [ ] `PlayerController.controller` + `UpperBodyMask.mask` committed and assigned.
- [ ] `VerifyAnimationAssets` extended + `verify-animation` CI job green.
- [ ] `AnimationTestComponent` MVP sequence passes headless.
- [ ] Run URL + SHA recorded; `docs/GATE_STATUS.md` Gate C → Verified.

- **MVP locomotion** (Idle/Walk/Run) + **Phone** (Retrieve/Hold/Browse/PutAway) + **Social greeting**.
- **PlayerController** Animator Controller: base locomotion blend tree + additive upper-body overlay layers (per `ANIMATOR_CONTROLLER_SETUP.md`).
- **UpperBodyMask** avatar mask for the phone/social overlay layer.
- Player prefab wired with `Animator` + `CharacterAnimationController` + `AnimationStateManager`.
- `VerifyAnimationAssets` extended to assert the contract, wired into CI as a new job mirroring `verify-unity`.

## Scope (out / deferred)

- Phase 2 (Seating, Food, full Social) and Phase 3 (Nightlife, Vehicle, NPC Work) — manifest-scoped, not required for the player-rig MVP.
- Root-motion vs script-driven locomotion decision (see C4).
- Blend shapes / facial emotes / Animation Rigging — deferred.
