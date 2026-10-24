/**
 * NAAD World Model — TypeScript contracts (SPEC v0.1)
 * Source of truth for shapes used by backend APIs.
 * Runtime authority remains PostgreSQL.
 */

export type UUID = string;
export type ISODateTime = string;

// --- Enums ---

export type CurrencyCode = "NGN";

export type LocationType =
  | "APARTMENT"
  | "STREET"
  | "SUYA_SPOT"
  | "NIGHTCLUB"
  | "BEACH"
  | "LOUNGE"
  | "RESTAURANT"
  | "HOTEL"
  | "OTHER";

export type NpcArchetype =
  | "PROMOTER"
  | "VENDOR"
  | "CREATIVE"
  | "DEVELOPER"
  | "SOCIALITE"
  | "OTHER";

export type MemoryType =
  | "FAVOR"
  | "INSULT"
  | "SHARED_EVENT"
  | "PROMISE"
  | "TRANSACTION"
  | "INTRO"
  | "OTHER";

export type RelationshipType =
  | "STRANGER"
  | "ACQUAINTANCE"
  | "ALLY"
  | "RIVAL"
  | "ROMANTIC"
  | "BUSINESS"
  | "OTHER";

export type Weather =
  | "CLEAR"
  | "CLOUDY"
  | "RAIN"
  | "HEAVY_RAIN"
  | "HARMATTAN";

export type WalletTxType =
  | "EARN"
  | "SPEND"
  | "TRANSFER_IN"
  | "TRANSFER_OUT"
  | "ADJUST";

export type ItemCategory = "FOOD" | "DRINK" | "TICKET" | "CLOTHING" | "MISC";

export type VehicleType =
  | "OKADA"
  | "DANFO"
  | "UBER"
  | "PRIVATE_CAR"
  | "BOAT"
  | "OTHER";

export type PropertyTenure = "OWNED" | "RENTED" | "TEMP";

export type JobTitle =
  | "DEVELOPER"
  | "DJ"
  | "DRIVER"
  | "EVENT_PROMOTER"
  | "RESTAURANT_WORKER"
  | "OTHER";

export type JobStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "FIRED";

export type MissionStatus =
  | "OFFERED"
  | "ACTIVE"
  | "COMPLETED"
  | "FAILED"
  | "EXPIRED";

export type ReputationDomain = "NIGHTLIFE" | "BUSINESS" | "STREET" | "CREATIVE";

export type CrowdProfile = "LOW" | "MEDIUM" | "HIGH" | "PACKED";

export type MessageSenderType = "NPC" | "PLAYER" | "SYSTEM";

export type SocialAuthorType = "PLAYER" | "NPC" | "SYSTEM";

export type ClientPlatform = "ANDROID" | "IOS" | "EDITOR" | "OTHER";

// --- Entities ---

export interface Player {
  id: UUID;
  displayName: string;
  createdAt: ISODateTime;
  updatedAt: ISODateTime;
}

export interface Wallet {
  playerId: UUID;
  currency: CurrencyCode;
  balance: number; // integer NGN, >= 0
  updatedAt: ISODateTime;
}

export interface WalletTransaction {
  id: UUID;
  playerId: UUID;
  type: WalletTxType;
  amount: number; // positive magnitude
  currency: CurrencyCode;
  reason?: string;
  relatedEntityType?: "ITEM" | "EVENT" | "MISSION" | "NPC" | "LOCATION" | "NONE";
  relatedEntityId?: UUID;
  requestId?: string;
  createdAt: ISODateTime;
}

export interface Location {
  id: UUID;
  name: string;
  type: LocationType;
  districtId?: UUID;
  createdAt: ISODateTime;
}

export interface District {
  id: UUID;
  name: string;
  metro?: "LAGOS";
  createdAt: ISODateTime;
}

export interface Npc {
  id: UUID;
  name: string;
  age?: number;
  archetype?: NpcArchetype;
  occupation?: string;
  currentLocationId?: UUID;
  mood: number; // 0–100
  reputation: number;
  createdAt: ISODateTime;
}

export interface NpcMemory {
  id: UUID;
  npcId: UUID;
  playerId: UUID;
  type: MemoryType;
  summary: string;
  importance: number; // 0–1
  createdAt: ISODateTime;
}

export interface Relationship {
  id: UUID;
  subjectId: UUID;
  targetId: UUID;
  trust: number;
  respect: number;
  affection: number;
  loyalty: number;
  conflict: number;
  relationshipType?: RelationshipType;
  updatedAt: ISODateTime;
}

export type WorldPeriod =
  | "MORNING" // 06:00-08:59
  | "DAY" // 09:00-15:59
  | "TRANSITION" // 16:00-18:59 sunset
  | "NIGHT" // 19:00-23:59
  | "LATE_NIGHT" // 00:00-02:59
  | "AFTER_HOURS"; // 03:00-05:59

/** Period boundaries in minutes since midnight (mirrors backend/game-api/src/contracts/world-clock.ts) */
export const PERIOD_BOUNDS: Record<WorldPeriod, { start: number; end: number }> = {
  MORNING: { start: 360, end: 539 },
  DAY: { start: 540, end: 959 },
  TRANSITION: { start: 960, end: 1139 },
  NIGHT: { start: 1140, end: 1439 },
  LATE_NIGHT: { start: 0, end: 179 },
  AFTER_HOURS: { start: 180, end: 359 },
};

export interface WorldState {
  id: 1;
  gameTime: ISODateTime;
  weather: Weather;
  trafficLevel: number; // 0–100
  nightlifeLevel: number; // 0–100
  timeScale: number;
  period?: WorldPeriod;
  minutesSinceMidnight?: number;
}

export interface WorldClock {
  gameDate: string;
  gameTime: ISODateTime;
  minutesSinceMidnight: number;
  period: WorldPeriod;
  timeScale: number;
  weather: Weather;
  trafficLevel: number;
  nightlifeLevel: number;
}

export interface Business {
  id: UUID;
  name: string;
  locationId: UUID;
  ownerNpcId?: UUID;
  createdAt: ISODateTime;
}

export interface Vehicle {
  id: UUID;
  type: VehicleType;
  ownerPlayerId?: UUID;
  label?: string;
  createdAt: ISODateTime;
}

export interface Property {
  id: UUID;
  locationId: UUID;
  ownerPlayerId?: UUID;
  tenure: PropertyTenure;
  createdAt: ISODateTime;
}

export interface Item {
  id: UUID;
  sku: string;
  name: string;
  category: ItemCategory;
  basePriceNgn: number;
}

export interface Inventory {
  id: UUID;
  playerId: UUID;
  itemId: UUID;
  quantity: number;
  updatedAt: ISODateTime;
}

export interface Job {
  id: UUID;
  playerId: UUID;
  title: JobTitle;
  status: JobStatus;
  employerNpcId?: UUID;
  createdAt: ISODateTime;
}

export interface Event {
  id: UUID;
  name: string;
  locationId: UUID;
  startTime: ISODateTime;
  endTime: ISODateTime;
  requiredReputation?: number;
  crowdProfile?: CrowdProfile;
}

export interface Mission {
  id: UUID;
  playerId: UUID;
  title: string;
  status: MissionStatus;
  giverNpcId?: UUID;
  relatedEventId?: UUID;
  createdAt: ISODateTime;
  completedAt?: ISODateTime;
}

export interface Reputation {
  playerId: UUID;
  domain: ReputationDomain;
  score: number;
  updatedAt: ISODateTime;
}

export interface Message {
  id: UUID;
  senderType: MessageSenderType;
  senderId: string;
  recipientPlayerId: UUID;
  body: string;
  readAt?: ISODateTime;
  createdAt: ISODateTime;
}

export interface SocialPost {
  id: UUID;
  authorType: SocialAuthorType;
  authorId: string;
  body: string;
  locationId?: UUID;
  eventId?: UUID;
  createdAt: ISODateTime;
}

export interface GameSession {
  id: UUID;
  playerId: UUID;
  currentLocationId?: UUID;
  clientPlatform?: ClientPlatform;
  startedAt: ISODateTime;
  lastSeenAt?: ISODateTime;
}
