# Supabase Setup & Play Runbook — First Night

**Current state (verified 2026-11-05):** the project in `SupabaseConfig.asset`,
`https://unzfqrfyejkyisalzkhc.supabase.co`, is **live and fully migrated**. All 28
client-callable RPCs respond:

```
$ python tools/verify-supabase.py
  dns        OK
  rpcs       28/28 present (28 answered UNAUTHENTICATED as expected)
```

So there is nothing to rebuild. **To play, you only need a user account** — go to
[Step 3](#step-3--supply-a-user-account).

Steps 1–2 apply only if you are standing up a **different** project.

> **Note on a correction.** Earlier on 2026-11-05 this host returned `NXDOMAIN` and
> was reported as a deleted project. It resolves and answers now; that reading was a
> transient DNS failure. Nothing was lost.

---

## Step 1 — (New project only) Create it

1. <https://supabase.com/dashboard> → **New project**.
2. Name `naija-after-dark`; region nearest your players (`eu-west-*` for Lagos).
3. Save the database password — you cannot recover it later.

## Step 2 — (New project only) Apply the schema

1. Dashboard → **SQL Editor** → **New query**.
2. Paste all of **`supabase/apply_all.sql`** (11 migrations, ≈3,400 lines,
   39 function definitions).
3. **Run.**

The file is generated — never edit it by hand:

```bash
python tools/supabase-bundle.py           # rewrite from supabase/migrations/*.sql
python tools/supabase-bundle.py --check   # fail when the bundle is stale
```

> On a **new** project this is the first execution of migrations 0010 and 0011.
> They redefine `naad_spend`, `naad_buy` and `naad_execute_command`, revoke
> `naad_react_to_event_attendance` from the public roles, and narrow `players`
> UPDATE to `display_name`. If the bundle errors, look there first.

## Step 3 — Supply a user account

The `Bootstrap` scene signs in with a dev account; there is no auth screen yet.

**The account must exist in the project and be confirmed.** Dashboard →
**Authentication** → **Users**:

- If a user already exists, use its email/password.
- Otherwise **Add user**, and tick **Auto Confirm User** — an unconfirmed address is
  the most common cause of "press Play and nothing happens".

Then give the client the credentials, one of two ways:

**Environment variables (preferred — nothing is committed):**

```bash
export NAAD_DEV_EMAIL="you@example.com"
export NAAD_DEV_PASSWORD="your-password"
```

**Or** set `Email` / `Password` on the `NAADBootstrap` component in
`Assets/Scenes/Bootstrap.unity`. This writes a password into a scene file — only do
it for a throwaway account.

If neither is set, the console now prints exactly what is missing
(`NAADBootstrap` logs it and names both environment variables) instead of failing
silently.

## Step 4 — Verify

```bash
python tools/verify-supabase.py                       # reads SupabaseConfig.asset
python tools/verify-supabase.py <url> <anon-key>      # or pass them explicitly
```

`28/28 present` means the backend side is healthy. Anything listed under
`MISSING RPCs` means the schema is incomplete for that project.

> **Why the probe sends arguments.** PostgREST returns `404` both for a function
> that does not exist and for one whose signature you failed to match. Probing with
> `{}` therefore reports every parameterised function as missing. The script builds
> the argument set from `rpc-catalog.ts`.

## Step 5 — Play

1. Open `game/unity` in Unity `6000.6.5f1`.
2. Open `Assets/Scenes/Bootstrap.unity` (build index 0).
3. Press **Play**.

You get an IMGUI panel (`FirstNightHud`, drawn in `OnGUI` — no prefabs or canvas
needed):

| Button | What it exercises |
|---|---|
| `1. Start Night` | `naad_start_night`; seeds Tunde's beach invite |
| `GO` / `ASK DETAILS` / `DECLINE` | `naad_respond_message` |
| `3. Suya Spot + Buy` | `TRAVEL` + `SPEND` (balance drops, receipt written) |
| `4. Club + Ticket` | `TRAVEL` + ticket purchase |
| `Talk to Tunde (AI)` | Edge Function `npc-dialogue` → Gemini (optional) |
| `5. Complete Night` | `naad_complete_night` → night summary |
| `Run Full Night (GO)` | the whole loop in one press |

The panel shows the current step, wallet balance, and any error.

## Optional — AI dialogue

`Talk to Tunde (AI)` needs the edge function deployed with a Gemini key:

```bash
supabase functions deploy npc-dialogue --project-ref <ref>
supabase secrets set GEMINI_API_KEY=<key> --project-ref <ref>
```

`verify_jwt = true` is already set in `supabase/config.toml`. Every other button
works without it.

## What is still not playable after this

This runbook gets the **First Night command loop** running against the
authoritative backend. It does not give you a third-person world:

- No scene instantiates `PlayerRig.prefab`, so there is no avatar to walk around.
- `Assets/NAAD/UI/` has no committed prefabs, so `FirstNightHud` (IMGUI) is the only
  interface.

Both need Unity Editor work — tracked under Gate D / W5 in
`docs/IMPLEMENTATION_PLAN_REMAINING.md`.
