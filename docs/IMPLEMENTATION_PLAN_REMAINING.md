# NAAD — Remaining Work Implementation Plan

**Created:** 2026-11-02 · **Branch:** `feat/unity-mcp-integration` · **Editor:** `6000.6.5f1`
**Basis:** direct inspection of the working tree during the 2026-11-02 audit.

This plan supersedes the "next steps" sections of `PROJECT_PLAN.md`,
`GATE_STATUS.md` and `PRODUCTION_READINESS_STATUS.md` where they disagree. Those
three documents describe the project at three different dates and all of them
understate the current state of Gate C and Gate D.

## Progress log

**2026-11-05 — audit + repair, verified locally in the running Editor:**

| Verified | Evidence |
|---|---|
| Gate C contract | `VerifyAnimationAssets.RunVerification` → `success: true`, **37/37**, 0 failures |
| Scene registry | `NaadScenePipeline.ValidateScenes` passes, now also enforcing anchor→command mapping |
| Metadata gate | `validate-unity-meta.py` → 2503 files, 0 problems |
| Backend typecheck + tests | `npm run build --workspaces` clean; both suites pass |

Corrected findings (the docs below were wrong about these):

- **W1 is not blocked on asset acquisition.** `3b0749c` (2026-11-02, on `main`)
  replaced all eight placeholder clips with real Mixamo takes. What was actually
  broken was the *wiring*: `PlayerController.controller` had zero states and every
  layer persisted `m_Mask: {fileID: 0}`, and `PlayerRig.prefab` had no
  MonoBehaviours and `m_Controller: {fileID: 0}`. Root cause was a real bug in
  `ConfigureGateCAssets.EnsureLayer` (mutated a layer, then looked it up with
  `Array.IndexOf` against a freshly-built array → `-1` → silent no-op). Fixed.
- **W2 is not inert.** There is only one `SceneAnchor` type (`NAAD.World`), and
  `SceneAnchorCommandMap` resolves contentIds to commands. `PlayerInteractionController`
  binds `NAAD.World.SceneAnchor` and filters via `TryResolve`. The missing piece was
  that `UnmappedContentIds` was never called by the validator — now wired in.
- **W0's premise was inverted.** The working tree was not "28 modified / 117
  deleted" — it was 15 tracked modifications plus a **duplicated scene tree**:
  untracked `Assets/NAAD/Prefabs/Scenes/*.unity` (byte-identical copies) held the
  original GUIDs, `EditorBuildSettings.asset` had been repointed at them, and the
  tracked `Assets/NAAD/Scenes/*.meta` had been re-GUIDed. That broke
  `ValidateScenes` (exact-path comparison against `NaadSceneNames`). The duplicate
  tree was quarantined outside the repo, the GUIDs and build settings restored, and
  the token-bearing `game/unity/.mcp.json` (plus `.claude/`/`.roo/`/`.agent/`
  scaffolding) unstaged and gitignored.

**2026-11-02 — commit `33bccc8`** (branch `feat/unity-mcp-integration`), verified locally:

| Verified | Evidence |
|---|---|
| Unity recompiles clean after the C# edits | `Assembly-CSharp-Editor.dll` rebuilt 07:57:30, 0 `error CS` |
| Metadata gate passes | `validate-unity-meta.py` → 2298 files, 0 problems, exit 0 |
| Backend typecheck passes | `npm run build --workspaces` exit 0 (was failing with TS5097) |
| Backend tests pass | `validate.test.ts: ok`, `contracts.test.ts: all assertions passed` |

Landed from this plan: **W0** (build/CI/docs fixes — CI result still pending),
part of **W5** (submodule initialised, backend build scoping, legacy-define hazard
documented), part of **W6** (`[functions.npc-dialogue]` JWT config), **W7**
(ledger consolidated, stale counts corrected).

Still open below: W1, W2, W3, W4, and the remainder of W5/W6.

> **Confidence note:** every item below was verified by reading code, assets or
> config in the working tree. Two automated deep-dives (a full C# client sweep and a
> SQL/contract-vs-`naad_execute_command` sweep) were started but never returned
> results, so additional defects in those areas may surface. Budget for that.

---

## 1. Where the project actually stands

| Area | Reality (verified) | Blocker? |
|---|---|---|
| Unity compile | Clean — 0 `error CS` in `Logs/Editor.log` | No |
| Uncommitted work | 28 modified, 117 deleted, 1 renamed, 43 untracked | **Yes** |
| Trust boundary | Client only calls `/rpc/*`; anon key only; `SupabaseConfig.cs:26` rejects `service_role` | No |
| Metadata integrity | Validator passes — 2369 `.meta`, 0 problems (after the 31-char GUID repair) | No |
| Scenes | 5 registered + Bootstrap; all exist; `SceneAnchor` present in all 5 | No |
| Gate C (rig/animation) | Contract wired, **8 placeholder clips**, rig has `clipAnimations: []` | **Yes — needs external assets** |
| Gate D (gameplay) | Framework present but **probes a `SceneAnchor` type that no scene contains** — inert at runtime (see W2) | **Yes — 1 real defect** |
| Gate E (atmosphere) | `WorldClockPresenter` exists; **no lighting profile assets**, only a `.md` spec | No |
| Gate F / 16 (mobile) | No device acceptance, no size/perf baseline | No |
| AI dialogue (Phase C) | Edge function present; deployment + `GEMINI_API_KEY` unverified | Unknown |

---

## 2. Workstreams

Each workstream lists **objective → tasks → exit criteria → dependencies**.
Sizes are T-shirt, not estimates in hours: S ≈ half a day, M ≈ 1–3 days,
L ≈ a week or more.

---

### W0 — Land the current working tree · **S** · blocks everything

The audit's fixes are all uncommitted, and **the Unity compile after the C# edits
has not been observed.** `Assembly-CSharp-Editor.dll` is still stamped 03:30 while
the edits landed 06:53–06:54.

- [ ] Focus the Unity window; confirm the console is clean after recompile.
- [ ] Re-run `tools/validate-unity-meta.py game/unity` (expect exit 0).
- [ ] Commit the audit fixes (`BuildMethods`, meta validator, GUID repair, verifier
      guards, `com.unity.splines` pin, MCP defines, dependabot, CI filter, docs).
- [ ] Push and open the PR — the branch filter is now `feat/**`, so push should
      trigger CI; confirm **all seven Unity jobs** run.
- [ ] Record the run URL + SHA into `docs/GATE_STATUS.md`.

**Exit:** green CI on a pushed commit, with the run linked from the ledger.

> The 117 staged deletions include `naad-fntest/`, which held `SceneLoader.cs`,
> `GamePhase.cs`, `GameStateManager.cs` and `RunVerify.cs` — scripts that exist
> nowhere else. Decide whether any of them are wanted *before* committing.
> Recovery: `git checkout HEAD -- naad-fntest`.

---

### W1 — Gate C: real player animation · **DONE 2026-11-05**

> **This section was wrong.** The eight clips are **real Mixamo takes**, landed in
> `3b0749c` on `main` on 2026-11-02 — a commit that post-dates the audit this plan
> was written from. No human asset acquisition was ever outstanding. What was
> actually broken was the *wiring*: `PlayerController.controller` had zero states
> and persisted `m_Mask: {fileID: 0}` on every layer, and `PlayerRig.prefab` had no
> MonoBehaviours and `m_Controller: {fileID: 0}`. Root cause was a real bug in
> `ConfigureGateCAssets.EnsureLayer` (mutated a layer, then looked its slot up with
> `Array.IndexOf` against a freshly-built array → `-1` → silent no-op). Fixed, and
> `VerifyAnimationAssets` now reports 37/37.

Remaining for Gate C:

- [ ] Play-test on device or in Editor: move, phone in/out, greet. The contract job
      proves wiring, not motion.
- [ ] Decide whether to commit `Animations/NAADASSETSNEW/` (~77 MB of source takes).
      It is gitignored today, so `ImportMixamoClips.Run` cannot regenerate from a
      clean checkout — the baked `.anim` clips are what ship.
- [x] Provenance recorded in `Animations/MVP_PLACEHOLDER_CLIPS.md` (the file name is
      now wrong; the contents are a real provenance table).

**Exit:** thirteen of fourteen bars met; blocked only on observed playback.

> **Do not** wire clips from `Assets/Kevin Iglesias/`. It is gitignored, contains
> only Idles + farming/crafting work, and would reintroduce the clean-checkout
> failure this plan is trying to remove.

---

### W2 — Gate D: gameplay/interaction · **DONE 2026-11-05** (play-test outstanding)

> **This section was wrong.** It described a duplicate `SceneAnchor` type and an
> inert controller. Both were already fixed on `main` before this plan was read
> again: there is exactly one type, `NAAD.World.SceneAnchor`, used by all 22 anchors
> across the five scenes; `SceneAnchorCommandMap` maps each `contentId` to a command
> intent; and `PlayerInteractionController` has `using NAAD.World;` and filters
> candidates through `TryResolve`.

What was genuinely missing: `SceneAnchorCommandMap.UnmappedContentIds` existed for
exactly this purpose but **was never called**, so an unmapped marker would have
failed silently at runtime instead of failing the scene gate. It is now wired into
`NaadScenePipeline.ValidateAnchors`. Verified statically: 22 anchors, 17 mapped
contentIds, 0 unmapped.

Remaining for Gate D:

- [x] Verify `StubCommandService` is never selected at runtime when a live config is
      present. Verified 2026-11-05: `NAADApplicationRoot.Compose()` branches once on
      `supabaseConfig != null && supabaseConfig.IsConfigured`, and the stub arm logs
      `SupabaseConfig missing — using stub services`. There is no other construction
      site. Minor residual risk: a build shipped without a configured asset degrades
      to stubs with only a warning — consider failing loudly in release.
- [x] **Player wired into all five production scenes (2026-11-06).**
      `WirePlayerIntoScenes.Run()` instantiated `PlayerRig.prefab` at each
      `PlayerSpawn` anchor and added `PlayerInteractionController`; 5/5 wired, its
      `Validate()` passes, and `NaadScenePipeline.ValidateScenes` still passes. Verified
      in Play mode: the rig renders with its Mixamo idle pose and no runtime errors.
      The scenes are dioramas no more.
- [ ] Play-test First Night through real interactions — walk to an anchor, interact,
      and confirm the command reaches the server. The player exists and renders; nobody
      has yet driven it with input.

**Exit:** anchors drive real commands end-to-end on device, and the validator
enforces the mapping (the validator and player-presence halves are now done).

---

### W3 — Gate E: atmosphere / simulation · **L**

Only `WorldClockPresenter` and a spec document exists; there are no lighting assets.

- [ ] Author day/night lighting profiles as real assets (the spec is
      `Assets/NAAD/Content/LIGHTING_PROFILES.md` — it describes intent, not assets).
- [ ] Drive them from `WorldClockPresenter`; the server owns time
      (`naad_get_world_clock`), the client only renders it.
- [ ] Ambient/crowd presentation from presence hints — **display only**; server owns
      counts (pillar #4, ephemeral ≠ persistent).
- [ ] Confirm atmosphere never writes authoritative state (Gate C invariant, applied
      to lighting).

**Exit:** the same scene reads visibly different across night phases, driven by the
server clock, with zero state writes.

---

### W4 — Gate F / Gate 16: mobile and device acceptance · **L**

- [ ] **Measure before optimizing** (the explicit Gate 16 rule). Baseline size, load
      time, FPS, RAM, battery on low/mid/high Android and one iPhone.
- [ ] **Build-size blast radius first:** `Assets/` is **5.1 GB**, of which
      `Assets/StreamingAssets/` is **3.96 GB** of LLM native binaries across six
      platforms (gitignored). A mobile build cannot carry this. Decide what ships.
- [ ] **Resolve the on-device LLM payload — highest-confidence size win, needs a
      product answer.** Measured 2026-11-05:

      | Artifact | Size | Tracked in git | Referenced by game code |
      |---|---|---|---|
      | `Assets/LLMUnity/` (211 files, vendored LLMUnity SDK) | 15 MB | **yes** | **no — zero references** |
      | `Assets/StreamingAssets/LlamaLib-v2.0.5/` (runtime payload) | 3.9 GB | no (gitignored) | no |

      `Assets/LLMUnity/Runtime/undream.llmunity.Runtime.asmdef` declares **no
      `includePlatforms`**, so it compiles into *every* player build, and it ships
      native `libusearch_c.so` for **Android arm64-v8a (1.96 MB) and x86_64
      (1.86 MB)**. A grep across `Assets/`, `*.cs`, `*.unity`, `*.prefab` and
      `*.asmdef` finds no reference to `LLMUnity` or `undream.llmunity` outside
      `Assets/LLMUnity` itself. So the APK carries ~3.8 MB of native code for a
      feature nothing calls, and `Assets/` carries 15 MB of dead SDK.

      CI surfaced this as: `[native-plugin-compatibility] ... usearch/windows/
      libusearch_c.dll` is Editor-visible on Windows hosts only, so on the Linux
      runner the asset is invisible while the Windows DLL is not — a portability
      trap if anyone ever references it unconditionally.

      **Decided 2026-11-05: keep it. On-device LLM is a planned feature, not dead
      weight.** Consequences that now need owning before Gate 16 ships:

      - `undream.llmunity.Runtime` must stop compiling into builds that do not use
        it, or the ~3.8 MB of Android `libusearch_c.so` ships for nothing. Give the
        asmdef an `includePlatforms`, or wire the feature.
      - The Windows-only `libusearch_c.dll` being Editor-visible while the Linux
        container sees nothing is a portability trap — any unconditional reference
        breaks the Linux CI build.
      - `Assets/StreamingAssets/LlamaLib-v2.0.5/` (3.9 GB) stays gitignored; a
        clean clone therefore cannot run on-device inference. Document the fetch
        step or the feature is unbuildable by anyone else.
      - A model file has not been chosen or measured. That is the Gate 16 blocker:
        a mobile LLM payload is tens to hundreds of MB and has to fit a store limit.
- [ ] Reconcile the 14 unused heavy packages: `com.unity.xr.openxr`, `com.unity.ml-agents`,
      `com.unity.services.vivox`, `com.unity.services.leaderboards`,
      `com.unity.services.ccd.management`, `com.unity.ai.assistant`,
      `com.unity.ai.inference`, `com.unity.feature.ecs`, `com.unity.2d.animation`.
      None are referenced by `Assets/NAAD` code.
- [ ] Fix `scriptingBackend: {}` — Android with `AndroidTargetArchitectures: 2`
      (ARM64) implies IL2CPP; the empty map leaves it implicit.
- [ ] Release build smoke: install, launch, First Night → Night Summary, save-state,
      network-failure behaviour.

**Exit:** First Night reaches Night Summary on a physical device, with recorded
measurements and a build size the store will accept.

---

### W5 — Structural & hygiene debt · **M** · parallel, no dependencies

- [ ] **Finish the UI layer.** *Correction to the 2026-11-05 reading below — the two
      folders are **paired**, not competing.*

      `NAAD.UI.Scripts.*View` is the presentation half and `NAAD.UI.*Controller` is the
      logic half; `MainHudView`'s own doc comment says "Bound to
      `NAAD.UI.MainHudController`". The View half is genuinely implemented (real
      `UpdateLocation`/`UpdateBalance`/`ShowInteractionPrompt` methods, events,
      unsubscribe-on-destroy). The Controller half held the data but had **no reference
      to any view**, so every `ShowHud`/`HideHud`/`UpdateHud` was an empty comment —
      and `ScreenNavigationManager.WireController` looked a controller up by
      `FindObjectOfType` inside a method nothing called. `ControllerWiring.WireViewControllers`
      literally logged "View-controller wiring not yet implemented." (and was behind the
      undefined `NAAD_LEGACY_EDITOR_TOOLS` define, so it was dead anyway).

      **Done 2026-11-05** (compiles clean against Unity's assemblies):
      - `BaseScreenController` now owns a `ScreenViewBase`, exposes `BindView()`, calls
        `OnViewBound()` and renders on every state change.
      - `MainHudController` renders `MainHudData` into `MainHudView` and routes the view's
        interaction tap through `OnInteract()`.
      - `ScreenNavigationManager.RegisterControllersAuto()` binds every controller to the
        view whose screen id matches its type name (`MainHudController` → `"MainHud"`),
        called from `Start()`. `WireController` now uses `FindAnyObjectByType`.
      - `NAADApplicationRoot.Compose()` publishes the composed graph through
        `ServiceContainer.RegisterProdServices`, which had never been called — leaving
        every `ServiceContainer.Resolve<T>()` in the UI layer returning null.

      Remaining to finish:
      - [x] All eight controllers now render into their views and route view events back
            through the controller: `MainHud`, `Auth`, `Bootstrap`, `BottomNavigation`,
            `Configuration`, `NightSummary`, `PhoneHome`, `PingMessaging`.
            `ConfigurationUIController.Render()` was an empty "view binding pending"
            stub — the hook it was waiting for now exists.
      - [x] **Screen prefabs authored (2026-11-06).** 8 prefabs under
            `Assets/NAAD/UI/Views/`, generated by `UIScreenFactory` and committed.
            Getting there took a real fix: the file was behind the undefined
            `NAAD_LEGACY_EDITOR_TOOLS` define, wrote to private `[SerializeField]`
            fields across the assembly boundary, and called
            `AddComponent<TMP_Text>()` — `TMP_Text` is abstract, so it returned null and
            the factory died on the first label. It now compiles and runs.
      - [ ] Decide the relationship with `Gameplay/FirstNight/FirstNightHud.cs`, which is
            the UI actually running today. The Bootstrap scene still mounts it, not the
            new `Views/` prefabs — instantiating those is the next step.
- [ ] **CityStreet reproducibility.** The committed scene references
      `lamp_pole_small.prefab` and `bench.prefab` from `Assets/Versatile Studio Assets/`,
      which `.gitignore:58-59` excludes. Either commit those two prefabs (+ deps) or
      remove the references. Every other scene is self-contained.
- [ ] Backend build scoping: per-package `tsconfig.json` **done 2026-11-02** — it also
      revealed that `npm run build --workspaces` had been failing outright with TS5097
      (`.ts` import extensions without `allowImportingTsExtensions`). Now a typecheck
      (`noEmit`), which is correct because nothing consumes emitted JS: the edge
      function runs under Deno and the tests under `tsx`. Both workspaces and both
      test suites pass.
- [ ] **Do not enable `NAAD_LEGACY_EDITOR_TOOLS`.** It is defined nowhere, so 18
      editor files are silently dead. Defining it does not just revive dead code — it
      **breaks the build immediately**: `NaadCityStreetBuilder.cs` imports both
      `NAAD.World` and `NAAD.Gameplay.Interaction`, and its unqualified
      `AddComponent<SceneAnchor>()` at lines 818/829 becomes an ambiguous reference
      (CS0104) once both types are visible with the duplicate `SceneAnchor` still
      present. Fix W2 first, then either delete these files or repair them.
- [ ] Initialize the `tools/unity-cloud-asset-manager-for-blender` submodule properly
      (`git submodule status` shows `-`, despite content on disk).
- [ ] Warning debt: ~40 warnings — `FindObjectOfType` deprecations, `CS1998` in
      `FirstNightTests`, unused fields.
- [ ] Review the flagged high-severity dependency advisory (dependabot now configured
      to surface the next one automatically).

---

### W6 — Backend & world-model hardening · **M**

- [ ] Stand up a **disposable staging Supabase project** + test account.
- [ ] Move `first-night-live-test` onto staging with resettable state; only then can
      it become a required gate instead of a manual ritual
      (`naad-unity.yml:294` is `workflow_dispatch`-only and skipped on PRs).
- [ ] Verify the hosted-project surface is reproducible: `supabase/config.toml` has
      **no `[functions.npc-dialogue]` block**, so JWT verification relies on the CLI
      default, and hosted auth/RLS config is not versioned anywhere.
- [ ] Confirm `npc-dialogue` is deployed with `GEMINI_API_KEY` set, and that Gemini
      output is schema-validated before return (AI proposes; it must never write).
- [ ] Contract tests for the failure modes already proven by hand: invalid SKU,
      duplicate `request_id`, same-location travel — promote them from probes to CI.

---

### W7 — Documentation & ledger discipline · **S**

Status docs are written as snapshots but read as current truth; that is how Gate C
and Gate D ended up understated.

- [ ] Establish one source of truth. Recommendation: `GATE_STATUS.md` becomes the
      only live ledger; retire `PRODUCTION_READINESS_STATUS.md` into it and keep
      `PROJECT_PLAN.md` for roadmap only.
- [ ] Rule going forward: **whatever commit lands a gate updates the ledger.**
- [ ] `docs/Kevin_Iglesias_Import_Guide.md` claims "57 CHR_* NPC models"; the repo
      contains **9**. Correct it or mark the 57 as planned content.
- [ ] `docs/HUMANOID_ANIMATION_ASSET_AUDIT.md` (root) still reports "0 `.anim` files",
      which is stale after the placeholder clips landed.

---

## 3. Sequencing

```
W0 (land + verify)
 │
 ├── W5 hygiene ──────────────────────────┐  (parallel, anytime)
 ├── W6 backend hardening ────────────────┤  (parallel)
 │                                       │
 ├── W1 Gate C ── needs human asset buy ──┐
 │                                       │
 ├── W2 Gate D ──────────────────┐        │
 │                              ▼        │
 │                         W3 Gate E      │
 │                              │        │
 └──────────────────────────────┴──► W4 Gate F/16 ──► W7 ledger close-out
```

**Critical path:** W0 → W2 → W4. Gate C (W1) is the highest-*visibility* gap but it
is gated on asset acquisition, not on engineering — start the acquisition
immediately and let engineering run W2 in parallel rather than blocking on it.

**Recommended order:** W0, then start W1 acquisition + W2 concurrently, W5/W6 in the
margins, W3 after W2, W4 last.

---

## 4. Open decisions

| # | Decision | Blocks |
|---|---|---|
| 1 | Commit the 117 deletions as-is, or rescue any of `naad-fntest`'s unique scripts first? | W0 |
| 2 | Which UI system survives — `UI/*.cs` Controllers or `UI/Scripts/*.cs` Views? | W5 |
| 3 | CityStreet: commit the two vendor prefabs, or drop the references? | W5 |
| 4 | Does `Assets/StreamingAssets` (3.96 GB of LLM binaries) ship in the mobile build at all? | W4 |
| 5 | Is on-device LLM a product requirement? Nothing in `Assets/NAAD` references LLMUnity, but 3.96 GB of native libs says someone intended it. | W4 |

---

## 5. Explicitly out of scope

Per `PROJECT_PLAN.md` §6, and re-affirmed here: no microservices split, no Unity
networking rewrite, no Redis/Kafka/custom game server, no full-city expansion, no
art/CPU optimisation before Gate 16 measurements, no service-role or Gemini keys in
the client, no client-side authoritative state, and animation events never mutate
game state.

---

## 6. Related

- `docs/PROJECT_PLAN.md` — roadmap and architecture pillars.
- `docs/GATE_STATUS.md` — live ledger.
- `game/unity/.ai-game-dev/knowledge/architecture/` — mutation surface, Gate C asset
  gap, doc-drift gotcha.
- `docs/GATE_C_PLAYER_RIG_ANIMATION.md` — Gate C detail.
- `Assets/NAAD/UNITY_CLI_COMMANDS.md` — headless verification entrypoints.
