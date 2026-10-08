/**
 * NAAD Game Command contracts — Phase A unified router
 * All mutations: naad_execute_command(requestId, type, payload)
 */

export type GameCommandType =
  | "TRAVEL"
  | "SPEND"
  | "BUY"
  | "START_NIGHT"
  | "COMPLETE_NIGHT"
  | "PHONE_REPLY"
  | "JOIN_ROOM"
  | "LEAVE_ROOM"
  | "ATTEND_EVENT"
  | "RELATIONSHIP_DELTA"
  | "SET_CAREER"
  | "ACCEPT_OPPORTUNITY"
  | "COMMIT_MEMORY"
  | "SET_WORLD_PERIOD";

export const COMMAND_REGISTRY: readonly GameCommandType[] = [
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

export interface GameCommand {
  requestId: string;
  type: GameCommandType;
  payload: Record<string, unknown>;
  playerId?: string;
  clientTimestamp?: string;
}

export interface CommandResult {
  requestId: string;
  success: boolean;
  errorCode?: string | null;
  errorMessage?: string | null;
  payload?: Record<string, unknown>;
}

export interface TravelPayload {
  toLocationId: string;
}

export interface SpendPayload {
  sku: string;
  locationId?: string;
}

export interface PhoneReplyPayload {
  messageId: string;
  action: "GO" | "ASK_DETAILS" | "DECLINE";
}

export interface JoinRoomPayload {
  locationId: string;
}

export interface AttendEventPayload {
  eventId: string;
}

export interface RelationshipDeltaPayload {
  targetId: string;
  trust?: number;
  respect?: number;
  affection?: number;
  loyalty?: number;
  conflict?: number;
  reason?: string;
}

export interface SetCareerPayload {
  careerId: string;
}

export interface AcceptOpportunityPayload {
  opportunityId: string;
}

export interface CommitMemoryPayload {
  npcId: string;
  memoryType: string;
  summary: string;
  importance?: number;
}

export interface SetWorldPeriodPayload {
  period: string;
}

export type DomainEventType =
  | "PLAYER_TRAVELED"
  | "MONEY_SPENT"
  | "ITEM_PURCHASED"
  | "RELATIONSHIP_CHANGED"
  | "PHONE_MESSAGE_RECEIVED"
  | "PHONE_MESSAGE_RESPONDED"
  | "NIGHT_STARTED"
  | "NIGHT_COMPLETED"
  | "ROOM_JOINED"
  | "ROOM_LEFT"
  | "EVENT_ATTENDED"
  | "OPPORTUNITY_OPENED"
  | "OPPORTUNITY_ACCEPTED"
  | "CAREER_SET"
  | "NPC_MEMORY_CREATED"
  | "WORLD_REACTIVITY"
  | "WORLD_PERIOD_CHANGED";

export interface DomainEvent {
  id: string;
  eventType: DomainEventType | string;
  playerId: string | null;
  requestId: string | null;
  payload: Record<string, unknown>;
  createdAt: string;
}

export const LOCATION_IDS = {
  APARTMENT: "a1111111-1111-1111-1111-111111111101",
  SUYA_SPOT: "a1111111-1111-1111-1111-111111111102",
  STREET: "a1111111-1111-1111-1111-111111111103",
  NIGHTCLUB: "a1111111-1111-1111-1111-111111111104",
  BEACH: "a1111111-1111-1111-1111-111111111105",
} as const;

export function toExecuteCommandBody(cmd: GameCommand): {
  p_request_id: string;
  p_type: string;
  p_payload: Record<string, unknown>;
} {
  return {
    p_request_id: cmd.requestId,
    p_type: cmd.type,
    p_payload: cmd.payload ?? {},
  };
}
