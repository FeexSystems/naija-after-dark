# Gate 4 — Authentication

## Architecture

```
UI
 ↓
AuthController
 ↓
IAuthService  (SupabaseAuthService)
 ↓
Supabase Auth API  (anon key + user JWT only)
```

**Never** put `service_role` in Unity.

## Server bootstrap

Migration `0002_auth_player_bootstrap.sql`:

- Trigger on `auth.users` INSERT → creates `players` + `wallets` (₦100,000)
- RLS: authenticated can insert/update own player row
- Wallets remain **no client write**

Push:

```bash
export SUPABASE_ACCESS_TOKEN=sbp_...
supabase db push
```

## Unity setup

1. Dashboard → Project Settings → API → copy **Project URL** and **anon public** key.
2. In Unity: **Create → NAAD → Supabase Config**
3. Paste URL + anon key (not service_role).
4. Assign the asset to `NAADApplicationRoot.supabaseConfig`.
5. On `NAADBootstrap`, set email/password for first login (or use AuthController UI later).
6. **Required for a passing signup on the live project:** Dashboard → Authentication → Providers → Email →
   uncheck **“Confirm email”**. The live project currently has `mailer_autoconfirm=false`, so `SignUpAsync`
   returns a user **without a session** and `SignInAsync` then fails with `email_not_confirmed`.
   With confirmation ON, the flow is: sign up → confirm via email → then sign in.

## Pass condition

On a real device / Editor:

```
Install → Create account → Login → Load player → Logout → Login again
```

Flow:

1. `SignUpAsync` or Dashboard create user
2. Trigger creates player + wallet
3. `SignInAsync` → access token
4. `FetchCurrentPlayerAsync` → player row
5. `SignOutAsync` → clear PlayerPrefs
6. `SignInAsync` again → restore path works

### Headless live check

`Assets/NAAD/Editor/AuthLiveTest.cs` drives the **real** `SupabaseAuthService` +
`SupabasePlayerService` against the configured project and writes
`Library/auth-live-report.json`:

```powershell
$env:NAAD_TEST_EMAIL    = "you@realdomain.com"
$env:NAAD_TEST_PASSWORD = "..."
Unity -batchmode -nographics -quit -projectPath game/unity `
      -executeMethod NAAD.Editor.AuthLiveTest.Run
```

Notes observed in practice:

- Supabase rejects `@example.com` addresses (`email_address_invalid`) — use a real-looking domain.
- Signups are rate-limited (see `[auth.rate_limit] email_sent` in `supabase/config.toml`).
- `SignUpAsync` returns `false` when email confirmation is required, even though the user
  **was** created — the log shows `user created but no session (confirm email?)`.

## Security checklist

| Item | Rule |
|------|------|
| anon key in client | OK |
| service_role in client | **Forbidden** |
| Gemini key in client | **Forbidden** |
| Wallet balance from client | **Never trust** |
| Session storage | PlayerPrefs MVP — upgrade later |
