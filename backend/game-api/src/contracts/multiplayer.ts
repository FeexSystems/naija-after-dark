/** Ephemeral room presence — never authoritative for money/inventory */
export interface GameRoom {
  id: string;
  locationId: string;
  maxPlayers: number;
  currentPlayers: number;
  eventId?: string | null;
  status: "OPEN" | "FULL" | "CLOSED";
}

export interface RoomMember {
  playerId: string;
  displayName: string;
  pos: { x: number; y: number; z: number };
  rotY: number;
  animation: string;
  emote?: string | null;
}

export interface WorldEvent {
  id: string;
  name: string;
  locationId: string;
  startTime: string;
  endTime: string;
  requiredReputation: number;
  crowdProfile: "LOW" | "MEDIUM" | "HIGH" | "PACKED";
  status: "SCHEDULED" | "ACTIVE" | "ENDED" | "CANCELLED";
}

export interface Opportunity {
  id: string;
  opportunityType: "INTRO" | "GIG" | "EVENT_ACCESS" | "JOB_OFFER" | "FAVOR";
  title: string;
  description: string;
  status: "OPEN" | "ACCEPTED" | "DECLINED" | "EXPIRED";
  sourceNpcId?: string | null;
}
