# NAAD — Next Architecture Engineering

**Date:** 2026-10-08  
**Status after Gates 0–20:** World Model + command RPCs live; Unity is still a thin client shell.

---

## 1. Where we are

### What is solid

| Layer | State |
|-------|--------|
| Trust boundaries | Clear: Postgres authoritative, Unity untrusted, Gemini proposals-only |
| Schema | 8 migrations, RLS, constraints |
| Commands | RPC style with `request_id` + `command_receipts` + `domain_events` |
| First Night server loop | Start → phone → spend/travel → complete → summary |
| AI path | Orchestrator + validate; Edge Function scaffold |
| Ephemeral vs persistent | Rooms/presence ≠ money/relationships |

### What is incomplete

| Gap | Risk if ignored |
|-----|-----------------|
| **No unified command router** | N RPC names; client must know every function |
| **Unity not wired to full loop** | Server loop unplayable |
| **Naive JSON parsing in C#** | Fragile under real payloads |
| **Edge Function not deployed** | Tunde AI silent |
| **No Realtime subscription layer** | Multiplayer presence is poll-only |
| **No command registry / contract tests** | Drift between TS contracts and SQL |
| **game-api is contracts-only** | Name implies a service that does not exist yet |

**Architectural stance stays the same.** Next work is **integration and hardening**, not new product domains.

---

## 2. Non-goals (explicit)

Do **not** do these next:

1. Split into microservices  
2. Rewrite Unity networking from scratch  
3. Add Redis / Kafka / custom game server  
4. Expand Lagos into a full city  
5. Optimize art/CPU before Gate 16 measurements  
6. Put service-role or Gemini keys in Unity  

---

## 3. Next engineering phases (ordered)

### Phase A — Command surface unification ✅ DONE

**Problem:** Client calls `naad_travel`, `naad_spend`, `naad_start_night`, … as separate RPCs.

**Change:**

```
Unity  →  POST /rest/v1/rpc/naad_execute_command
          { requestId, type, payload }
       →  single Postgres function dispatches by type
       →  existing handlers (travel, spend, …)
```

| Type | Handler (existing) |
|------|--------------------|
| `TRAVEL` | `naad_travel` |
| `SPEND` | `naad_spend` |
| `START_NIGHT` | `naad_start_night` |
| `COMPLETE_NIGHT` | `naad_complete_night` |
| `PHONE_REPLY` | `naad_respond_message` |
| `JOIN_ROOM` | `naad_join_room` |
| `LEAVE_ROOM` | `naad_leave_room` |
| `ATTEND_EVENT` | `naad_attend_event` |
| `RELATIONSHIP_DELTA` | `naad_apply_relationship_delta` |
| `SET_CAREER` | `naad_set_career` |
| `ACCEPT_OPPORTUNITY` | `naad_accept_opportunity` |

**Reads stay separate** (wallet, clock, messages, events, presence) — not every read must go through the command bus.

**Deliverables:**

- Migration: `naad_execute_command(request_id, type, payload jsonb)`
- TS: command registry map type → payload schema  
- Unity: one `ICommandService.ExecuteAsync` path for all mutations  

### Phase B — Client First Night vertical slice ✅ DONE (client code)

Wire Unity only:

1. Auth (Gate 4)  
2. Start night  
3. Phone UI (read + GO/DECLINE)  
4. Travel to Suya → spend  
5. Optional club ticket  
6. Complete night → summary screen  

No new server features. Use Phase A router.

**Pass:** one device run matches server summary shape.

### Phase C — AI dialogue live path (1 day + key)

1. `supabase secrets set GEMINI_API_KEY`  
2. Deploy `npc-dialogue`  
3. Unity `TalkToTunde` → Edge Function  
4. Optional memory commit only when `memoryCandidate` present and importance ≥ threshold  

Still: Gemini never writes DB.

### Phase D — Contract hardening (2 days)

| Item | Action |
|------|--------|
| JSON in Unity | Prefer `UnityEngine.JsonUtility` / small DTO structs over string search |
| Contract tests | TS or SQL tests: invalid spend, double request_id, travel same location |
| OpenAPI-ish list | Document every RPC in `backend/game-api` as the API catalog |
| Logging | Structured `requestId` on client + server events |

### Phase E — Presence Realtime (optional, after B)

Subscribe to `room_presence` for current `room_id` via Supabase Realtime.

Rules:

- Presence updates are **hints**  
- Server still owns join/leave counts  
- No trusting client for wallet on presence channel  

### Phase F — Mobile profile (Gate 16)

Only after B plays on device. Use `docs/GATE16_MOBILE_OPT.md`.

---

## 4. Recommended module map (unchanged topology)

```
Unity (untrusted)
  ├── Auth
  ├── Commands  → naad_execute_command
  ├── Reads     → get_wallet, get_clock, list_messages, list_events, presence
  └── AI talk   → Edge Function npc-dialogue (JWT)

Supabase
  ├── Postgres World Model + RPCs
  ├── Auth
  ├── Realtime (later: presence)
  └── Edge Functions (Gemini only)

ai-orchestrator
  └── validate + prompt assembly (shared with Edge Function logic)
```

No new deployable service until a measured bottleneck appears.

---

## 5. Command router sketch (Phase A)

```sql
-- conceptual
naad_execute_command(p_request_id text, p_type text, p_payload jsonb)
  → case p_type
      when 'TRAVEL' then naad_travel(...)
      when 'SPEND'  then naad_spend(...)
      ...
      else error UNSUPPORTED_COMMAND
```

Idempotency remains on `command_receipts.request_id` inside each handler.

---

## 6. Success metrics for “architecture done enough”

1. One Unity build completes First Night against production project.  
2. All mutations go through ≤ 2 entry points (command RPC + AI edge).  
3. Contract list in `backend/game-api` matches live RPCs.  
4. Zero service-role usage in client.  
5. Domain events queryable for a single night session.

---

## 7. Immediate next action

**Implement Phase A** (`naad_execute_command` + Unity single command path), then **Phase B** on device.

Everything else waits on a playable local night.
