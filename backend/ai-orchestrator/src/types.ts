export type Emotion =
  | "happy"
  | "excited"
  | "neutral"
  | "angry"
  | "sad"
  | "nervous";

export type IntentType =
  | "INVITE_EVENT"
  | "OFFER_JOB"
  | "REQUEST_PAYMENT"
  | "MAKE_PLAN"
  | "REFUSE"
  | "NONE";

export type MemoryType =
  | "FAVOR"
  | "INSULT"
  | "SHARED_EVENT"
  | "PROMISE"
  | "TRANSACTION"
  | "INTRO"
  | "OTHER";

export interface NPCDialogueResponse {
  dialogue: string;
  emotion: Emotion;
  intent?: { type: IntentType; targetId?: string };
  suggestedRelationshipDelta?: {
    trust?: number;
    respect?: number;
    affection?: number;
    conflict?: number;
  };
  memoryCandidate?: {
    summary: string;
    importance: number;
    type?: MemoryType;
  };
}

export interface DialogueRequest {
  requestId: string;
  npcId: string;
  playerMessage: string;
}

export const TUNDE_ID = "e1111111-1111-1111-1111-111111111101";
