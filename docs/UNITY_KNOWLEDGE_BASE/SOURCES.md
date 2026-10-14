# Sources — Provenance + Page Pointers

Raw extraction: `C:/Users/user/Downloads/*.pdf` → `_tmp_pdftext/*.txt`
(scratch, gitignored). Distilled Oct 2026. Page numbers = PDF pages.

## 1. unity-game-dev-field-guide.pdf (79 pp, ~110k chars)
- Key features / Prefab workflow / Hub: pp. 6–13
- Editor interface / Play mode / version control (Plastic/Git/Perforce): pp. 15–21
- Project org / folder structure / naming / scenes / hierarchy: pp. 22–28
- GameObjects/components/Inspector/Transform/prefabs/Variants/Nested: pp. 29–36
- Packages / scripting (Start/Update, lifecycle) / C# tips / common classes: pp. 37–43
- Memory (value vs ref, GC/Boehm, fragmentation) / Jobs+Burst / backends: pp. 44–47
- Editor scripting / IDE / templates / builds / Device Simulator: pp. 47–53
- Input (Input System vs built-in class) / UI (UGUI vs IMGUI): pp. 54–57
- Profiling/optimization (Profiler, markers): pp. 58–59+

## 2. Introduction_to_DOTS_Unity_6_edition_1.pdf (60 pp, ~102k chars)
- Performance problem (GC/compiler/cores/cache): pp. 6–8
- Job System / scheduling / safety / Burst / Collections / Mathematics: pp. 9–15
- ECS (archetypes/chunks/queries/jobs) / subscenes+baking / streaming: pp. 16–21
- Entities Graphics (URP/HDRP BatchRendererGroup) / Physics / Havok: p. 22
- Netcode for Entities (authoritative server + prediction) vs GOs: pp. 23–24
- Evaluating for existing/new projects (4 reasons): pp. 30–31
- Made-with-DOTS cases; misconceptions appendix (NOT every project): pp. 32–43+

## 3. 2021_ABM-TheGameDesignerPlaybook_EBook.pdf (103 pp, ~150k chars)
- Pitch/one-pager/demo-trailer/marketability: pp. 6–9
- Roles (narrative/level/gameplay/economy): pp. 10–12
- Editor/GameObjects/Scene view/Gizmo/Grid/Gizmos: pp. 13–20
- Package Manager / Prefabs / Asset Store import: pp. 23–36
- Visual scripting / scene teams / modular systems: pp. 37–44
- Input System / character controller / UI+UnityEvents: pp. 45–65
- Levels / FBX Exporter / timing / cameras / Cinemachine / Timeline: pp. 66–86
- Juice (microinteractions/randomness): pp. 87+

## 4. Animation_in_Unity.pdf (121 pp, ~123k chars)
- Animator enablement / pipeline stages: pp. 8–11
- Motion libs (Mixamo) / Maya+Blender export (bake, full character): pp. 14–18
- Import (mesh/materials/skeleton/avatar/clips/FBX root, Generic vs Humanoid): pp. 19–25
- Controller (params/layers/blend trees/character controller): pp. 26–39
- Animation window / shortcuts / preview-vs-record / events: pp. 40–50
- Root motion / blend shapes / Humanoid / Rigging+IK / Timeline: pp. 51–80+

## 5. Modular_game_architecture_with_ScriptableObjects_Unity_6_Final.pdf (76 pp, ~90k chars)
- What are SOs / serialization / YAML / VS MonoBehaviour: pp. 6–14
- Creation/lifecycle (CreateAssetMenu/CreateInstance/Destroy, callbacks): pp. 15–16
- Data containers / duplicate-data reduction: pp. 17–20
- Patterns: extendable enums / delegates / pluggable behavior / AI: pp. 34–44
- Observer / no-singletons / event channels / Action vs UnityAction / debugging: pp. 45–54
- InputReader / static-vs-instance events / Command / RuntimeSet / UIArch: pp. 55–70+

## Web (fetched Oct 2026)
- Unity 6 best-practice guides hub (art/design, DevOps, graphics, perf, scripting):
  `docs.unity3d.com/6/Documentation/Manual/best-practice-guides.html`
- UI Toolkit advanced-dev guide index (UXML/USS, binding, perf, localization):
  `docs.unity3d.com/6000.7/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/bpg-uiad-index.html`
- UI system comparison (uGUI recommended runtime; Toolkit alternative; IMGUI editor):
  `docs.unity3d.com/Manual/UI-system-compare.html`
- Unity Services solutions (samples: Auth/Lobby/Relay/Cloud Code/Cloud Save/Economy):
  `docs.unity.com/en-us/services/solutions`
- Netcode for GameObjects 2.5 manual (platforms incl. WebGL 1.2+/UTP 2.0+):
  `docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/index.html`
