# Architecture & Decisions

Durable architectural decisions for NAAD and the reasons behind them — the trust model, the RPC mutation surface, and the client/Supabase/AI boundaries. Record a decision when it changes how the client talks to the authoritative backend.

- [Mutation Surface](/architecture/mutation-surface.md) — which RPCs the client may call, and why the "single command surface" claim was retired.
- [Gate C Asset Sourcing](/architecture/gate-c-asset-gap.md) — **resolved**: real Mixamo takes landed in `3b0749c`; the real defect was broken wiring, and why the old note was wrong.
- [Gate Status & Doc Drift](/architecture/gate-status-drift.md) — the status docs disagree; how to establish truth and which claims were corrected.
- [Duplicate Scene Copies Re-GUID the Tracked Ones](/architecture/scene-guid-duplication.md) — why a stray copy of a scene silently rewrites GUIDs and breaks `ValidateScenes`.
- [On-Device LLM Stays](/architecture/on-device-llm-decision.md) — LLMUnity is planned scope, not dead code; what that obliges before Gate 16.
- [Compiling NAAD C# Without the Unity Editor](/architecture/offline-csharp-compile.md) — typecheck `Assets/NAAD` in ~5 s with `dotnet build` when the MCP bridge is down.
- [Unity MCP Defines Are Self-Managed](/architecture/mcp-defines-self-managed.md) — stripping the MCP scripting defines by hand does not survive a domain reload.
