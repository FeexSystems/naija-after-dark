# Next phase tasks

This checklist reflects the current repository evidence. It does not authorize paid API calls, production database mutations, or live gameplay tests.

## Unity

1. Install Unity `6000.7.0b6` or provide a working host with the declared `6000.7.0b4` editor.
2. Run `NAAD.Editor.Editor.ListPackages` and preserve the package inventory log.
3. Run `NAAD.Editor.Editor.VerifyProject` and `NAAD.Editor.NAADBuild.Verify`.
4. Resolve remaining package-lock entries through Unity Package Manager; do not hand-edit generated lock data.
5. Run EditMode tests and a non-live scene/build smoke test.

## Supabase

1. Install Supabase CLI/Deno or use a staging project with equivalent migrations.
2. Apply migrations `0001` through `0009` to staging only.
3. Validate RLS, security-definer search paths, request-id idempotency, and command-router envelopes.
4. Run the First Night flow with a disposable staging account and bounded test data.
5. Keep the live Supabase workflow opt-in.

## GitHub and CI

1. Confirm the GameCI image supports the project editor version.
2. Run non-live Unity verification jobs and retain reports as artifacts.
3. Keep the live First Night job manual and isolated.
4. Review workflow failures before changing package or scene configuration.

## Provider configuration

1. Rotate any credentials previously pasted into chat.
2. Use environment variables only; never serialize provider keys into Unity assets.
3. Do not implement OpenAI prompt caching until a supported model and server-side request path are selected.
