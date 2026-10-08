/**
 * NAAD Game Command contracts (Gate 5)
 * TypeScript is the explicit API contract language.
 * Runtime authority remains PostgreSQL validation + RPC.
 */

export type GameCommandType =
  | "MOVE"
  | "INTERACT"
  | "TALK"
  | "TRAVEL"
  | "BUY"
  | "SELL"
  | "SEND_MESSAGE"
  | "ACCEPT_EVENT";

export interface GameCommand {
  requestId: string;
  playerId: string;
  type: GameCommandType;
  payload: Record<string, unknown>;
  clientTimestamp: string; // ISO-8601
}

export interface CommandResult {
  requestId: string;
  success: boolean;
  errorCode?: string | null;
  errorMessage?: string | null;
  payload?: Record<string, unknown>;
}

/** TRAVEL payload */
export interface TravelPayload {
  toLocationId: string;
}

export interface TravelResultPayload {
  fromLocationId: string;
  toLocationId: string;
}

export type DomainEventType = "PLAYER_TRAVELED";

export interface DomainEvent {
  id: string;
  eventType: DomainEventType | string;
  playerId: string | null;
  requestId: string | null;
  payload: Record<string, unknown>;
  createdAt: string;
}

/** Fixed seed location IDs (migration 0003) */
export const LOCATION_IDS = {
  APARTMENT: "a1111111-1111-1111-1111-111111111101",
  SUYA_SPOT: "a1111111-1111-1111-1111-111111111102",
} as const;
