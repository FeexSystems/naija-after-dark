import type { NPCDialogueResponse, Emotion, IntentType, MemoryType } from "./types";

const EMOTIONS: Emotion[] = ["happy", "excited", "neutral", "angry", "sad", "nervous"];
const INTENTS: IntentType[] = [
  "INVITE_EVENT",
  "OFFER_JOB",
  "REQUEST_PAYMENT",
  "MAKE_PLAN",
  "REFUSE",
  "NONE",
];
const MEMORY_TYPES: MemoryType[] = [
  "FAVOR",
  "INSULT",
  "SHARED_EVENT",
  "PROMISE",
  "TRANSACTION",
  "INTRO",
  "OTHER",
];

function clampDelta(n: unknown): number | undefined {
  if (typeof n !== "number" || !Number.isFinite(n)) return undefined;
  return Math.max(-10, Math.min(10, Math.round(n)));
}

/**
 * Validate Gemini JSON. Returns null if invalid.
 * Never applies mutations — caller decides.
 */
export function validateDialogueResponse(raw: unknown): NPCDialogueResponse | null {
  if (!raw || typeof raw !== "object") return null;
  const o = raw as Record<string, unknown>;

  if (typeof o.dialogue !== "string" || o.dialogue.trim().length === 0) return null;
  if (o.dialogue.length > 500) o.dialogue = (o.dialogue as string).slice(0, 500);

  if (typeof o.emotion !== "string" || !EMOTIONS.includes(o.emotion as Emotion)) {
    o.emotion = "neutral";
  }

  const result: NPCDialogueResponse = {
    dialogue: (o.dialogue as string).trim(),
    emotion: o.emotion as Emotion,
  };

  if (o.intent && typeof o.intent === "object") {
    const intent = o.intent as Record<string, unknown>;
    if (typeof intent.type === "string" && INTENTS.includes(intent.type as IntentType)) {
      result.intent = {
        type: intent.type as IntentType,
        targetId: typeof intent.targetId === "string" ? intent.targetId : undefined,
      };
    }
  }

  if (o.suggestedRelationshipDelta && typeof o.suggestedRelationshipDelta === "object") {
    const d = o.suggestedRelationshipDelta as Record<string, unknown>;
    result.suggestedRelationshipDelta = {
      trust: clampDelta(d.trust),
      respect: clampDelta(d.respect),
      affection: clampDelta(d.affection),
      conflict: clampDelta(d.conflict),
    };
  }

  if (o.memoryCandidate && typeof o.memoryCandidate === "object") {
    const m = o.memoryCandidate as Record<string, unknown>;
    if (typeof m.summary === "string" && m.summary.trim().length >= 3) {
      const importance =
        typeof m.importance === "number" ? Math.max(0, Math.min(1, m.importance)) : 0.5;
      const type =
        typeof m.type === "string" && MEMORY_TYPES.includes(m.type as MemoryType)
          ? (m.type as MemoryType)
          : "OTHER";
      result.memoryCandidate = {
        summary: m.summary.trim().slice(0, 280),
        importance,
        type,
      };
    }
  }

  return result;
}
