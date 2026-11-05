# On-Device LLM Stays

**Decision (2026-11-05):** `Assets/LLMUnity/` is kept. On-device LLM is a planned
feature, not dead weight. It was reviewed for deletion and rejected.

## What was considered

An audit measured the payload and found it entirely unreferenced:

| Artifact | Size | Tracked | Referenced by game code |
|---|---|---|---|
| `Assets/LLMUnity/` (211 files, vendored LLMUnity SDK) | 15 MB | yes | **no — zero references** |
| `Assets/StreamingAssets/LlamaLib-v2.0.5/` (runtime payload) | 3.9 GB | no (gitignored) | no |

`undream.llmunity.Runtime.asmdef` declares no `includePlatforms`, so the Runtime
assembly compiles into every player build and ships `libusearch_c.so` for Android
arm64-v8a (1.96 MB) and x86_64 (1.86 MB). Deleting the folder was the obvious
cleanup.

## Why it was kept

On-device inference is intended product scope. The cost of carrying it while it is
unwired is a few MB; the cost of deleting it and rebuilding the integration later
is much higher. The audit's finding stands — it is simply a *known, accepted* cost
rather than an oversight.

## What now has to be true

The decision moves the problem rather than solving it. Before Gate 16 ships:

1. **Stop compiling it into builds that do not use it.** Either give
   `undream.llmunity.Runtime.asmdef` an `includePlatforms` restriction, or wire the
   feature. Today every APK carries ~3.8 MB of native code for nothing.
2. **Fix the platform asymmetry.** The Windows `libusearch_c.dll` is Editor-visible
   on Windows hosts only; the Linux connector sees nothing. Any unconditional
   reference breaks the Linux CI build. CI already reports this as
   `[native-plugin-compatibility]`.
3. **Document the payload fetch.** `StreamingAssets/LlamaLib-v2.0.5/` (3.9 GB) is
   gitignored, so a clean clone cannot run inference at all.
4. **Choose and measure a model.** No model has been picked, and a mobile LLM
   payload is tens to hundreds of MB. That is the real Gate 16 sizing question.

## Note on scope

This is *not* a contradiction of the server-authoritative pillar. On-device
inference must remain presentation/interpretation only — it may never become a
mutation path. All state changes still go through the RPC surface. See
[Mutation Surface](/architecture/mutation-surface.md).

Related: [Gate Status & Doc Drift](/architecture/gate-status-drift.md).
