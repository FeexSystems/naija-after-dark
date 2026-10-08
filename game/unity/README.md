# NAAD Unity Client

Untrusted presentation + input client.  
**Server state is authoritative. Never put service-role or Gemini keys here.**

## Open in Unity Hub

This folder **is** the Unity project (`ProjectSettings` + `Assets` + `Packages`).

### Option A — Add from disk (recommended)

1. Clone the repo:
   ```bash
   git clone https://github.com/FeexSystems/naija-after-dark.git
   ```
2. Unity Hub → **Open** → **Add project from disk**
3. Select folder: `naija-after-dark/game/unity`
4. Open with **Unity 6 LTS** (or the version Hub offers; first open may upgrade `ProjectVersion`)

### Option B — Add from repository

Unity Hub scans for a folder that contains `ProjectSettings/ProjectVersion.txt`.

- Repository: `FeexSystems/naija-after-dark`
- Branch: `main`
- If Hub still says “No Unity projects found”, use Option A (nested path `game/unity`).

Do **not** point Hub at the monorepo root (`backend/`, `supabase/`, etc.). The project root is **`game/unity`**.

## First open

1. Let Unity import scripts under `Assets/NAAD/`.
2. Create scene `Assets/Scenes/Bootstrap.unity` (File → New Scene → Save).
3. Empty GameObject → `NAADApplicationRoot` + assign `SupabaseConfig` asset.
4. Empty GameObject → `FirstNightController` + `FirstNightHud`.
5. Enter Play Mode after auth is configured (see `Docs/PHASE_B_FIRST_NIGHT.md`).

## Architecture rules (client)

1. No service-role credentials.
2. No Gemini API keys.
3. All mutations via `ICommandService` → `naad_execute_command`.
4. Device clock is never authoritative for game time.
