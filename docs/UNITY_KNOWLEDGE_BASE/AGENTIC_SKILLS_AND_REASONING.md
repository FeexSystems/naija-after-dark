# Agentic Skills + Reasoning — How to Work on NAAD Unity

How this agent reasons on NAAD Unity tasks, which tooling it may rely on, and
which "durable facts" it should stop carrying around because they go stale.

Last verified against the repo: **2026-10-24** (`dd9eb98`). Anything dated is a
snapshot, not a promise; re-run the check command before quoting it.

## Facts about this repo (verify with the command, don't trust the memory)

| What | Fact | Verify |
|---|---|---|
| Repo root | `c:/NAAD/naija-after-dark` — this checkout is authoritative | `git rev-parse --show-toplevel` |
| Primary Unity project | `game/unity/` (`AGENTS.md:15`); 186 `.cs` incl. vendored `Assets/LLMUnity/` | `Get-ChildItem game/unity/Assets -Recurse -Filter *.cs \| Measure-Object` |
| Secondary Unity project | `naad-fntest/` at repo root — **tracked, documented** (`AGENTS.md:16`), 38 `.cs`, 7 manifest deps, no UniTask/URP/InputSystem. A fixture, **not** dead weight: do not delete it to "clean up" | `git ls-files naad-fntest \| Measure-Object` |
| Probe project | `.naad-authtest/` — 1 tracked file (`Packages/manifest.json`), no `NAAD/` tree | `git ls-files .naad-authtest` |
| `game/unity/Packages/manifest.json` | 78 dependencies — the real wiring; `LLMUnity` and vendor packs live here | `python -c "import json;print(len(json.load(open('game/unity/Packages/manifest.json'))['dependencies']))"` |
| Unity gate editor | `6000.6.5f1` — `ProjectSettings/ProjectVersion.txt` **and** all five CI jobs (`.github/workflows/naad-unity.yml:52,86,119,153,214`) | `Get-Content game/unity/ProjectSettings/ProjectVersion.txt` |
| Second install | `6000.7.0b4` exists at `C:/Program Files/Unity/Hub/Editor/`. Never a gate editor: headless hangs at `Application.AssetDatabase Initial Refresh Start` (`docs/GATE_STATUS.md`) | `Get-ChildItem 'C:/Program Files/Unity/Hub/Editor'` |
| `docs/GATE_STATUS.md` | **Stale** on branch (`feat/audit-gate-recovery-2026-10-14`) and on "actual editor `6000.7.0b4`". Trust `ProjectVersion.txt` + CI, not that header | `git rev-parse --abbrev-ref HEAD` |
| Branch / HEAD | Do **not** memorize. Was `chore/unity-mcp-scaffolding-and-gitignore` @ `dd9eb98` ahead 2 on 2026-10-24 | `git rev-parse --abbrev-ref HEAD; git log -1 --oneline; git status -sb` |
| Duplicate trees | `unity-validation/`, `unity-cli-validation/`, `unity-validation-backup-*/` are untracked near-copies of `Assets/NAAD`. Never edit or grep them for findings — they triple-report the same bug | `git status --ignored --short \| Select-String validation` |

## Environment quirks (M-series, kept because they still bite)

- **M4. Lockfile.** A live `Unity` process or `Temp/UnityLockfile` blocks
  Batchmode. Kill the PIDs, confirm the lockfile is gone, then run headless.
- **M5. Staged ghosts.** Empty/placeholder files show as staged `A` even after
  `git reset` if still on disk. Always check `git status --short` scope before
  committing.
- **M6. Scratch dirs.** `_tmp_*/` and `_tmp_pdftext/` are local extraction
  scaffolding (`.gitignore:140`), may not exist. The KB in
  `docs/UNITY_KNOWLEDGE_BASE/` is the durable artifact.
- **M7. Ignored dirs.** `unity_best_practices/` (reference PDFs) and
  `unity-agent-plugin/` (local tool checkout) are gitignored (`.gitignore:160-162`).
  `unity-agent-plugin/` is **absent on a fresh clone** — never depend on its
  skills existing; the sections below degrade gracefully when it is missing.
- **M8. `.git` hygiene.** Unreachable loose objects bloat `.git`; `git prune` is
  safe when git warns (measured 211.5 MB → 5.1 MB on 2026-10-17), and `git fsck`
  afterwards should show only dangling blobs.

## Tooling that actually exists here

- **Content:** read files directly (`read`), find by pattern (`grep`), by name
  (`glob`). Names like `read_files`/`search_codebase` are not available.
- **Shell:** PowerShell. `Select-String`, `Get-Content -Raw`, `Select-Object`,
  `Test-Path` are reliable. If a command's output looks truncated, redirect to a
  file and read it (`... | Out-File x.txt; Get-Content x.txt`).
- **Host caveat:** `grep`/`rg` and Bash-only syntax are not always available on
  this host. When a pattern search fails, fall back to a scoped `python -c`
  one-liner rather than scanning the whole tree.
- **Python:** available (`python`), used for the Blender pipeline tests and
  quick file probes. **Node:** v24, runs `.ts` tests with
  `node --experimental-strip-types`.

## Reasoning flows (plan before tools; batch independent calls)

1. **Explore**: read `AGENTS.md`, the task's named files, the scene registry,
   and `Packages/manifest.json` in one parallel batch. Do not read
   `Library/`, `PackageCache/`, or the duplicate validation trees.
2. **Diagnose**: name the blocker class (compile CS0234 / YAML script ref /
   lockfile / missing prefab / pipeline gap) with file:line evidence.
3. **Fix minimal**: smallest edit honoring G1–G9; runtime vs Editor assembly
   check for any prefab-touching type; setter check for any author-assigned
   property.
4. **Verify headless** (real method paths — these are `-executeMethod` strings):
   - `NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders` → logs
     `[NAAD][Prefabs] Phase-1 placeholders authored (6/6).`
   - `NAAD.Editor.NaadScenePipeline.BuildApartmentAndCityStreet` (calls
     `ValidateScenes` itself), or `NAAD.Editor.NaadScenePipeline.ValidateScenes`
     alone.
   - `NAAD.Editor.NAADBuild.Verify` — the CI entry point and the one that sets a
     real exit code.
   - **Exit-code trap:** `ValidateScenes` only logs its errors
     (`NaadScenePipeline.cs:54-80`); it does not call `Exit`. A failed validation
     under `-batchmode -quit` still returns 0. Use `NAADBuild.Verify`, or grep
     the log for `error CS:`, `Validation`, and the counts above.
5. **Report**: changed files, commands, log lines, git scope, risks (G10).

## Skill wiring (unity-agent-plugin — optional, absent on fresh clones)

If `unity-agent-plugin/` is checked out, these map cleanly. If it is missing, use
the plain Unity/`UnityEditor.PackageManager` path and do not pretend a skill ran.

- Start project/scaffold → `new-unity-project` (delegates: `unity-cli`,
  `unity-package-management`, visual baseline `urp-postprocessing`/`ui`).
- Drive an Editor headless/CI → `unity-cli`; NAAD's own equivalent is the
  `-executeMethod` list above.
- Packages → `unity-package-management` (C# Client API). Editing
  `Packages/manifest.json` by hand is allowed for a deliberate version pin or a
  local `file:`/`git:` dependency — it is the documented wiring for `LLMUnity`
  and vendor packs — but prefer the Package Manager client for adds/removes and
  re-open the Editor afterwards.
- HUD/settings → `ui` → `ui-ugui` (current, G8); `ui-uitk` FUTURE only;
  `ui-imgui` for Editor debug tools; text → `optimize-text-mesh-pro`;
  localization → `localization`.
- Perf → `optimize-audio`, `optimize-web`; DOTS guidance = Jobs/Burst without
  ECS, but G9 forbids Entities/ECS this gate.
- Multiplayer/services → `setup-multiplayer-services` + `build-live-game`, but
  NAAD authority = Supabase/game-api: map UGS concepts, never install Netcode.
- Shaders/VFX → `shader-graph-create-custom-node`, `urp-postprocessing`
  (FUTURE — Built-in freeze G5); 2D → `2d-pixel-perfect`,
  `create-tile-palette…`, `manage-sprite-atlas`, `sprite-editor`,
  `sprite-segment-3x3grid`; physics → `create-2d-physics` /
  `physics-3d-collision`; nav → `initialize-ai-navigation`; GTK → `build-gtk`;
  IAP/ads/voice → `implement-in-app-purchases`, `levelplay-unity-integration`,
  `setup-vivox-voice-chat`; migration → `migrate-birp-to-urp` (FUTURE);
  search → `generate-editor-search-query`; audio buses → `audio-setup-mixers`;
  render-graph check → `validate-urp-render-graph-renderer-feature`.

## Citations for future work

Field Guide → FIELD_GUIDE_CORE / SCRIPTING_PERFORMANCE_JOBS_BURST;
Playbook → DESIGNER_PLAYBOOK_SYSTEMS; SO guide → SCRIPTABLEOBJECT_ARCHITECTURE;
DOTS → DOTS_UNITY6_GUIDE; Animation → ANIMATION_UNITY;
UI Toolkit/Services → UI_TOOLKIT_VS_UGUI. Rules → GUARDRAILS.md.
