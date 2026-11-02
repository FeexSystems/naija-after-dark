# Mutation Surface

**Decision (2026-11-02):** the client may call a small, fixed set of named RPCs, not
only `naad_execute_command`. The architecture doc was amended to match the code
rather than refactoring the RPCs into the router.

## What the client actually calls

Mutations:
`naad_execute_command`, `naad_start_night`, `naad_complete_night`,
`naad_respond_message`, `naad_seed_tunde_beach_invite`, `naad_commit_npc_memory`,
`naad_set_world_period`, `naad_advance_world_time`.

Reads:
`naad_get_wallet`, `naad_list_messages`, `naad_get_npc_greeting`,
`naad_get_world_clock`.

There are no direct table reads or writes from the Unity client — every call goes
through `/rest/v1/rpc/*`.

## Why

`PROJECT_PLAN.md` pillar #2 previously claimed a *single* command surface. The code
never matched: the night lifecycle, phone, NPC memory and world clock are
server-owned *transitions*, not player "commands" with a type + payload the client
composes. Folding them into `naad_execute_command` would mean inventing command
types the server alone decides when to run.

Replaced: pillar #2 "Single command surface" and success metric #2 ("≤ 2 entry
points"), both of which the code could never satisfy.

## The invariant that still holds

Postgres is never written from the client by direct table access. A new mutation
means a new named RPC — never a client-side table write.

See [Gate Status & Doc Drift](/architecture/gate-status-drift.md) for the audit that
found this.
