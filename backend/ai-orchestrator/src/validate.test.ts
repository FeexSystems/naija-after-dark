import { validateDialogueResponse } from "./validate.ts";

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

// unknown emotion falls back to neutral
{
  const r = validateDialogueResponse({ dialogue: "Wetin dey?", emotion: "furious" });
  assert(r !== null, "valid despite unknown emotion");
  assert(r!.emotion === "neutral", "unknown emotion -> neutral");
}

// missing / non-string emotion falls back to neutral
{
  const r = validateDialogueResponse({ dialogue: "Oya." });
  assert(r?.emotion === "neutral", "missing emotion -> neutral");
}

// dialogue longer than 500 chars is truncated
{
  const r = validateDialogueResponse({ dialogue: "a".repeat(900), emotion: "happy" });
  assert(r !== null && r.dialogue.length === 500, "dialogue capped at 500");
}

// whitespace-only dialogue is rejected
{
  const r = validateDialogueResponse({ dialogue: "   ", emotion: "happy" });
  assert(r === null, "blank dialogue should fail");
}

// unknown intent type is dropped, known one kept with targetId
{
  const bad = validateDialogueResponse({ dialogue: "Hey.", intent: { type: "SUMMON_DEMON" } });
  assert(bad !== null && bad.intent === undefined, "unknown intent dropped");

  const good = validateDialogueResponse({ dialogue: "Hey.", intent: { type: "OFFER_JOB", targetId: "n1" } });
  assert(good?.intent?.type === "OFFER_JOB" && good.intent.targetId === "n1", "intent passthrough");
}

// non-finite deltas are omitted rather than coerced to 0/NaN
{
  const r = validateDialogueResponse({
    dialogue: "Cool.",
    suggestedRelationshipDelta: { trust: "high", respect: Number.NaN, affection: 3.6 },
  });
  const d = r?.suggestedRelationshipDelta;
  assert(d !== undefined, "delta block kept");
  assert(d!.trust === undefined, "string delta ignored");
  assert(d!.respect === undefined, "NaN delta ignored");
  assert(d!.affection === 4, "fractional delta rounded");
  assert(d!.conflict === undefined, "absent delta stays undefined");
}

// memory candidate: short summary dropped, importance clamped, unknown type -> OTHER
{
  const short = validateDialogueResponse({ dialogue: "Yo.", memoryCandidate: { summary: "hi", importance: 0.9 } });
  assert(short !== null && short.memoryCandidate === undefined, "summary < 3 chars dropped");

  const clamped = validateDialogueResponse({
    dialogue: "Yo.",
    memoryCandidate: { summary: "You paid my transport", importance: 7, type: "WEIRD" },
  });
  assert(clamped?.memoryCandidate?.importance === 1, "importance clamped to 1");
  assert(clamped?.memoryCandidate?.type === "OTHER", "unknown memory type -> OTHER");
}

// memory summary is trimmed and capped at 280 chars; importance defaults to 0.5
{
  const r = validateDialogueResponse({
    dialogue: "Yo.",
    memoryCandidate: { summary: "  " + "x".repeat(400) + "  ", type: "FAVOR" },
  });
  assert(r?.memoryCandidate?.summary.length === 280, "summary capped at 280");
  assert(r?.memoryCandidate?.summary.startsWith("x"), "summary trimmed");
  assert(r?.memoryCandidate?.importance === 0.5, "importance default 0.5");
}

// null / non-object input is rejected instead of throwing
{
  for (const raw of [null, undefined, 42, "text", []]) {
    assert(validateDialogueResponse(raw) === null || typeof raw === "object", "non-object rejected");
  }
  assert(validateDialogueResponse(null) === null, "null rejected");
  assert(validateDialogueResponse("dialogue") === null, "string rejected");
  assert(validateDialogueResponse(42) === null, "number rejected");
}
