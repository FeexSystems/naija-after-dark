# NAAD Unity Client

Untrusted presentation + input client.  
**Server state is authoritative. Never put service-role or Gemini keys here.**

## Create the Unity project (Gate 3)

1. Open **Unity Hub**
2. New project → **3D (URP)** → Unity LTS
3. Project name: `NAAD` (or any)
4. Location: point at this folder **or** create elsewhere and copy `Assets/NAAD/` into the project’s `Assets/`
5. Open the project

Recommended: create the URP project **inside** `game/unity/` so the repo layout stays:

```
game/unity/
  Assets/
    NAAD/
  Packages/
  ProjectSettings/
```

If Unity generates `Library/`, `Temp/`, etc., they are already gitignored at repo root.

## Bootstrap flow (Gate 3.1)

```
BOOT
  → AUTH
  → PLAYER
  → WORLD
  → SCENE
```

Entry: place a single GameObject in a `Bootstrap` scene with `NAADBootstrap`.

## Architecture rules (client)

1. No service-role credentials.
2. No hard-coded secrets (use ScriptableObject config / env for anon key + URL only).
3. Prefer interfaces (`IAuthService`, `IWorldStateService`) over concrete globals.
4. `DontDestroyOnLoad` only on the application root.
5. No gameplay systems in Gate 3.

## Folders

```
Assets/NAAD/
  Core/          Bootstrap, state, logging
  Networking/    Service interfaces + future Supabase client adapters
  Gameplay/      (empty — later gates)
  World/
  Characters/
  UI/
  AI/
  Audio/
  Animation/
  Content/
```
