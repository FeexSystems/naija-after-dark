# Duplicate Scene Copies Re-GUID the Tracked Ones

**Gotcha (verified 2026-11-05):** if two copies of the same scene exist anywhere
under `Assets/`, Unity resolves the GUID collision by **rewriting the tracked
copy's `.meta` with a brand-new GUID** and repointing references at whichever copy
it imported first. The change is silent, staged-looking, and lands in
`ProjectSettings/EditorBuildSettings.asset` plus every affected `.unity.meta`.

## What it looked like here

`Assets/NAAD/Scenes/*.unity` (tracked) and `Assets/NAAD/Prefabs/Scenes/*.unity`
(untracked, byte-identical) coexisted. Result:

| File | Before | After |
|---|---|---|
| `Assets/NAAD/Scenes/Apartment.unity.meta` | `4e1906ad…` | `e19fcd4c…` |
| `EditorBuildSettings.asset` scene path | `Assets/NAAD/Scenes/Apartment.unity` | `Assets/NAAD/Prefabs/Scenes/Apartment.unity` |

The original GUIDs ended up living only in the **untracked** directory, so a clean
checkout would have had five build scenes pointing at a path that does not exist.

## Why it is nasty

`NaadScenePipeline.ValidateScenes` compares build entries by exact path against
`NaadSceneNames.*Path` (which is `Assets/NAAD/Scenes/…`). So the corruption shows
up as `Build Settings missing enabled scene: …` rather than as a GUID problem,
which points you at the wrong file.

## Check and repair

```bash
# 1. Are there duplicate scene files?
find game/unity/Assets -name "*.unity" | xargs -n1 basename | sort | uniq -d

# 2. Do tracked scenes still own their committed GUIDs?
git diff -- game/unity/Assets/NAAD/Scenes/*.meta game/unity/ProjectSettings/EditorBuildSettings.asset
```

Repair order matters: **remove the duplicate tree first**, then restore the GUIDs —
otherwise Unity re-collides on the next import.

```bash
mv game/unity/Assets/NAAD/Prefabs/Scenes ../_naad-quarantine/   # move, don't delete
git restore --staged --worktree \
  game/unity/Assets/NAAD/Scenes.meta \
  'game/unity/Assets/NAAD/Scenes/*.meta' \
  game/unity/ProjectSettings/EditorBuildSettings.asset
python tools/validate-unity-meta.py game/unity   # expect 0 problems
```

Related: [Gate Status & Doc Drift](/architecture/gate-status-drift.md),
[Gate C Asset Sourcing](/architecture/gate-c-asset-gap.md).
