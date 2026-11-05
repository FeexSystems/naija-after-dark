# Supabase Restore Runbook — getting First Night playable

**Why this exists.** The project's only backend,
`https://unzfqrfyejkyisalzkhc.supabase.co`, **no longer exists**. Its hostname
returns NXDOMAIN from Google DNS (8.8.8.8) and Cloudflare, while `supabase.com`
resolves normally — so this is a deleted project, not a network problem.

Consequences:

- Every First Night command fails, because the client composes *live* services
  (`SupabaseConfig.IsConfigured` is true) and every RPC call hits a dead host.
- The historical `GATE_B_PASS` / `PHASE_D_PASS` evidence in `docs/GATE_STATUS.md`
  is no longer reproducible. It is a record of a past run, not a current guarantee.
- The CI job `first-night-live-test` cannot pass until this is fixed.

Everything below is prepared; only steps 1, 2 and 4 need a human with Supabase
account access.

---

## Step 1 — Create the project (human)

1. <https://supabase.com/dashboard> → **New project**.
2. Name: `naija-after-dark`. Region: whichever is closest to your players
   (for Lagos, `eu-west-*` or `eu-central-*` give the lowest round-trip).
3. Save the database password somewhere safe — you do not need it for the game,
   but you cannot recover it later.

## Step 2 — Apply the schema (human, ~1 minute)

1. Dashboard → **SQL Editor** → **New query**.
2. Paste the entire contents of **`supabase/apply_all.sql`** (3,429 lines,
   11 migrations, 39 function definitions).
3. **Run**.

That file is generated — never edit it. If a migration changes, regenerate:

```bash
python tools/supabase-bundle.py           # rewrite apply_all.sql
python tools/supabase-bundle.py --check   # CI-friendly staleness check
```

> **First real test of migrations 0010 and 0011.** Those two were written on
> 2026-11-05 and have never been executed against a database (no Docker daemon,
> no live project). They redefine `naad_spend`, `naad_buy` and
> `naad_execute_command`, and revoke `naad_react_to_event_attendance` from
> `public`/`anon`/`authenticated`. If anything in the bundle errors, it will be
> there — read the error, and check the `do $$ ... revoke` block at the end of
> 0010 and the `players` column grant in 0011.

## Step 3 — Get the credentials (human)

Dashboard → **Project Settings** → **API**:

- **Project URL** → `https://<new-ref>.supabase.co`
- **anon / public** key → the long `eyJ...` JWT

Do **not** use the `service_role` key. `SupabaseConfig.IsConfigured` rejects it on
purpose, and it must never reach the client.

## Step 4 — Create a player account (human)

The Bootstrap scene signs in with a dev account; there is no auth screen yet.

Dashboard → **Authentication** → **Users** → **Add user**:

- Email + password of your choosing.
- Tick **Auto Confirm User** — otherwise sign-in fails until the address is
  verified, which is the most common cause of "it just does nothing".

Alternatively, Authentication → **Providers** → **Email** → turn off
"Confirm email" for the project.

## Step 5 — Point the client at the project (edit one asset)

`game/unity/Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset`:

```yaml
projectUrl: https://<new-ref>.supabase.co
anonKey: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9....
```

Or set both fields in the Inspector on the `SupabaseConfig` asset.

## Step 6 — Supply the dev sign-in (no secrets in the repo)

The Bootstrap scene's `email`/`password` fields are intentionally blank. Provide
them one of two ways:

**Environment variables (preferred — nothing gets committed):**

```bash
export NAAD_DEV_EMAIL="you@example.com"
export NAAD_DEV_PASSWORD="your-password"
```

**Or** set `Email` / `Password` on the `NAADBootstrap` component in
`Assets/Scenes/Bootstrap.unity` (this puts a password in the scene file — only do
it for a throwaway account).

## Step 7 — Verify before pressing Play

```bash
python tools/verify-supabase.py                       # reads SupabaseConfig.asset
python tools/verify-supabase.py <url> <anon-key>      # or pass them explicitly
```

Expected output:

```
project: https://<ref>.supabase.co
  dns        OK
  rest       OK
  rpcs       28/28 present
OK - project is reachable and the full RPC surface exists.
```

(`28` is the client-callable surface in `backend/game-api/src/contracts/rpc-catalog.ts`. The
probe treats 401/403/400/422 as "exists but rejected the anonymous call", which is the
correct answer for anything requiring `auth.uid()`; only a 404 means the function is absent.)

Anything reported as `MISSING RPC` means the bundle did not fully apply — re-run
Step 2 and read the SQL error.

## Step 8 — Play

1. Open `game/unity` in Unity `6000.6.5f1`.
2. Open `Assets/Scenes/Bootstrap.unity` (build index 0).
3. Press **Play**.

You get an IMGUI panel (`FirstNightHud`, `OnGUI` — no prefabs or canvas needed):

| Button | What it exercises |
|---|---|
| `1. Start Night` | `naad_start_night`, seeds Tunde's beach invite |
| `GO` / `ASK DETAILS` / `DECLINE` | `naad_respond_message` |
| `3. Suya Spot + Buy` | `TRAVEL` + `SPEND` (₦ deducted, receipt written) |
| `4. Club + Ticket` | `TRAVEL` + ticket purchase |
| `Talk to Tunde (AI)` | Edge Function `npc-dialogue` → Gemini (optional, see below) |
| `5. Complete Night` | `naad_complete_night` → night summary |
| `Run Full Night (GO)` | the whole loop in one press |

The panel shows the current step, the wallet balance, and any error.

## Optional — AI dialogue

`Talk to Tunde (AI)` needs the edge function deployed and a Gemini key:

```bash
supabase functions deploy npc-dialogue --project-ref <new-ref>
supabase secrets set GEMINI_API_KEY=<key> --project-ref <new-ref>
```

`verify_jwt = true` is already set in `supabase/config.toml`. Every other button
in the night works without this.

## What is still not playable after this

This runbook gets the **First Night command loop** running against a real
authoritative backend. It does not give you a third-person world:

- The player has no character in the scenes yet — `PlayerRig.prefab` is wired but
  no scene instantiates it.
- The `UI/` shell has no committed prefabs, so `FirstNightHud` (IMGUI) remains the
  only interface.
- Those need Unity Editor work, tracked in `docs/IMPLEMENTATION_PLAN_REMAINING.md`
  under Gate D / W5.

## If you want a different project later

Nothing is hard-coded except `SupabaseConfig.asset`. Change the URL and anon key,
create a user, and re-run Step 2 against the new project.
