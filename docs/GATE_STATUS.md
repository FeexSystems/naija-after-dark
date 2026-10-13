# NAAD Gate Status

Last audit: 2026-10-13  
Branch: `feat/gate-a-unity-foundation-audit`  
Unity editor (actual): `6000.6.5f1` (`game/unity/ProjectSettings/ProjectVersion.txt`)  
Note: older bible drafts mentioning `6000.0.32f1` are outdated; Gate 22 docs already pin `6000.6.5f1`.

Status key: Planned · In progress · Implemented · Verified · Blocked

## Gate A — Project audit and Unity foundation

| Item | Status | Evidence |
|---|---|---|
| Repo / branch inspection | Verified | On feature branch; `main` at `a109508` (Gate 22 CI baseline merged) |
| Core service graph | Implemented | `NAADApplicationRoot`, stubs + Supabase live services, command/auth/player/world/phone/dialogue |
| Bootstrap scene in Build Settings | Verified | Only `Assets/Scenes/Bootstrap.unity` at build index 0 |
| Bootstrap First Night wiring | Implemented | `NAADBootstrap`, `FirstNightController`, `FirstNightHud`, `MainCamera` present |
| SupabaseConfig assigned on root | Verified | Wired to `Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset` |
| Bootstrap MainCamera component | Verified | Tagged `MainCamera` object was missing `Camera` component; added and saved |
| `NAADBuild.Verify` (Gate 22) | Verified | Local Editor: `VERIFY_PASS` via `NAAD.Editor.NAADBuild.Verify` |
| Production scene registry (Apartment/Street/Suya/Club/Beach) | Planned | Vendor demo scenes exist; no NAAD production scenes registered |
| Scene transition after Bootstrap | Planned | Serialized `nextSceneName` is empty (First Night currently lives in Bootstrap) |
| Unity CLI Pipeline drive | Verified | `unity status` → Editor `ready` on port 7801 (`6000.6.5f1`) |
| Compile-clean Editor | Verified | Console cleared after Input System 1.20.1 + package cleanup |

## Gate B — Environment / scenes

Planned. Vendor packs present (Apartment Kit, Demo City, PalmTreePack, cars) but not integrated as NAAD scenes.

## Gate C — Characters / animation

Planned. Kevin Iglesias human animation demos present; no NAAD player Animator/controller contract verified yet.

## Gate D — Gameplay / interaction

In progress (backend-backed First Night controller/HUD exist; no third-person controller or interaction framework in `Assets/NAAD`).

## Gate E — Atmosphere / simulation

Planned. `WorldClockPresenter` exists; no day/night lighting profiles or crowd systems verified.

## Gate F — Mobile UI / AI / release

In progress. Auth UI + First Night HUD exist; phone apps / Android device acceptance not verified in this audit.

## First blocking dependency (Gate A)

1. **Bootstrap must pass `NAAD.Editor.NAADBuild.Verify` with a non-null, configured `SupabaseConfig`.**  
   That assignment was missing (`supabaseConfig: {fileID: 0}`) and is the first foundation blocker for CI/local Gate 22 verification.
2. After Verify passes: missing NAAD production scenes and player locomotion remain the next blockers for Gate B/D — not more package installs.

## What this audit deliberately did not do

- Did not invent Apartment/Street/Nightclub scenes.
- Did not replace stub/live service architecture.
- Did not commit secrets or alter anon keys.
- Did not mark Android/device tests as passed.
