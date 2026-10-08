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
6. Disable email confirmations for dev: Auth → Providers → Email → uncheck “Confirm email” (optional).

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

## Security checklist

| Item | Rule |
|------|------|
| anon key in client | OK |
| service_role in client | **Forbidden** |
| Gemini key in client | **Forbidden** |
| Wallet balance from client | **Never trust** |
| Session storage | PlayerPrefs MVP — upgrade later |
