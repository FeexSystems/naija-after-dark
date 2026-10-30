/**
 * Explicit RPC catalog for NAAD game-api (Phase D).
 * Mirrors live Postgres functions the Unity client may call.
 * Mutations go through naad_execute_command; reads stay direct.
 */

export type RpcKind = "command_router" | "mutation_legacy" | "read" | "auth_context";

export interface RpcEntry {
  name: string;
  kind: RpcKind;
  description: string;
  /** Payload / arg keys (snake_case as sent on the wire). */
  args: string[];
}

/** Authoritative list — keep in sync with supabase/migrations. */
export const RPC_CATALOG: readonly RpcEntry[] = [
  {
    name: "naad_execute_command",
    kind: "command_router",
    description: "Phase A unified mutation router (request_id + type + payload).",
    args: ["p_request_id", "p_type", "p_payload"],
  },
  {
    name: "naad_get_wallet",
    kind: "read",
    description: "Current player wallet balances.",
    args: [],
  },
  {
    name: "naad_get_world_clock",
    kind: "read",
    description: "Authoritative world time / period.",
    args: [],
  },
  {
    name: "naad_list_phone_messages",
    kind: "read",
    description: "Inbox messages for the authenticated player.",
    args: [],
  },
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
    kind: "mutation_legacy",
    description: "Prefer COMMIT_MEMORY via naad_execute_command.",
    args: ["p_request_id", "p_npc_id", "p_memory_type", "p_summary", "p_importance"],
  },
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
