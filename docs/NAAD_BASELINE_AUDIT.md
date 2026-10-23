# NAAD Baseline Audit — Phase 0

**Document:** NAAD_BASELINE_AUDIT
**Runbook:** NAAD-MASTER-IMPLEMENTATION-RUNBOOK-v1.0, Part II (Phase 0)
**Repository:** `FeexSystems/naija-after-dark`
**Audit branch:** `docs/naad-baseline-audit` (forked from `origin/main`)
**Audit subject commit:** `272b7ec5ed54e4970213ca5bcdd898d1ef3df279` (`origin/main`, "Merge pull request #11")
**Audit method:** read-only inspection via `git show` / `git ls-tree` / `git grep` against `origin/main`.
**Audit date:** 2026-10-23

> This document records **observed** state only. Anything not directly
> evidenced by a command in this session is listed under *Unverified
> assumptions*. No production system was contacted. No repository content
> other than this file was modified.

---

## 1. Gate report

**Gate ID:** P0-BASELINE
**Phase:** 0 — Repository and safety baseline
**Branch:** `docs/naad-baseline-audit` (created from `origin/main`, not tracked upstream)
**Commit SHA:** `272b7ec5ed54e4970213ca5bcdd898d1ef3df279`
**Objective:** Establish the real repository state and separate known baseline
issues from new ones.

**Files inspected:**
- `.gitmodules`, `git status`, `git branch -a`, `git log`, `git stash list`
- `game/unity/ProjectSettings/ProjectVersion.txt`
- `naad-fntest/ProjectSettings/ProjectVersion.txt`
- `game/unity/Packages/manifest.json`, `game/unity/Packages/packages-lock.json`
- `game/unity/ProjectSettings/ProjectSettings.asset`
- `game/unity/ProjectSettings/PackageManagerSettings.asset`
- `.github/workflows/naad-unity.yml`
- `.gitignore`
- `game/unity/Assets/**/*.cs` (enumerated, 180 files), `.asmdef` inventory
- `game/unity/Assets/NAAD/Gameplay/FirstNight/FirstNightTests.cs`
- `game/unity/Assets/NAAD/Editor/FirstNightLiveTest.cs`
- `game/unity/Assets/NAAD/Networking/Config/SupabaseConfig.cs` + `Resources/SupabaseConfig.asset`
- `supabase/functions/npc-dialogue/index.ts`
- `game/unity/Assets/NAAD/Animation/PlayerLocomotionController.cs`
- `game/unity/Assets/NAAD/Characters/Player/PlayerLocomotionController.cs`

**Files changed:** `docs/NAAD_BASELINE_AUDIT.md` (this file, new)
**Commands executed:** git inspection only. **No** Unity, UBA, GameCI, Supabase,
or network-mutating command was run.
**Tests executed:** none.
**Evidence paths:** this document. No build artifacts exist.

**Result:** **BLOCKED**

**Verified facts:** sections 2–10 below.
**Unverified assumptions:** section 11.
**Security and production impact:** no mutation. One production anon key is
committed in a `Resources` asset (see §9) — public by design, not a breach.
**Outstanding risks:** §12.
**Next gate:** Phase 1 — editor/CI proof (§13).

---

## 2. Git state — contradicts the runbook's stated baseline

| Item | Runbook states | Observed |
|---|---|---|
| Phase 1 branch | `chore/phase1-pre-edit-decision-record` | Exists only as `origin/...`. **0 commits ahead of `main`.** |
| PR #11 | open, draft, unmerged | **Merged.** `main` HEAD is the merge commit for PR #11. |
| Baseline SHA | `2c56797…` | Ancestor of `main` (still valid history), but not the current baseline. `main` has advanced. |
| Local checkout at audit start | (assumed isolation branch) | `chore/unity-mcp-scaffolding-and-gitignore` @ `8c98b97`, **0 ahead / 85 behind** `main`. |

**Consequence:** there is no isolation branch in the sense the runbook
assumes. The branch named `chore/phase1-pre-edit-decision-record` is fully
contained in `main`; committing to it now would put work directly on top of
`main`'s history. A fresh branch from `main` is required for all future
phase work. This audit therefore used `docs/naad-baseline-audit`.

**Branch deletions observed:** during `git fetch --all --prune` on 2026-10-23,
seven remote branches were reported deleted on `origin`
(`feat/audit-gate-recovery-*`, `feat/gate-a-*`, `feat/naad-gate-c-*`,
`feat/naad-gate22-*`, `feat/naad-integration-gate-21`,
`feat/production-readiness-gate-recovery-*`, `vendor/com-vehiclephysics-pro-local`).
**Attribution unknown.** Local refs for most remain. No recovery attempted —
outside Phase 0 scope and would require approval.

**Working tree at audit start:** one untracked file, `RUNBOOK` (33,669 bytes),
supplied by the user. **Untouched.** Not ignored by `.gitignore`; not committed.

---

## 3. Unity project layout — two committed projects, one is on the wrong editor

The runbook's referenced path `game/ProjectSettings/...` does not exist. The
real project root is **`game/unity`**.

| Path | `m_EditorVersion` | Revision |
|---|---|---|
| `game/unity/ProjectSettings/ProjectVersion.txt` | **`6000.7.0b4`** ✅ | `9debfb8362db` |
| `naad-fntest/ProjectSettings/ProjectVersion.txt` | **`6000.6.5f1`** ⚠️ | `3ff58d469c8a` |

`naad-fntest` is a fully tracked second Unity project (109 tracked files) and
`.naad-authtest` is a third (1 tracked file). Neither is covered by CI. If any
validation flow opens them, it produces evidence at the wrong editor version.

**`main`'s `game/unity` editor matches the target.** That part of the baseline
is sound.

---

## 4. CI — one workflow, unproven

`.github/workflows/naad-unity.yml` (9,379 bytes) is the only workflow.

- Runs on `ubuntu-latest`, via `game-ci/unity-builder@v4`, with
  `unityVersion: 6000.7.0b4` hard-coded in five jobs.
- Jobs: `verify-unity` (build method `NAAD.Editor.NAADBuild.Verify`),
  `verify-project-structure`, `verify-animation` (Gate C),
  `list-packages` (`continue-on-error: true`), and
  `first-night-live-test` (gated behind `workflow_dispatch` +
  `inputs.run_live_first_night_test`, default `false`).
- The live-test guard is **correct** and its inline comment correctly
  documents the `manualExit: true` requirement and the production-mutation
  warning.

**Problem:** this is a `game-ci` Linux path, **not** the UBA Windows
configuration the runbook describes. There is no UBA configuration in the
repository — UBA is configured in the Unity Cloud Dashboard only. No stored
run artifacts, badges, or build logs exist in-repo. **No evidence was found
that this workflow has ever passed on `main`.**

The prior failure referenced in `docs/PHASE1_PRE_EDIT_DECISION_RECORD.md`
(run `39794296448`) was under editor **6000.6.5f1** and is therefore
superseded by the current `6000.7.0b4` pin — but superseded is not fixed.

---

## 5. Package graph — manifest, lockfile, registry are mutually inconsistent

This is the highest-value new finding. `manifest.json` declares **78** direct
dependencies; `packages-lock.json` has **117** entries. Seven declared direct
dependencies have **no lock entry at all**:

```
com.ivanmurzak.unity.mcp
com.ivanmurzak.unity.mcp.animation
com.ivanmurzak.unity.mcp.navigation
com.ivanmurzak.unity.mcp.particlesystem
com.ivanmurzak.unity.mcp.terrain
com.unity.testtools            = 2.0.0
com.rankupgames.unity-cursor-toolkit = 1.0.0
```

Note this is a *different* subset of MCP packages than the ones that do lock —
`mcp.cinemachine`, `mcp.inputsystem`, `mcp.probuilder`, `mcp.splines` are
present. The lockfile is from a partial or aborted resolution, not a clean one.

Additional version inconsistencies:

| Package | Manifest | Lock | Notes |
|---|---|---|---|
| `com.unity.nuget.newtonsoft-json` | `3.2.1` | `3.2.2` (depth 1) | Manifest asks 3.2.1, lock resolved 3.2.2 |
| `com.unity.inputsystem` | `6.7.0` | `6.7.0`, `"source": "builtin"` | See §6 — suspicious |
| `com.unity.xr.management` | `6.0.0` | `6.0.0`, `"source": "builtin"` | Registry also advertises `4.4.0` elsewhere in the graph |
| `com.unity.testtools` | `2.0.0` | **absent** | No such package ID in the Unity registry graph |

**`com.unity.testtools@2.0.0` is the prime suspect for hard resolution
failure.** It is a manifest dependency with no lock entry and no matching
package in the resolved graph. Git history shows
`ce088a9 fix(deps): add com.unity.testtools for Test Runner API` — added to
satisfy a compile error, at a version that may not exist.

### git-based MCP dependencies are unpinned

All ten `com.ivanmurzak.unity.mcp*` packages are declared as bare Git URLs with
no `#ref`:

```
https://github.com/IvanMurzak/Unity-MCP.git?path=<subpath>
```

They resolve to **whatever `HEAD` is on GitHub at import time**. Combined with
§5's missing lock entries, a "clean" `main` checkout is not reproducible. This
directly conflicts with Phase 2's "make package resolution deterministic"
objective and with the rule that MCP packages may not be removed without
evidence — you currently cannot even prove *which commit* was used.

### Scoped registry is misconfigured

`PackageManagerSettings.asset` and `manifest.json` both add a scoped registry
`package.openupm.com` with scopes `extensions.unity`, `com.ivanmurzak`,
`com.rankupgames`. `com.ivanmurzak.*` and `com.rankupgames.*` are **Git URL
and Unity-registry packages, not OpenUPM packages**. Routing those scopes to
OpenUPM is at minimum useless and at worst resolves the wrong package.

`m_UserSelectedRegistryName: package.openupm.com` is persisted in project
settings — consistent with manual GUI package installation.

### MCP retention evidence

- **Code consumers:** `git grep` for `Mcp|MCP` across `Assets/**` found **zero**
  C# references. The only hits were two PNG filenames containing "ML".
- **Declared surface:** 10 packages (base + animation, cinemachine,
  inputsystem, navigation, particlesystem, probuilder, splines, terrain).
- **Manual consumers:** `mcp-manifest.json` at `game/unity/` and
  `tools/mcp-config.json`.

Conclusion: MCP packages are **editor-tooling only** with no compile-time
consumer. That is exactly the evidence needed to argue they should not be
required to *build*, but per the retention rule they are **kept** pending the
removal-evidence gate in Phase 2.

---

## 6. 🔴 New critical finding — the NAAD test suite cannot run

`Assets/NAAD` contains **zero `.asmdef` files**. The only asmdefs in the
project are inside the vendored `Assets/LLMUnity/` tree.

Consequences:

1. All ~130 NAAD `.cs` files, **including Editor scripts**, compile into the
   default `Assembly-CSharp` / `Assembly-CSharp-Editor` pair.
2. `Assets/NAAD/Gameplay/FirstNight/FirstNightTests.cs` uses
   `using NUnit.Framework;` and `[Test]` / `[SetUp]` / `[TearDown]` but lives
   in a **runtime** assembly. `nunit.framework.dll` is not referenced by
   `Assembly-CSharp` by default. This file either fails to compile or, if it
   resolves transitively, is never discovered as a test.
3. `Assembly-CSharp` has no reference to
   `UnityEngine.TestRunner` / `UnityEditor.TestRunner`, so nothing in it can
   be registered as an EditMode or PlayMode test.
4. There is no `Tests` assembly and no test asmdef for the Unity Test Framework
   to enumerate.

`git grep` found 9 files matching `(Test|Tests).*\.cs` under
`game/unity/Assets`, but only the two `LLMUnity/Tests/Editor/*.cs` files have a
proper test asmdef (`undream.llmunity.Editor.Tests.asmdef`).

**Combined with UBA having EditMode/PlayMode disabled and
"Fail build if tests fail" enabled, the configured test gate can only ever
evaluate the bundled LLMUnity vendor tests — never any NAAD gameplay logic.**

This makes Phase 2's "Unity scripts compile" and Phase 3's "deterministic
tests for movement input and state transitions" structurally impossible as
currently configured. It is the single largest blocker for every later phase.

---

## 7. Input and locomotion — two competing implementations confirmed

| | `NAAD.Animation.PlayerLocomotionController` | `NAAD.Characters.Player.PlayerLocomotionController` |
|---|---|---|
| Path | `Assets/NAAD/Animation/` | `Assets/NAAD/Characters/Player/` |
| Namespace | `NAAD.Animation` | `NAAD.Characters.Player` |
| CharacterController | Yes (`[RequireComponent]`) | (per runbook: `NavMeshAgent` — see §11) |
| Input path | injected `SetMoveInput`, `MoveInput`, `IsMoving` | `PlayerInputBridge` |
| Animation | `[SerializeField] AnimationStateManager` | `PlayerAnimator` |
| Camera | `_cameraTransform` field | `ThirdPersonCamera` in same folder |
| Config asset | none | `PlayerLocomotionConfig` |

Both are camera-relative, both own gravity/rotation, both are
`[RequireComponent(typeof(CharacterController))]` on the `NAAD.Animation` side.
`NAAD.Characters.Player` also carries `PlayerInputBridge`, `PlayerAnimator`,
and `ThirdPersonCamera` — a more complete "player stack".

Input handling is globally configured `activeInputHandler: 2` (Both),
`com.unity.inputsystem` `6.7.0`, with `InputSystem_Actions.inputactions`,
`VirtualJoystick.cs`, and `InteractButton.cs`. Touch support exists.

**Resolution is a Phase 3 decision, not a Phase 0 one.** Determining which one
the actual player prefab references is required before either can be called
canonical.

---

## 8. Scenes, prefabs, animation assets

- **15 scenes total**, of which 9 are `LLMUnity` vendor samples.
- First-party scenes: `Assets/Scenes/Bootstrap.unity`,
  `Assets/NAAD/Scenes/{Apartment,Beach,CityStreet,Nightclub,SuyaSpot}.unity`.
- **7 prefabs**, including `Assets/New Mesh.prefab` (default-named, likely
  leftover).
- Gate C required-asset paths listed in the runbook (`PlayerRig.fbx`,
  `PlayerController.controller`, `PlayerAvatar.asset`, 13-item list) are **not
  present** under `Assets/NAAD/Animation/`; previously reported as 0 of 37.
  **Not re-verified file-by-file in this audit** — deferred to Phase 4, where
  the list must be rechecked against the branch before creating anything.
- `VerifyAnimationAssets.cs` exists as the CI entry point for Gate C.

---

## 9. Secrets and configuration

**Scan result: no service-role key, no Gemini key, no API token leaked.**

`git grep` for `eyJhbGciOi`, `service_role`, `SUPABASE_SERVICE`, `sk-…`,
`AIza…` across all tracked files returned only:

- `supabase/functions/npc-dialogue/index.ts` — correctly reads
  `GEMINI_API_KEY`, `SUPABASE_ANON_KEY`, `SUPABASE_URL` from `Deno.env.get`
  only. Correct pattern.
- Documentation mentions of `service_role` as forbidden — correct.
- `SupabaseConfig.cs` — guards against service-role keys in code. Correct.

**One real finding:** `Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset`
is committed with a populated production `anonKey` (JWT, `role: anon`,
project ref `unzfqrfyejkyisalzkhc`). An anon key is designed to be public, so
this is **not a credential breach**. It is however a single hard-coded
**production** project ref inside a `Resources/` folder, meaning:

- it ships inside every player build,
- there is no local/staging/production switch (Phase 7 blocker),
- `.gitignore` has no rule to stop it later holding something sensitive.

**Live-test production coupling:** `FirstNightLiveTest` resolves its target
from `cfg.projectUrl`, i.e. the committed asset → production. The workflow
guard (§4) is correct, but the capability to mutate production sits behind one
checkbox. No staging project exists in the repo (`config.toml` + 10 migrations,
single ref).

---

## 10. Dependency map (summary)

```
Repository root
├── game/unity            ← THE Unity project (6000.7.0b4)  [CI + UBA target]
│   ├── Packages/manifest.json (78 direct)  ⇄ lock (117)  ⚠ 7 missing
│   ├── Assets/NAAD       130 first-party .cs, 0 asmdef 🔴
│   ├── Assets/LLMUnity   ~50 vendored .cs, has asmdefs (TestLLM, TestSearch)
│   └── ProjectSettings   DefaultCompany / productName "unity" ⚠
├── naad-fntest           second Unity project (6000.6.5f1) ⚠ untested
├── .naad-authtest        stub Unity project
├── supabase/             10 migrations, 1 Edge Function (npc-dialogue), seed
├── ai/                   prompts, schemas, evaluations
├── backend/              ai-orchestrator, game-api
├── world-model/          (empty/stub on main)
├── tools/                blender-bpy, mcp-config.json, git_filter.py,
│                         unity-cloud-asset-manager-for-blender [submodule]
├── Gameplay/             root-level .cs — dialogue scripts OUTSIDE the Unity project 🔴
└── .github/workflows     naad-unity.yml only
```

**🔴 New finding — orphaned source.** Eight C# files live at the *repository
root* in `Gameplay/` — `DialogueBubble.cs`, `DialogueBubbleManager.cs`,
`DialogueCondition.cs`, `DialogueManager.cs`, `DialogueResponse.cs`,
`RelationshipCondition.cs`, `ReputationCondition.cs`, `SeenCondition.cs`.
They are outside any Unity project and therefore never compile into any build.
They appear to be the committed output of
`git show e6fcbe1 "Add Phase 3 AI/LLM Orchestration scripts"` /
`4b1596e "Add dialogue condition scripts and animation avatars"`. Their
duplicates exist inside `game/unity/Assets/NAAD/UI/`. **Do not delete** —
this is unproven-duplicate territory and needs Phase 3/6 inspection first.

---

## 11. Unverified assumptions

These are **not** established and must not be relied on:

1. That `naad-unity.yml` has ever succeeded on `main`.
2. That GameCI publishes a Linux image for the `6000.7.0b4` beta.
3. That UBA can select an exact `6000.7.0b4` rather than the rolling channel.
4. That the UBA machine size is eligible under the current account plan
   (the free-trial notice limits builds to Windows/macOS STANDARD).
5. That `com.unity.testtools@2.0.0` exists; that `com.unity.inputsystem@6.7.0`
   is a real selectable version rather than a resolved builtin.
6. That the root-level `Gameplay/*.cs` files are exact duplicates of the
   in-project copies.
7. That the Gate C asset list in the runbook matches the current branch.
8. Which `PlayerLocomotionController` the player prefab actually references.
9. That the MCP packages install successfully under `6000.7.0b4` at all.
10. That the submodule content is *usable* — only that the SHA matches.

---

## 12. Blockers

| ID | Severity | Blocker | Evidence |
|---|---|---|---|
| B1 | 🔴 Critical | No NAAD test assembly; test suite cannot be discovered or run | §6 |
| B2 | 🔴 Critical | Manifest/lock agree on only 71 of 78 direct deps; 7 missing | §5 |
| B3 | 🔴 Critical | `com.unity.testtools@2.0.0` unresolvable — likely hard failure | §5 |
| B4 | 🟠 High | MCP Git deps unpinned → builds not reproducible | §5 |
| B5 | 🟠 High | Exact `6000.7.0b4` execution unproven on both CI and UBA | §3, §4 |
| B6 | 🟠 High | Two competing locomotion implementations; canonical owner undecided | §7 |
| B7 | 🟠 High | No staging environment; live test wired to production ref | §9 |
| B8 | 🟡 Medium | Second Unity project committed on `6000.6.5f1`, outside CI | §3 |
| B9 | 🟡 Medium | OpenUPM scoped registry includes scopes it doesn't serve | §5 |
| B10 | 🟡 Medium | Android identity is `DefaultCompany` / `unity`, `applicationIdentifier: {}` | §5 |
| B11 | 🟡 Medium | 8 orphaned `.cs` files at repo root, never compiled | §10 |
| B12 | 🟡 Medium | Isolation branch contract no longer exists; PR #11 merged | §2 |
| B13 | 🟢 Low | 7 remote branches deleted; attribution unknown | §2 |

**Known baseline issues (pre-existing, from `PHASE1_PRE_EDIT_DECISION_RECORD.md`)
vs new issues (found in this audit):**

- *Pre-existing:* 6000.6.5f1 CI failure, unresolved package set, lock/manifest
  inconsistency, submodule metadata, deprecated Blender add-on, Gate C 0/37,
  two locomotion implementations, live-test danger.
- *New in this audit:* **B1** (no test asmdefs), **B3** (specific
  `testtools@2.0.0` diagnosis), **B4** (Git deps have no ref pin),
  **B9** (registry scopes), **B10** (Android identity),
  **B11** (orphaned root `Gameplay/` source), missing lock entries being
  *MCP-specific rather than general*, and `naad-fntest` on `6000.6.5f1`.

---

## 13. Tests and commands available today

**These are the commands that exist. None were executed in Phase 0.**

| Purpose | Command | Entry point | Verified runnable? |
|---|---|---|---|
| Compile + Gate 22 scene contract | `-executeMethod NAAD.Editor.NAADBuild.Verify` | CI `verify-unity` | **No** |
| Project structure | `-executeMethod NAAD.Editor.Editor.VerifyProject` | CI | **No** |
| Gate C animation contract | `-executeMethod NAAD.Editor.VerifyAnimationAssets.RunVerification` | CI | **No** |
| Package inventory | `-executeMethod NAAD.Editor.Editor.ListPackages` | CI | **No** |
| Live First Night (PROD, mutating) | `-executeMethod NAAD.Editor.FirstNightLiveTest.Run` | CI, opt-in | **Must not run** |
| NUnit gameplay tests | Unity Test Framework | **not wired** | **No** (B1) |
| Batch entry point | `NAAD.Editor.NAADEditorBatch` | present | Unknown |
| Auth live test | `NAAD.Editor.AuthLiveTest` | present | Unknown |

---

## 14. Untouched user changes

- `RUNBOOK` (untracked, repo root, 33,669 bytes) — supplied by the user,
  **not modified, not staged, not committed**.
- `.venv/`, `PRP_Bed_ND,PRP_TV_ND/`, `naad-fntest/`, `.naad-authtest/` —
  ignored/untracked working directories, untouched.
- **No user change was discarded. No destructive command was run.**
- The pre-existing local branch `chore/unity-mcp-scaffolding-and-gitignore`
  @ `8c98b97` remains intact and checked-out-able.

---

## 15. Phase 0 conclusion

**Phase 0 acceptance criteria:**

| Criterion | Status |
|---|---|
| Current branch and commit recorded | ✅ §1 |
| No user changes discarded | ✅ §14 |
| Known baseline issues separated from new issues | ✅ §12 |
| No production mutation or deployment | ✅ none performed |

**Phase 0 deliverable `docs/NAAD_BASELINE_AUDIT.md`: produced.**

The runbook's STOP condition — *"STOP if the local state cannot be safely
determined"* — **is not triggered.** Local state is fully determined and safe.

However Phase 0 surfaces **B12**: the branch isolation contract the runbook
depends on no longer exists, because PR #11 merged. Every subsequent phase
needs a branch decision before it can start.

---

## 16. Smallest safe next action

Phase 1's objective is *"establish a reproducible route to resolve packages,
import assets, compile, and build using the intended editor."* B1–B5 all
block that, and **B3 is the cheapest to disprove**.

**Proposed Phase 1 step 1 (evidence-only, reversible, no manifest edit):**

Determine whether the seven unlocatable direct dependencies actually exist,
before changing anything:

1. Query the Unity registry for `com.unity.testtools` and confirm whether
   `2.0.0` exists, and whether the correct ID is `com.unity.test-framework`
   (which **is** already resolved in the lock at `1.9.0`).
2. Query registry metadata for `com.unity.inputsystem` to confirm `6.7.0` is
   selectable for `6000.7.0b4` — the lock says `"source": "builtin"`, which
   may mean it was never resolved from a registry.
3. Record the resolved `HEAD` SHAs of the five missing MCP Git packages
   (`git ls-remote`) so builds become attributable even if unpinned.
4. Report findings. **Change nothing.**

Only after that evidence exists does a manifest diff become justifiable —
and it would be the smallest possible one (`testtools` → `test-framework`
compatibility, or removal of `testtools`).

**This is a BLOCKED gate awaiting the user's decision on §2 (branch model).**

---

## 17. Phase 1 step 1 — executed (evidence-only probe)

Executed against the public Unity Package Registry. **No files were changed.**

### Command

```powershell
Invoke-RestMethod https://packages.unity.com/<package-id>
```

### Results

| Package | Query | Result |
|---|---|---|
| `com.unity.testtools` | full metadata | **HTTP 404 — package does not exist** |
| `com.unity.test-framework` | version list | `1.9.0` **not present**. 64 versions published; highest stable **`1.4.6`** (`dist-tags.latest`) |
| `com.unity.inputsystem` | version list | `6.7.0` **not present**. 64 versions published; highest **`1.20.1`** (`dist-tags.latest`) |

### Findings — two hard resolution failures proven

**F1 — `com.unity.testtools@2.0.0` cannot resolve.**
The package ID does not exist in the Unity registry (404). This is not a
version problem; the ID itself is wrong. The manifest declares it, the
lockfile has no entry for it (§5), and no such package is publishable.

**F2 — `com.unity.inputsystem@6.7.0` cannot resolve from a registry.**
`6.7.0` is not a published version. The highest published is `1.20.1`.
The lockfile records it as `"source": "builtin"`, meaning UPM treated it as a
built-in module supplied by the `6000.7.0b4` editor rather than a registry
package — so it may resolve *only* on that exact editor, and cannot resolve on
any editor where Input System is not a builtin. This fully explains the
`6000.6.5f1` CI failure quoted in `PHASE1_PRE_EDIT_DECISION_RECORD.md`, and it
means the input path is editor-version-coupled in a way nobody has documented.

**F3 — lockfile references a version that has never existed.**
`packages-lock.json` resolves `com.unity.test-framework` at **`1.9.0`**, which
is not a published version (max `1.4.6`). The lockfile is therefore either
hand-edited, produced by a different registry, or corrupted. It cannot
regenerate to that state. **The lockfile is untrustworthy as a whole** and
every version claim inside it requires independent verification.

### Consequence for Phase 1

The Phase 1 objective — *"clean checkout succeeds, package resolution
succeeds"* — **cannot pass with the current manifest**, regardless of which
editor or CI system is used. This is not an environment problem; it is a
source defect in the repository.

### Smallest reversible diff that would advance the gate (PROPOSED — NOT APPLIED)

```diff
-    "com.unity.testtools": "2.0.0",
     "com.unity.test-framework": "1.4.6",
+    "com.unity.inputsystem": "1.20.1",
```

Rationale: remove the non-existent package ID; adopt the current stable Test
Framework (which is also what supplies `UnityEngine.TestRunner`, required by
B1); pin Input System to a real published version so it stops depending on
editor builtin status.

**This diff is NOT approved and NOT applied.** It would cascade into a
lockfile regeneration that must be produced by the `6000.7.0b4` editor itself,
and it interacts with B1 (test asmdefs) — so it belongs in a single reviewed
package-integrity change, not a drive-by edit.

### Remaining Phase 1 uncertainty (still unverified)

- Whether `6000.7.0b4` ships `com.unity.inputsystem` as a builtin, and at which
  version — **this must be read from the editor, not guessed.**
- Whether the five missing MCP Git packages resolve at all under `6000.7.0b4`.
- Whether UBA can select an exact `6000.7.0b4` rather than the rolling channel.
- Actual `HEAD` SHAs of the MCP Git repositories (not yet captured).

**Gate P1-STEP1 result: PASS (evidence captured). Gate P1-PACKAGES result: FAIL
(source defect proven). Gate P1-EDITOR-VERSION result: BLOCKED (requires UBA or
editor access).**

---

## 18. UBA build evidence — exact editor PROVEN, package failure CONFIRMED

**Source:** Unity Build Automation log supplied by the user, run 2026-10-23
05:18:50Z–05:32:12Z, target `naad-phase1-6000-7-0b4-validation`.

### 18.1 Editor version gate — PASS (first hard evidence)

Three independent lines in the log establish the exact editor:

```
[2026-10-23T05:18:57Z] - INFO: Mounting Unity Version: 6000_7_0b4
Built from '6000.7/staging' branch; Version is '6000.7.0b4 (9debfb8362db) revision 10349563'
Initialize engine version: 6000.7.0b4 (9debfb8362db)
```

`9debfb8362db` matches `game/unity/ProjectSettings/ProjectVersion.txt` exactly.

**This resolves B5 for the UBA path.** The rolling channel
"Always Use Latest 6000.7 beta" resolved to `6000.7.0b4` on this run, and the
build log proves it. The **exact-version gate is now satisfied for UBA** —
but it is satisfied *for this build only*: the channel can move. It must be
re-checked on every validation run, not assumed.

> Caveat recorded in the log itself:
> `WARN: Building with alpha/beta Unity version 6000.7.0b4. We do not provide support for alpha/beta versions.`
> UBA does not support beta editors. The build ran; support is not implied.

### 18.2 Source revision — matches `main`, no drift

```
GIT: Current revision is 512fba9de25592be1bd329c6524028bce0e0bed3
```

Verified locally: `512fba9` ("docs(phase1): assess Unity Build Automation beta
support") **is an ancestor of `main`**, and
`git diff --stat 512fba9 origin/main -- game/unity/Packages/` is **empty**.
The package defect proven below therefore applies to `main` unchanged.

### 18.3 Package resolution gate — FAIL (confirmed, and it is not my proposal)

The build reached package resolution and stopped:

```
[Package Manager] Done resolving packages in 219.99 seconds
An error occurred while resolving packages:
  Project has invalid dependencies:
    com.rankupgames.unity-cursor-toolkit: Package [com.rankupgames.unity-cursor-toolkit@1.0.0] cannot be found
    com.unity.testtools: Package [com.unity.testtools@2.0.0] cannot be found
nchBugReporter] Exiting without the bug reporter. Application will exit with return code 1
FATAL: ! Unity player export failed!
Build step 'Execute scripts' changed build result to FAILURE
```

**Exactly two packages fail — not seven.** This corrects §5.

### 18.4 Corrections to §5 and §17

| Earlier claim | Status | Correction |
|---|---|---|
| 7 direct deps missing from lock, all failing | ❌ Wrong | **2 fail.** The missing-from-lock list was a manifest/lock diff, not a failure list. The 5 MCP packages I flagged **resolved successfully** — UPM fetched them from Git at build time. |
| `com.unity.inputsystem@6.7.0` cannot resolve (F2) | ❌ Wrong | **It resolved fine.** The log shows `Input System module state changed to: Initialized.` and lists `InputSystem` among registered platform modules. `6000.7.0b4` supplies it. My registry query was correct (6.7.0 is not *published*) but my inference was wrong: it resolves as an **editor builtin**, exactly as the lockfile's `"source": "builtin"` said. **The lockfile was telling the truth and I misread it as corruption.** |
| `com.unity.testtools@2.0.0` cannot resolve (F1) | ✅ **Confirmed** | Log: `Package [com.unity.testtools@2.0.0] cannot be found`. Registry: 404. |
| Lockfile untrustworthy as a whole (F3) | ⚠️ Softened | `test-framework@1.9.0` is still a phantom version, but the lock resolved enough to get to package-validation stage. Treat as **suspect**, not **corrupt**. |
| `com.rankupgames.unity-cursor-toolkit` — I called the OpenUPM scope "useless" | ❌ Wrong premise, right conclusion | I verified: it is **404 on OpenUPM AND 404 on Unity registry**. The OpenUPM scope exists precisely because the private registry was *supposed* to serve it. **The package is simply unavailable on every public registry.** |

**The net correction:** Phase 1 does **not** need a manifest redesign. It needs
two packages dealt with. My §17 proposed diff was **too large and partly
wrong** — pinning `inputsystem` to `1.20.1` would have introduced an
unnecessary regression by overriding a working builtin.

### 18.5 Status of each failing package

**`com.unity.testtools@2.0.0`** — the ID does not exist anywhere (404 on
`packages.unity.com`). It was added in `ce088a9 fix(deps): add com.unity.testtools
for Test Runner API`. The correct package for the Test Runner API is
`com.unity.test-framework`, which supplies `UnityEngine.TestRunner` and
`UnityEditor.TestRunner`. **Removal is the correct action, not replacement.**

**`com.rankupgames.unity-cursor-toolkit@1.0.0`** — 404 on both public registries.
This is third-party editor tooling. Evidence gathered:

- **Zero C# consumers.** `git grep -n "rankupgames"` across all tracked files
  returns only `manifest.json`, `PackageManagerSettings.asset`, and the decision
  record. No `using`, no type reference, no asmdef reference.
- It is **not** a Unity MCP package, so the MCP retention rule does not apply to it.
- It carries no build-time or player-time dependency.

Given §18.4 shows every other package resolves, this single third-party
editor package is the sole remaining blocker on that side of the failure.

### 18.6 Untouched by this failure

Because Unity exited at package resolution, **compilation, tests, asset import,
and the player build were never attempted.** The build proves only:
checkout ✅, submodule handling (n/a — UBA checked out, no `-recursive`),
editor mount ✅, license ✅, package resolution ❌.

Note: `No Library cache found - ALL assets will be re-imported.` — first build,
no cache, 219.99s in package resolution alone.

---

## 19. Phase 1 gate report (revised)

**Gate ID:** P1-EDITOR-CI
**Phase:** 1 — Unity editor and CI compatibility
**Branch:** `chore/phase1-pre-edit-decision-record` (as built by UBA)
**Commit SHA:** `512fba9de25592be1bd329c6524028bce0e0bed3`
**Editor proven:** `6000.7.0b4 (9debfb8362db)` ← from build log
**Objective:** Establish a reproducible route to resolve packages, import
assets, compile, and build the project using the intended editor.

| Acceptance criterion | Result |
|---|---|
| Editor version established | ✅ **PASS** — `6000.7.0b4 (9debfb8362db)`, three log lines |
| Clean checkout succeeds | ✅ **PASS** — clone + checkout in 24s, rev `512fba9` |
| Submodules resolve | ⚠️ **N/A / UNPROVEN** — UBA did not use `--recursive`; no submodule step in log |
| Package resolution succeeds | ❌ **FAIL** — 2 invalid dependencies |
| Unity compilation succeeds | ⚠️ **NOT REACHED** — blocked by package failure |
| Build result and logs retrievable | ✅ **PASS** — this log |
| No deployment or production test triggered | ✅ **PASS** — Publishing section ran but reported "No files were added to be check in therefore the check-in operation will not be performed" |

**Result: FAIL**

**Failing step:** `package resolution` (not editor, not checkout, not compile).
**Verified facts:** §18.
**Unverified assumptions:** submodule resolution; compilation; test execution.
**Security and production impact:** none. License is `Unity Personal`,
`Type: Assigned`, `Expiration: 2026-10-24T05:21:40.887Z` — **expires within
24 hours of that run**. No credentials appeared unmasked in the log.
**Outstanding risks:**

1. **Personal licence expiring 2026-10-24** — the next validation build may
   fail at licensing, not at packages. Must be re-confirmed before any run.
2. `No Library cache found` → every validation build pays a full reimport.
3. `ProcessInitializeOnLoadMethodAttributes (50374ms)` — 50 seconds in
   `InitializeOnLoad`. With 10 MCP packages this is the likely cost. It works,
   but it dominates cold-start time.
4. UBA does not support beta editors (log warning) — success is not guaranteed
   on future channel moves.

**Next gate:** Phase 1 retry after the two-package fix.

---

## 20. Smallest reversible diff that advances Phase 1 (PROPOSED — NOT APPLIED)

Replaces the §17 proposal, which was based on incorrect inferences.

```diff
--- a/game/unity/Packages/manifest.json
+++ b/game/unity/Packages/manifest.json
@@
-    "com.rankupgames.unity-cursor-toolkit": "1.0.0",
@@
-    "com.unity.testtools": "2.0.0",
```

**Two deletions. No additions. No version changes.**

- `com.unity.testtools` → **delete.** ID does not exist (404). Test Runner API
  is provided by `com.unity.test-framework`, already present transitively.
- `com.rankupgames.unity-cursor-toolkit` → **delete.** 404 on both public
  registries, zero code consumers, editor-only third-party tooling.
- `com.unity.inputsystem` `6.7.0` → **leave alone.** It resolved; §17's
  proposed pin was wrong.
- The 5 MCP packages → **leave alone.** They resolved successfully.
- The OpenUPM scoped registry → **leave alone for now.** With both offending
  scopes removed by deletion, it becomes inert rather than harmful; cleaning it
  is a separate, cosmetic change.

**Expected effect:** package resolution should proceed past the two invalid
dependencies, reaching compilation for the first time.

**What this does NOT fix:**

- **B1 (no test assembly) remains untouched.** Deleting `testtools` does not
  make `FirstNightTests.cs` runnable. With EditMode/PlayMode disabled in UBA,
  no NAAD test will execute. This is Phase 2 work.
- `packages-lock.json` will be regenerated by the editor at import. The
  phantom `test-framework@1.9.0` entry should be verified as replaced.
- Compilation was never reached, so **a new class of failure may appear after
  this fix.** That is expected and is progress, not regression.

**Not applied. Awaiting approval.**

---

## 21. Process note — a correction I owe

In §17 I stated three findings as proven. The UBA log shows **one was right
(`testtools`), one was wrong (`inputsystem`), and one was overstated (lockfile
corruption)**. The registry queries were accurate; the inferences drawn from
them were not, because I reasoned about *publication* when the system resolves
*builtins*.

The lesson is recorded in §18.4 rather than edited away: the lockfile's
`"source": "builtin"` field **was** the evidence, and I treated it as a symptom.
Future package claims in this document draw only on build logs or registry
404s, not on inference from absence.
