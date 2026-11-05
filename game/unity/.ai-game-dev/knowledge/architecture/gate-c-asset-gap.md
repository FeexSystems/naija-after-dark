# Gate C Asset Sourcing — RESOLVED (2026-11-05)

> **This note said "Blocked" and is now wrong.** Kept as a record of the error,
> because the correction is the useful part.

## What this note claimed (2026-11-02)

That the eight Gate C clips could not be sourced from anything in the project, and
that Gate C was blocked on human asset acquisition.

## What was actually true

Commit `3b0749c` — `feat(gate-c): replace placeholder clips with real Mixamo takes`
— landed on `main` on **2026-11-02 at 16:48**, i.e. *after* this note was written.
It replaced all eight placeholders: 672,477 inserted lines, every clip now
`m_Name: mixamo.com` muscle curves. `Assets/NAAD/Editor/ImportMixamoClips.cs` maps
each target `.anim` to a source take under `Animations/NAADASSETSNEW/` and copies
the clip without touching the target `.meta`, so controller bindings survive.

Verify by reading the asset, not the doc:

```bash
git log --oneline -1 -- game/unity/Assets/NAAD/Animation/Animations/Locomotion/Idle.anim
head -13 game/unity/Assets/NAAD/Animation/Animations/Locomotion/Idle.anim   # m_Name: mixamo.com
```

## The real Gate C defect (also fixed 2026-11-05)

The clips were fine; the **wiring** was not. `PlayerController.controller` had
zero states, zero motions and `m_Mask: {fileID: 0}` on every layer, and
`PlayerRig.prefab` had no MonoBehaviours and `m_Controller: {fileID: 0}`. Root
cause was a genuine bug in `ConfigureGateCAssets.EnsureLayer`: it mutated a layer
and then located its slot with `Array.IndexOf(controller.layers, layer)`, but every
`controller.layers` read returns a fresh array, so the lookup returned `-1` and the
mask write was silently dropped. Fixed by mutating a local array copy and assigning
the whole array back.

## Lesson

A gate that *regenerates* the artifact before verifying it (`ConfigureAndVerify`)
proves nothing about what is committed. The CI job now runs
`VerifyAnimationAssets.RunVerification` — verify only.

Related: [Gate Status & Doc Drift](/architecture/gate-status-drift.md).
