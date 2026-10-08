import { validateDialogueResponse } from "./validate";

function assert(cond: boolean, msg: string) {
  if (!cond) throw new Error(msg);
}

// valid
{
  const r = validateDialogueResponse({
    dialogue: "Beach dey hot tonight. You pulling up?",
    emotion: "excited",
    intent: { type: "INVITE_EVENT" },
    memoryCandidate: { summary: "Invited player to beach", importance: 0.7, type: "INTRO" },
  });
  assert(r !== null, "expected valid");
  assert(r!.dialogue.includes("Beach"), "dialogue");
  assert(r!.emotion === "excited", "emotion");
}

// invalid dialogue
{
  const r = validateDialogueResponse({ emotion: "happy" });
  assert(r === null, "missing dialogue should fail");
}

// clamp deltas
{
  const r = validateDialogueResponse({
    dialogue: "Sharp.",
    emotion: "neutral",
    suggestedRelationshipDelta: { trust: 99, conflict: -50 },
  });
  assert(r !== null, "valid with deltas");
  assert(r!.suggestedRelationshipDelta?.trust === 10, "clamp trust");
  assert(r!.suggestedRelationshipDelta?.conflict === -10, "clamp conflict");
}

console.log("validate.test.ts: ok");
