/**
 * Explicit RPC catalog for NAAD game-api (Phase D).
 * Mirrors live Postgres functions the Unity client may call.
 *
 * Two mutation shapes exist, and both are intentional (see
 * `game/unity/.ai-game-dev/knowledge/architecture/mutation-surface.md`):
 *  - `command_router`  — the general gameplay router, carries idempotency.
 *  - `mutation`        — server-owned transitions the client cannot compose
 *                        (night lifecycle, phone, NPC memory, world clock).
 *
 * Every entry here must exist in `supabase/migrations`. A name that is not in
 * SQL is a 404 at runtime, so this list is checked against the migrations by
 * `contracts.test.ts`.
 */

export type RpcKind = "command_router" | "mutation" | "read";

export interface RpcEntry {
  name: string;
  kind: RpcKind;
  description: string;
  /** Payload / arg keys (snake_case as sent on the wire). */
  args: string[];
}

/** Authoritative list — keep in sync with supabase/migrations. */
export const RPC_CATALOG: readonly RpcEntry[] = [
  // ---- Command router -----------------------------------------------------
  {
    name: "naad_execute_command",
    kind: "command_router",
    description: "Unified mutation router (request_id + type + payload).",
    args: ["p_request_id", "p_type", "p_payload"],
  },

  // ---- Night lifecycle (server-owned transitions) -------------------------
  {
    name: "naad_start_night",
    kind: "mutation",
    description: "Open a night session; closes any stale one and seeds the invite.",
    args: ["p_request_id"],
  },
  {
    name: "naad_complete_night",
    kind: "mutation",
    description: "Close the night and return the summary payload.",
    args: ["p_request_id"],
  },
  {
    name: "naad_advance_world_time",
    kind: "mutation",
    description: "Advance the authoritative world clock by elapsed real time.",
    args: [],
  },
  {
    name: "naad_set_world_period",
    kind: "mutation",
    description: "Set the world period. Server-side/admin only — not client-dispatchable.",
    args: ["p_period"],
  },

  // ---- Phone ---------------------------------------------------------------
  {
    name: "naad_list_messages",
    kind: "read",
    description: "Inbox messages for the authenticated player.",
    args: [],
  },
  {
    name: "naad_respond_message",
    kind: "mutation",
    description: "GO / ASK / DECLINE reply to a message.",
    args: ["p_request_id", "p_message_id", "p_action"],
  },
  {
    name: "naad_seed_tunde_beach_invite",
    kind: "mutation",
    description: "Seed Tunde's beach invite (per-player deduped).",
    args: [],
  },

  // ---- Economy -------------------------------------------------------------
  {
    name: "naad_get_wallet",
    kind: "read",
    description: "Current player wallet balances.",
    args: [],
  },
  {
    name: "naad_spend",
    kind: "mutation",
    description: "Spend on a SKU at the player's current location.",
    args: ["p_request_id", "p_sku", "p_location_id"],
  },
  {
    name: "naad_buy",
    kind: "mutation",
    description: "Buy an item SKU at the player's current location.",
    args: ["p_request_id", "p_item_sku", "p_location_id"],
  },
  {
    name: "naad_travel",
    kind: "mutation",
    description: "Move the player to another location.",
    args: ["p_request_id", "p_to_location_id"],
  },

  // ---- Relationships -------------------------------------------------------
  {
    name: "naad_ensure_relationship",
    kind: "mutation",
    description: "Create the player's relationship row for a target NPC.",
    args: ["p_target_id"],
  },
  {
    name: "naad_apply_relationship_delta",
    kind: "mutation",
    description: "Apply a clamped (±10) trust/respect delta.",
    args: ["p_request_id", "p_target_id", "p_trust", "p_respect"],
  },
  {
    name: "naad_get_relationship",
    kind: "read",
    description: "Player's relationship with a target NPC.",
    args: ["p_target_id"],
  },

  // ---- Dialogue / NPC memory ----------------------------------------------
  {
    name: "naad_get_npc_greeting",
    kind: "read",
    description: "Deterministic greeting for non-AI NPCs.",
    args: ["p_npc_id"],
  },
  {
    name: "naad_npc_dialogue_context",
    kind: "read",
    description: "Bounded context pack for Tunde AI dialogue.",
    args: ["p_npc_id"],
  },
  {
    name: "naad_commit_npc_memory",
    kind: "mutation",
    description: "Commit an NPC memory candidate (24h summary dedupe).",
    args: ["p_request_id", "p_npc_id", "p_memory_type", "p_summary", "p_importance"],
  },

  // ---- World / events ------------------------------------------------------
  {
    name: "naad_get_world_clock",
    kind: "read",
    description: "Authoritative world time / period.",
    args: [],
  },
  {
    name: "naad_list_events",
    kind: "read",
    description: "Scheduled and active world events.",
    args: [],
  },
  {
    name: "naad_attend_event",
    kind: "mutation",
    description: "Attend a world event (requires being at the venue).",
    args: ["p_request_id", "p_event_id"],
  },

  // ---- Social graph --------------------------------------------------------
  {
    name: "naad_refresh_opportunities",
    kind: "mutation",
    description: "Regenerate the player's opportunities.",
    args: [],
  },
  {
    name: "naad_accept_opportunity",
    kind: "mutation",
    description: "Accept a generated opportunity.",
    args: ["p_request_id", "p_opportunity_id"],
  },
  {
    name: "naad_set_career",
    kind: "mutation",
    description: "Set the player's career.",
    args: ["p_request_id", "p_career_id"],
  },

  // ---- Rooms / presence (ephemeral) ---------------------------------------
  {
    name: "naad_join_room",
    kind: "mutation",
    description: "Join or open the room at a location.",
    args: ["p_request_id", "p_location_id"],
  },
  {
    name: "naad_leave_room",
    kind: "mutation",
    description: "Leave the player's current room.",
    args: ["p_request_id"],
  },
  {
    name: "naad_update_presence",
    kind: "mutation",
    description: "Push ephemeral position/emote presence.",
    args: ["p_pos_x", "p_pos_y", "p_pos_z", "p_rot_y", "p_animation", "p_emote"],
  },
  {
    name: "naad_list_room_presence",
    kind: "read",
    description: "Presence rows for a location (display hints only).",
    args: ["p_location_id"],
  },
] as const;

/**
 * Every SQL function that is NOT reachable from the client.
 * Kept explicit so "the client can only call what is listed above" is a
 * verifiable claim rather than an assumption.
 */
export const INTERNAL_RPCS: readonly string[] = [
  // Server-internal world simulation. No auth.uid() check by design — it is
  // invoked by the attendance path, never by a client.
  "naad_react_to_event_attendance",
  // Pure helpers.
  "naad_period_from_minutes",
  "naad_nightlife_for_period",
] as const;

/**
 * `returns trigger` functions — attached to tables, never called over RPC.
 * Listed so the catalog-vs-migrations check can tell "not an RPC" apart from
 * "an RPC somebody forgot to catalogue".
 */
export const TRIGGER_FUNCTIONS: readonly string[] = [
  "handle_new_user",
  "set_updated_at",
  "set_default_player_location",
] as const;

export function listCommandRouterTypes(): readonly string[] {
  // Re-exported expectation: every mutation type hits the router.
  return [
    "TRAVEL",
    "SPEND",
    "BUY",
    "START_NIGHT",
    "COMPLETE_NIGHT",
    "PHONE_REPLY",
    "JOIN_ROOM",
    "LEAVE_ROOM",
    "ATTEND_EVENT",
    "RELATIONSHIP_DELTA",
    "SET_CAREER",
    "ACCEPT_OPPORTUNITY",
    "COMMIT_MEMORY",
    "SET_WORLD_PERIOD",
  ] as const;
}
