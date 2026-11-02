# Architecture & Decisions

Durable architectural decisions for NAAD and the reasons behind them — the trust model, the RPC mutation surface, and the client/Supabase/AI boundaries. Record a decision when it changes how the client talks to the authoritative backend.

- [Mutation Surface](/architecture/mutation-surface.md) — which RPCs the client may call, and why the "single command surface" claim was retired.
- [Gate C Asset Sourcing](/architecture/gate-c-asset-gap.md) — the eight animation clips cannot come from assets already in the repo; evidence and what is required.
- [Gate Status & Doc Drift](/architecture/gate-status-drift.md) — the status docs disagree; how to establish truth and which claims were corrected.
- [Unity MCP Defines Are Self-Managed](/architecture/mcp-defines-self-managed.md) — stripping the MCP scripting defines by hand does not survive a domain reload.
