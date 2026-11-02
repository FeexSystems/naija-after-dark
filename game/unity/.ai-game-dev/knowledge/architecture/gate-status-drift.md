# Gate Status & Doc Drift

**Gotcha:** the three status documents disagree, and the oldest is the one agents
tend to read first. Check dates before trusting any of them.

| Doc | Dated | State |
|---|---|---|
| `docs/PROJECT_PLAN.md` | 2026-10-14 | Roadmap and pills. Gate C/D shown as planned. |
| `docs/GATE_STATUS.md` | 2026-10-23 | Ledger. Gate C "in progress, not release-ready". |
| `PRODUCTION_READINESS_STATUS.md` | 2026-10-29 | Gate C "blocked". |

Gate C assets (placeholder clips, rig prefab, avatar recovery) landed *after*
`GATE_STATUS.md` was last edited, so every one of them understates current state.
Establish truth with a CI run, not by reading a ledger.

## Corrected claims (2026-11-02)

- `GATE_STATUS.md` claimed `first-night-live-test` passed inside a green CI run. It
  did not: that job is gated on `workflow_dispatch` + `inputs.run_live_first_night_test`
  and is **skipped** on push and pull_request. A green PR run says nothing about
  First Night.
- The Unity MCP endpoint was documented as a fixed `http://localhost:17890/mcp/rpc`
  in four places. That port is not listened on. The live endpoint is written to
  `game/unity/.mcp.json` per Unity session — read it there, never hardcode a port.
- `PROJECT_PLAN.md` had an unterminated code fence that swallowed the entire
  roadmap into a code block, plus an orphaned duplicate fragment at the end.

## Why this keeps happening

Status docs here are written as snapshots ("Last audit: …") but read as current
truth. When landing a gate, update the ledger in the same commit — otherwise the
doc becomes an argument against the work that was just finished.

Related: [Mutation Surface](/architecture/mutation-surface.md),
[Gate C Asset Sourcing](/architecture/gate-c-asset-gap.md).
