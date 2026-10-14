# Field Guide Core — Project Org, Prefabs, Scenes (NAAD-applied)

Source: Unity Game Dev Field Guide §§ Project organization / Prefabs / Scenes.
Project: `game/unity`, Unity 6000.6.5f1, Built-in RP.

## Rules
- Lock Editor version: `ProjectSettings/ProjectVersion.txt` = 6000.6.5f1.
  Never upgrade mid-gate. Hub ties project→version so updates can't break prod.
- Folder style: `Assets/NAAD/<Area>/` (AI, Animation, Audio, Content, Core,
  Editor, Gameplay, Networking, Prefabs, Scenes, UI, World). No spaces in
  file/folder names (CLI breaks). No extra root folders.
- Third-party stays separated: `Assets/<Publisher>/` (Brick Project Studio,
  Versatile Studio, etc.) — ALL gitignored (~1.05 GB, incl. 291 MB
  LightingData.asset over GitHub's 100 MB cap). Fresh clones re-import;
  NAAD code must build with prefab-first + primitive fallback.
- Force Text serialization: scenes/prefabs/SOs are YAML — reviewable, mergeable.
- Every `.cs`/asset commits with its `.meta`. Never commit `Library/ Temp/
  obj/ Build/ Logs/`.
- Style guide lives here + `Content/ASSET_NAMING_CONVENTION.md`
  (`[Category]_[Subcat]_[Variant]_[Detail]`). Rename in bulk via script, never drift.

## Prefabs (guide: build once, instance; Variants + Nested)
- Author reusable `Assets/NAAD/Prefabs/Environment/` modules (Gate B Phase-1:
  RdSurfaceAsphalt, StrSidewalkStraight, SyuStandStructure, SyuGrillOpen,
  NclEntranceDoorLarge, NclStagePlatform) from primitives with URP-safe
  materials (`URP/Lit ?? Standard`).
- Tag placeholders with runtime `NAAD.World.NaAdPlaceholderTag`
  (assetId+description) — NEVER an Editor-assembly type (prefab YAML can't
  resolve it; `m_Script:{fileID:0}` = broken).
- Nested prefabs over duplication; Variants for district skins (ML/IS/CR/CS).
- `NaadScenePipeline.TryInstantiate(path)` warns + falls back to `CreateBox`
  so missing vendor packs never break Batchmode builds.

## Scenes (guide: modular scene flow via SceneManagement API)
- Registry: `docs/UNITY_SCENE_REGISTRY.md`; truth =
  `ProjectSettings/EditorBuildSettings.asset`. Index 0 = Bootstrap.
- Only `SceneManager.LoadScene` names that are registered + verified
  (`NaadScenePipeline.ValidateScenes`: file exists, build entry enabled,
  PlayerSpawn + ExitTravel anchors, MainCamera tag).
- Organize hierarchy with named empty spacers; zero parent transforms before
  adding children; real-world scale from the start.

## Applies to NAAD
Gate B done-state = 6 tagged prefabs import clean, CityStreet builds headless
via `BuildApartmentAndCityStreet`, `ValidateScenes` passes, vendor packs absent.
