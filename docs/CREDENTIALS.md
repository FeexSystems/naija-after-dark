# Unity & CI Credential Locations

**This file records *where* credentials belong. It must never contain a credential
value.** If you are about to paste a key here, put it in the platform instead —
GitHub Actions secrets, a local environment variable, or the Unity Editor's own
sign-in — and note only its location below.

## Unity account (email + password)

Used for two independent things:

| Consumer | Where it lives |
|---|---|
| Unity Hub / Editor sign-in | Unity's own credential store. Not in the repo. |
| CI licensing (`game-ci/unity-builder`) | GitHub Actions secrets `UNITY_EMAIL` and `UNITY_PASSWORD` |

`.github/workflows/naad-unity.yml:42` accepts either `UNITY_LICENSE` alone, or
`UNITY_EMAIL` + `UNITY_PASSWORD`. The preflight fails with an explicit message if
neither is configured.

To set them (no `gh` CLI is installed on the dev machine, so do this in the
browser, or install `gh` first):

```bash
# with gh installed and authenticated for FeexSystems/naija-after-dark
gh secret set UNITY_EMAIL
gh secret set UNITY_PASSWORD
```

> **Only overwrite these if CI is currently failing licensing.** `verify-unity`
> has run green on `main`, which means licensing already works somehow. Replacing
> a working secret with a different account is how a green pipeline goes red.

## Unity Cloud project

The project is identified by a UUID in **two** places, which must match:

| File | Field | Tracked in git |
|---|---|---|
| `ProjectSettings/ProjectSettings.asset` | `cloudProjectId` | **yes — this is the source of truth** |
| `Assets/Unity Cloud/Resources/UnityCloudPlayerSettings.asset` | `AppId` | no — under gitignored `Assets/Unity Cloud/` (`.gitignore:60`) |

Current value: `a7e173c7-0e15-4f2d-b203-5ec0e695e479` (rebound 2026-11-05 from
`6064ee90-f849-4e52-a88a-c6ed43494390`).

Because the second file is gitignored, a clean clone does not carry the `AppId` —
Unity derives it from `cloudProjectId` on import. So the durable change is the
tracked one; editing the local asset only updates this machine's cache. Both were
set here so the running Editor agrees without a reimport.

Anything already linked to the previous AppId — deep linking, Asset Manager, cloud
saves — now resolves against the new project. Verify in the Editor's
**Services** panel after reopening the project.

`ProjectSettings/Packages/com.unity.services.core/Settings.json` separately pins an
**environment** (`EnvironmentId`), which is a child of a project. Rebinding the
project does not rebind the environment; if the new project has no matching
environment, that file needs regenerating from the Services panel.

## Unity Cloud API key

**Never committed.** It is a server/tooling credential, not a client one — the same
rule as the Supabase `service_role` key. Supply it through the environment of
whatever tool needs it (Unity CLI, Asset Manager sync, UGS tooling) and nothing
else.

The Unity client assemblies that talk to Cloud (`com.unity.cloud.*`) authenticate
through the Editor's own session, not through this key.

## Unity MCP bridge token

`game/unity/.mcp.json` holds a **per-session** bearer token for the local MCP
bridge. It is gitignored (`.gitignore`, "Unity MCP bridge config") because a
`git add -A` would otherwise commit a live credential — which is exactly what had
happened before 2026-11-05, when it sat staged in a 380-file index.

It expires. When the MCP tools start answering `Unauthorized — invalid_token`, the
token has rotated; re-pair from the Unity Editor (the AI Game Developer / MCP
window regenerates `.mcp.json`). Signing into Unity with an account does **not**
refresh it — they are unrelated credentials.

## Supabase

Client side: `game/unity/Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset`
holds `projectUrl` plus the **anon** key. Both are safe to commit; the anon key is
a public client credential and `SupabaseConfig.IsConfigured` rejects anything
containing `service_role`.

Server side: the `service_role` key and `GEMINI_API_KEY` live only in Supabase
secrets (`supabase secrets set ...`). See `docs/SUPABASE_SETUP_RUNBOOK.md`.

Dev player sign-in for the First Night loop uses `NAAD_DEV_EMAIL` /
`NAAD_DEV_PASSWORD` environment variables rather than committed scene values —
`NAADBootstrap` reads them as a fallback.
