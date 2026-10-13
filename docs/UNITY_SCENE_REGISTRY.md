# NAAD Unity Scene Registry

Last updated: 2026-10-13  
Source of truth for build inclusion: `ProjectSettings/EditorBuildSettings.asset`

## Registered (Build Settings)

| Build index | Scene path | Role | Status |
|---|---|---|---|
| 0 | `Assets/Scenes/Bootstrap.unity` | Application root, auth bootstrap, First Night HUD/controller | Verified (`NAADBuild.Verify` PASS) |

## Required MVP scenes (not yet registered)

| Logical name | Intended path (proposed) | Role | Status |
|---|---|---|---|
| Apartment | `Assets/NAAD/Scenes/Apartment.unity` | Player home / spawn / phone | Planned |
| CityStreet | `Assets/NAAD/Scenes/CityStreet.unity` | Travel / pedestrians / markers | Planned |
| SuyaSpot | `Assets/NAAD/Scenes/SuyaSpot.unity` | Food purchase / NPC encounter | Planned |
| Nightclub | `Assets/NAAD/Scenes/Nightclub.unity` | Event / dance / social | Planned |
| Beach | `Assets/NAAD/Scenes/Beach.unity` | Coast event / late-night close | Planned |

Do not call `SceneManager.LoadScene` for names that are not registered and verified.

## Vendor / demo scenes (not NAAD production)

These exist under third-party folders and must not be treated as NAAD scene contracts until adapted and registered:

- `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_01.unity`
- `Assets/Brick Project Studio/Apartment Kit/Scenes/Scene_02.unity`
- `Assets/Versatile Studio Assets/Demo City By Versatile Studio/Scenes/demo_city_night.unity`
- `Assets/PalmTreePack/Scene/Sample_Palm_Pack.unity`
- Other vendor demo scenes under FreeFlyCamera, Marcos Schultz, Kevin Iglesias, Awbmecreations

## Bootstrap transition note

`NAADBootstrap.nextSceneName` is currently empty in the serialized Bootstrap scene. First Night presentation currently runs inside Bootstrap. When Apartment (or a Main hub) is ready, set `nextSceneName` only to a Build Settings–registered scene name and extend `NAADBuild.Verify` accordingly.
