# Gate C Animation Clips — Provenance

> **Corrected 2026-11-05.** This file previously described all eight clips as
> placeholders that "are **not** production Mixamo takes". That stopped being true
> when `3b0749c feat(gate-c): replace placeholder clips with real Mixamo takes`
> landed on `main` on 2026-11-02. The stale text was itself an audit finding.

All eight clips under `Animations/` now contain real humanoid muscle-space takes
(`m_Name: mixamo.com`, stored as muscle curves, so they retarget onto `PlayerRig`).

## Takes

| Clip | Path | Source take | Notes |
|------|------|-------------|-------|
| Idle | `Locomotion/Idle.anim` | Neutral Idle | loop |
| Walk | `Locomotion/Walk.anim` | Feminine Walk Forward | loop |
| Run | `Locomotion/Run.anim` | Running Forward Quickly | loop |
| Retrieve | `Phone/Retrieve.anim` | Searching Front And Back Pockets | one-shot |
| Browse | `Phone/Browse.anim` | Female Using Touchscreen Tablet | loop |
| Hold | `Phone/Hold.anim` | *derived* from Female Using Touchscreen Tablet | **not a dedicated take** |
| PutAway | `Phone/PutAway.anim` | *derived* from Neutral Idle | **not a dedicated take** |
| Greeting | `Social/Greeting.anim` | Quick Informal Bow | one-shot |

`Hold` and `PutAway` are intentionally derived — the source pack has no phone
hold/put-away take. Revisit when a dedicated take exists.

## How they are produced

`Assets/NAAD/Editor/ImportMixamoClips.cs` maps the eight target `.anim` files to
source FBX takes under `Animations/NAADASSETSNEW/`, imports each FBX as Humanoid,
and `EditorUtility.CopySerialized`s the first clip onto the target. It never
touches the target `.meta`, so GUIDs — and therefore every
`PlayerController.controller` binding — stay stable.

Run: **NAAD > Gate C > Import Mixamo Clips**, or headless
`-executeMethod NAAD.Editor.ImportMixamoClips.Run`.

## Known limitation

`Animations/NAADASSETSNEW/` (~77 MB, 268 files) is **not committed**. The baked
`.anim` clips are, which is what ships; but it means `ImportMixamoClips.Run`
cannot regenerate them from a clean checkout. Re-download the takes from Mixamo
or commit the source folder if regeneration needs to be reproducible.
