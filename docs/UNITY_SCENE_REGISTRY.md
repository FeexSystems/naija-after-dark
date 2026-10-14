# NAAD Unity Scene Registry

Last updated: 2026-10-14 (audit `feat/audit-gate-recovery-2026-10-14`)
Source of truth for build inclusion: `ProjectSettings/EditorBuildSettings.asset`

## Registered (Build Settings)

Verified 2026-10-14 via `NAAD.Editor.NAADBuild.Verify` (exit 0) and
`NAAD.Editor.NaadScenePipeline.ValidateScenes` (exit 0), editor `6000.6.5f1`.

| Build index | Scene path | Role | Status |
|---|---|---|---|
| 0 | `Assets/Scenes/Bootstrap.unity` | Application root, auth bootstrap, First Night HUD/controller | Verified (`NAADBuild.Verify` PASS) |
| 1 | `Assets/NAAD/Scenes/Apartment.unity` | Player home / spawn / phone | Verified (`ValidateScenes` PASS) |
| 2 | `Assets/NAAD/Scenes/CityStreet.unity` | Travel / pedestrians / markers | Verified (`ValidateScenes` PASS) |
| 3 | `Assets/NAAD/Scenes/SuyaSpot.unity` | Food purchase / NPC encounter | Verified (`ValidateScenes` PASS) |
| 4 | `Assets/NAAD/Scenes/Nightclub.unity` | Event / dance / social | Verified (`ValidateScenes` PASS) |

## Required MVP scenes (not yet registered)

| Logical name | Intended path (proposed) | Role | Status |
|---|---|---|---|
| Beach | `Assets/NAAD/Scenes/Beach.unity` | Coast event / late-night close | Planned (server flow uses `NightDistrictIds.Beach` but no scene file/build entry exists) |

Do not call `SceneManager.LoadScene` for names that are not registered and verified.

## Vendor / demo scenes (not NAAD production)

These exist under third-party folders and must not be treated as NAAD scene contracts until adapted and registered:

- `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_01.unity`
- `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_02.unity`
- `Assets/Versatile Studio Assets/Demo City By Versatile Studio/Scenes/demo_city_night.unity`
- `Assets/PalmTreePack/Scene/Sample_Palm_Pack.unity`
- Other vendor demo scenes under FreeFlyCamera, Marcos Schultz, Kevin Iglesias, Awbmecreations

## Bootstrap transition note

`NAADBootstrap.nextSceneName` is currently empty in the serialized Bootstrap scene (verified 2026-10-14 against `Assets/Scenes/Bootstrap.unity`). First Night presentation currently runs inside Bootstrap. Apartment, CityStreet, SuyaSpot and Nightclub are now registered and validated, but no verified scene-transition code paths load them yet — the travel flow is authoritative-server travel (`GameCommandType.Travel`), not scene loading. Set `nextSceneName` only to a Build Settings–registered scene name and extend `NAADBuild.Verify` accordingly when a transition is added.

## Unverified verifier (known drift)

`NAAD/Editor/VerifyEnvironmentScenes.cs` checks for `Assets/NAAD/Content/Scenes/Blockout/*.unity` scenes that do not exist in the current tree and exposes no Batchmode entry point. It is not part of the CI gate and its requirements do not match this registry. Do not use it as scene evidence until reconciled.
