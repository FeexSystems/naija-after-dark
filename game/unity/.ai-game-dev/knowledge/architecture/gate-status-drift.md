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

## Audit findings go stale too (2026-11-05)

The 2026-11-02 audit's two headline defects had both already been fixed on `main`
by the time anyone acted on them:

| Audit claim | Reality on `main` |
|---|---|
| Two duplicate `SceneAnchor` types; interaction inert | One type (`NAAD.World.SceneAnchor`), 22 anchors, `SceneAnchorCommandMap` resolves them, controller has `using NAAD.World` |
| Gate C blocked; assets absent; clips are placeholders | `3b0749c` replaced all eight with real Mixamo takes (672,477 lines) |

Both "corrections" in the plan were written from a working tree that was already
behind a commit that fixed them. **An audit is a snapshot of a working tree, not of
`main`.** Before acting on a finding, confirm it still reproduces — `git log` the
file the finding names.

The failure mode is asymmetric and worth naming: a stale *bug* report wastes work
and can cause someone to "fix" a non-problem; a stale *status* claim hides a real
defect. The 2026-11-05 audit found both in the same tree.

## Why this keeps happening

Status docs here are written as snapshots ("Last audit: …") but read as current
truth. When landing a gate, update the ledger in the same commit — otherwise the
doc becomes an argument against the work that was just finished.

Related: [Mutation Surface](/architecture/mutation-surface.md),
[Gate C Asset Sourcing](/architecture/gate-c-asset-gap.md).
