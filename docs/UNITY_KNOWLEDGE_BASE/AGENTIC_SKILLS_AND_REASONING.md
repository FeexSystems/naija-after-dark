# Agentic Skills + Reasoning — Memories for Future Unity Builds

How this agent reasons on NAAD Unity tasks, which skills to invoke, and what
to remember across sessions.

## Memories (persist these)
- M1. Active project = `c:/Users/user/naija-after-dark-1/game/unity`
  (NOT the stale `naija-after-dark` clone). Branch `main@ce3ac67`, Gate B WIP.
- M2. Unity 6000.6.5f1 at `C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe`;
  second install 6000.7.0b4 exists — NEVER use beta for gates.
- M3. Shell capture is unreliable here: prefer `read_files` for content;
  for commands, redirect to file then read (`... | Out-File x.txt; Get-Content x.txt`).
  `search_codebase` times out on this repo — scope greps via python one-liners.
- M4. Live `Unity` PIDs + `Temp/UnityLockfile` block Batchmode: kill PIDs, confirm
  lockfile gone, then run headless.
- M5. Git quirk: empty/placeholder files show as staged `A` even after `git reset`
  if still on disk — check `git status --short` scope before committing.
- M6. `_tmp_*` scratch + `_tmp_pdftext/` + `_tmp_pylibs/` are LOCAL extraction
  scaffolding — gitignored, never committed. KB in `docs/UNITY_KNOWLEDGE_BASE/`
  is the durable artifact.

## Reasoning flows (plan before tools; batch independent calls)
1. **Explore**: read AGENTS/README/ARCHITECTURE/DEVELOPMENT + registry + manifest
   + the exact files named in the task. One parallel `read_files` batch.
2. **Diagnose**: name the blocker class (compile CS0234 / YAML script ref /
   lockfile / missing prefab / pipeline gap) with file:line evidence.
3. **Fix minimal**: smallest edit honoring G1–G9; runtime vs Editor assembly check
   for any prefab-touching type; setter check for any author-assigned property.
4. **Verify headless**: kill stale Editors → `AuthorPlaceholders` →
   `BuildApartmentAndCityStreet` → `ValidateScenes`/`Verify*`; grep logs for
   `error CS:0`, `Saved (6/6)`, `Validation passed`.
5. **Report**: changed files, commands, log lines, git scope, risks (G10).

## Skill wiring (unity-agent-plugin, 35 skills)
- Start project/scaffold → `new-unity-project` (delegates: `unity-cli`,
  `unity-package-management`, visual baseline `urp-postprocessing`/`ui`).
- Drive open Editor headless/CI → `unity-cli` (install/auth/build/test flows).
- Packages → `unity-package-management` (C# Client API; NEVER hand-edit manifest).
- HUD/settings screens → `ui` → `ui-ugui` (current); `ui-uitk` FUTURE only;
  `ui-imgui` for Editor debug tools; text → `optimize-text-mesh-pro`;
  localization → `localization`.
- Perf → `optimize-audio`, `optimize-web`, DOTS doc says Jobs/Burst-without-ECS.
- Multiplayer/services → `setup-multiplayer-services` + `build-live-game`,
  but NAAD authority = Supabase/game-api (map UGS concepts, don't install Netcode).
- Shaders/VFX → `shader-graph-create-custom-node`, `urp-postprocessing`
  (FUTURE — Built-in freeze G5); 2D → `2d-pixel-perfect`, `create-tile-palette…`,
  `manage-sprite-atlas`, `sprite-editor`, `sprite-segment-3x3grid`;
  physics → `create-2d-physics` / `physics-3d-collision`; nav → `initialize-ai-navigation`;
  GTK → `build-gtk`; IAP/ads/voice → `implement-in-app-purchases`,
  `levelplay-unity-integration`, `setup-vivox-voice-chat`;
  migration → `migrate-birp-to-urp` (FUTURE); search → `generate-editor-search-query`;
  audio buses → `audio-setup-mixers`; render-graph check →
  `validate-urp-render-graph-renderer-feature`.

## Citations for future work
Field Guide → FIELD_GUIDE_CORE / SCRIPTING_PERFORMANCE_JOBS_BURST;
Playbook → DESIGNER_PLAYBOOK_SYSTEMS; SO guide → SCRIPTABLEOBJECT_ARCHITECTURE;
DOTS → DOTS_UNITY6_GUIDE; Animation → ANIMATION_UNITY;
UI Toolkit/Services → UI_TOOLKIT_VS_UGUI. Rules → GUARDRAILS.md.
