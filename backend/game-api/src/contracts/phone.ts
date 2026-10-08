export type PhoneAction = "GO" | "ASK_DETAILS" | "DECLINE";

export interface PhoneMessage {
  id: string;
  senderType: "NPC" | "PLAYER" | "SYSTEM";
  senderId: string;
  recipientPlayerId: string;
  body: string;
  threadKey: string;
  actionOptions: string[];
  chosenAction?: string | null;
  readAt?: string | null;
  createdAt: string;
}

export interface NightSummary {
  title: string;
  spentNgn: number;
  transactions: number;
  peopleMet: number;
  newConnection?: string | null;
  trustWithTunde?: number | null;
  bestMoment: string;
  tomorrow: {
    opportunities: number;
    invitations: number;
    unresolved: number;
  };
}
