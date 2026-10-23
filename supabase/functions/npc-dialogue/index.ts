/**
 * Edge Function: npc-dialogue (Gate 9 / Phase C)
 *
 * Unity → JWT → this function → Postgres context RPC → Gemini → validated JSON
 *
 * Contract rules (see docs/GATES_7_10.md, docs/NEXT_ARCHITECTURE.md):
 * - Gemini NEVER writes the database. Gemini output is a *proposal* only.
 * - Every persistence / economy change must travel through the Phase A router
 *   `naad_execute_command` (request_id + command_receipts idempotency).
 * - Response envelope stays Unity-compatible: top-level "success" boolean plus
 *   a nested "response" object holding "dialogue" / "emotion" / "memoryCandidate"
 *   (see game/unity/Assets/NAAD/Networking/Supabase/SupabaseDialogueService.cs).
 * - Output is validated against ai/schemas/npc-dialogue-response.schema.json.
 *
 * Env (Supabase secrets):
 * - GEMINI_API_KEY                (required)
 * - GEMINI_MODEL                  (optional, defaults to gemini-3.8-flash)
 * - NPC_DIALOGUE_APPLY_PROPOSALS  (optional, "true" lets the server execute the
 *                                  validated COMMIT_MEMORY / RELATIONSHIP_DELTA
 *                                  proposals through naad_execute_command;
 *                                  default "false" keeps this proposal-only)
 */

const SUPABASE_URL = (Deno.env.get("SUPABASE_URL") ?? "").replace(/\/+$/, "");
const SUPABASE_ANON_KEY = Deno.env.get("SUPABASE_ANON_KEY") ?? "";
const GEMINI_API_KEY = Deno.env.get("GEMINI_API_KEY") ?? "";
const GEMINI_MODEL = Deno.env.get("GEMINI_MODEL") ?? "gemini-3.8-flash";
const APPLY_PROPOSALS =
  (Deno.env.get("NPC_DIALOGUE_APPLY_PROPOSALS") ?? "false").toLowerCase() === "true";

/** Gate 9: Tunde is the only AI-driven NPC. */
const TUNDE_ID = "e1111111-1111-1111-1111-111111111101";

const MAX_PLAYER_MESSAGE = 500;
const MAX_DIALOGUE = 500;
const MAX_SUMMARY = 280;
const MAX_REQUEST_ID = 128;
const MAX_CONTEXT_CHARS = 4000;
const MAX_MEMORY_CONTEXT = 5;
const GEMINI_TIMEOUT_MS = 15_000;
const GEMINI_MAX_OUTPUT_TOKENS = 512;
/** Below this importance a memory candidate is treated as noise (Phase C). */
const MEMORY_IMPORTANCE_THRESHOLD = 0.6;
/** Hard ceiling on the number of commands a single turn may execute. */
const MAX_COMMANDS_PER_TURN = 2;

const SYSTEM_PROMPT = `You are Tunde, an event promoter in Lagos nightlife (NAIJA AFTER DARK).
Personality: street-smart, warm, hustling, loyal once trust is earned.
Speak natural Nigerian English (not exaggerated pidgin caricature).
Stay in character. Never claim to change money, inventory, or world time.
Never invent past events that are not in the provided memories.
Return ONLY valid JSON matching this shape:
{
  "dialogue": "string",
  "emotion": "happy|excited|neutral|angry|sad|nervous",
  "intent": { "type": "INVITE_EVENT|OFFER_JOB|REQUEST_PAYMENT|MAKE_PLAN|REFUSE|NONE", "targetId": "optional" },
  "suggestedRelationshipDelta": { "trust": 0, "respect": 0, "affection": 0, "conflict": 0 },
  "memoryCandidate": { "summary": "short factual summary", "importance": 0.0, "type": "INTRO" }
}
memoryCandidate is optional — only include if something important and factual happened.
suggestedRelationshipDelta values must be integers from -10 to 10.
Do not mutate any database. You only propose dialogue and optional deltas.`;

const CORS_HEADERS: Record<string, string> = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, content-type, apikey",
  "Access-Control-Allow-Methods": "POST, OPTIONS",
};

const EMOTIONS = ["happy", "excited", "neutral", "angry", "sad", "nervous"] as const;
const INTENT_TYPES = [
  "INVITE_EVENT",
  "OFFER_JOB",
  "REQUEST_PAYMENT",
  "MAKE_PLAN",
  "REFUSE",
  "NONE",
] as const;
const MEMORY_TYPES = [
  "FAVOR",
  "INSULT",
  "SHARED_EVENT",
  "PROMISE",
  "TRANSACTION",
  "INTRO",
  "OTHER",
] as const;
const DELTA_KEYS = ["trust", "respect", "affection", "conflict"] as const;

type Emotion = (typeof EMOTIONS)[number];
type IntentType = (typeof INTENT_TYPES)[number];
type MemoryType = (typeof MEMORY_TYPES)[number];
type DeltaKey = (typeof DELTA_KEYS)[number];

interface DialogueIntent {
  type: IntentType;
  targetId?: string;
}

interface RelationshipDelta {
  trust?: number;
  respect?: number;
  affection?: number;
  conflict?: number;
}

interface MemoryCandidate {
  summary: string;
  importance: number;
  type: MemoryType;
}

interface DialogueResponse {
  dialogue: string;
  emotion: Emotion;
  intent?: DialogueIntent;
  suggestedRelationshipDelta?: RelationshipDelta;
  memoryCandidate?: MemoryCandidate;
}

/** A Phase A command proposal — never executed without validation. */
interface CommandProposal {
  requestId: string;
  type: string;
  payload: Record<string, unknown>;
}

/**
 * Validate Gemini JSON against ai/schemas/npc-dialogue-response.schema.json.
 * Returns null when the payload is unusable; never mutates state.
 */
function validateDialogueResponse(raw: unknown): DialogueResponse | null {
  if (!isRecord(raw)) return null;

  if (typeof raw.dialogue !== "string") return null;
  const dialogue = raw.dialogue.trim();
  if (dialogue.length === 0) return null;

  const result: DialogueResponse = {
    dialogue: dialogue.slice(0, MAX_DIALOGUE),
    emotion: isEmotion(raw.emotion) ? raw.emotion : "neutral",
  };

  if (isRecord(raw.intent) && isIntentType(raw.intent.type)) {
    const intent: DialogueIntent = { type: raw.intent.type };
    if (typeof raw.intent.targetId === "string" && raw.intent.targetId.trim().length > 0) {
      intent.targetId = raw.intent.targetId.trim().slice(0, MAX_REQUEST_ID);
    }
    result.intent = intent;
  }

  if (isRecord(raw.suggestedRelationshipDelta)) {
    const delta: RelationshipDelta = {};
    for (const key of DELTA_KEYS) {
      const clamped = clampDelta(raw.suggestedRelationshipDelta[key]);
      if (clamped !== undefined) delta[key] = clamped;
    }
    if (Object.keys(delta).length > 0) result.suggestedRelationshipDelta = delta;
  }

  if (isRecord(raw.memoryCandidate)) {
    const m = raw.memoryCandidate;
    if (typeof m.summary === "string" && m.summary.trim().length >= 3) {
      const importance = typeof m.importance === "number" && Number.isFinite(m.importance)
        ? Math.max(0, Math.min(1, m.importance))
        : 0.5;
      result.memoryCandidate = {
        summary: m.summary.trim().slice(0, MAX_SUMMARY),
        importance,
        type: isMemoryType(m.type) ? m.type : "OTHER",
      };
    }
  }

  return result;
}

function jsonResponse(
  body: Record<string, unknown>,
  status = 200,
): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { ...CORS_HEADERS, "Content-Type": "application/json" },
  });
}

function fail(
  requestId: string,
  errorCode: string,
  status: number,
  errorMessage?: string,
): Response {
  return jsonResponse(
    {
      requestId,
      success: false,
      errorCode,
      ...(errorMessage ? { errorMessage } : {}),
    },
    status,
  );
}

function sanitizeRequestId(value: unknown): string {
  if (typeof value !== "string") return crypto.randomUUID();
  const trimmed = value.trim();
  if (trimmed.length === 0) return crypto.randomUUID();
  return trimmed.slice(0, MAX_REQUEST_ID);
}

/** Strip anything that is not a printable, non-control character. */
function sanitizeText(value: unknown, maxLength: number): string {
  if (typeof value !== "string") return "";
  // eslint-disable-next-line no-control-regex
  return value.replace(/[\u0000-\u001f\u007f]/g, " ").trim().slice(0, maxLength);
}

async function supabaseRpc(
  accessToken: string,
  rpcName: string,
  args: Record<string, unknown>,
): Promise<{ ok: boolean; status: number; data: unknown }> {
  const res = await fetch(`${SUPABASE_URL}/rest/v1/rpc/${rpcName}`, {
    method: "POST",
    headers: {
      apikey: SUPABASE_ANON_KEY,
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(args),
  });
  let data: unknown = null;
  try {
    data = await res.json();
  } catch {
    data = null;
  }
  return { ok: res.ok, status: res.status, data };
}

/**
 * The bounded context pack handed to Gemini. Every field is optional so the
 * pack can be shrunk field-by-field without breaking the shape.
 */
interface ContextPack {
  npc: {
    id: string;
    name: string;
    archetype?: string;
    occupation?: string;
    personality?: string;
    goal?: string;
    mood?: number;
  };
  relationship: {
    trust: number;
    respect: number;
    affection: number;
    loyalty: number;
    conflict: number;
  };
  world: {
    gameDate: string;
    minutesSinceMidnight: number;
    period: string;
    weather: string;
    nightlifeLevel: number;
  };
  location: {
    name: string;
    type: string;
  };
  memories: Array<{
    summary: string;
    type: string;
    importance: number;
  }>;
}

/**
 * Build a bounded context pack for Gemini from the `naad_npc_dialogue_context`
 * payload. Only the fields Tunde needs are forwarded, and the total prompt
 * payload is hard-capped so a large world cannot blow the token budget.
 */
function buildContextPack(raw: unknown): ContextPack {
  const ctx = isRecord(raw) ? raw : {};
  const npc = isRecord(ctx.npc) ? ctx.npc : {};
  const rel = isRecord(ctx.relationship) ? ctx.relationship : {};
  const world = isRecord(ctx.world) ? ctx.world : {};
  const location = isRecord(ctx.location) ? ctx.location : {};

  const memories: unknown[] = Array.isArray(ctx.memories) ? ctx.memories : [];

  const pack: ContextPack = {
    npc: {
      id: typeof npc.id === "string" ? npc.id : TUNDE_ID,
      name: sanitizeText(npc.name, 64),
      archetype: sanitizeText(npc.archetype, 48),
      occupation: sanitizeText(npc.occupation, 64),
      personality: sanitizeText(npc.personality, 280),
      goal: sanitizeText(npc.goal, 140),
      mood: numberOr(npc.mood, 50),
    },
    relationship: {
      trust: numberOr(rel.trust, 50),
      respect: numberOr(rel.respect, 50),
      affection: numberOr(rel.affection, 0),
      loyalty: numberOr(rel.loyalty, 0),
      conflict: numberOr(rel.conflict, 0),
    },
    world: {
      gameDate: sanitizeText(world.gameDate, 16),
      minutesSinceMidnight: numberOr(world.minutesSinceMidnight, 0),
      period: sanitizeText(world.period, 16),
      weather: sanitizeText(world.weather, 32),
      nightlifeLevel: numberOr(world.nightlifeLevel, 0),
    },
    location: {
      name: sanitizeText(location.name, 64),
      type: sanitizeText(location.type, 48),
    },
    memories: memories.slice(0, MAX_MEMORY_CONTEXT).map((m) => {
      const mem = isRecord(m) ? m : {};
      return {
        summary: sanitizeText(mem.summary, MAX_SUMMARY),
        type: sanitizeText(mem.memory_type, 32),
        importance: numberOr(mem.importance, 0.5),
      };
    }),
  };

  // Hard cap the serialized pack so Gemini input stays bounded.
  // Shrink by dropping memories first, then fall back to a minimal pack.
  // Never slice a JSON string mid-token (that would produce invalid JSON).
  let candidate: ContextPack = pack;
  if (JSON.stringify(candidate).length > MAX_CONTEXT_CHARS) {
    candidate = { ...pack, memories: [] };
  }
  if (JSON.stringify(candidate).length > MAX_CONTEXT_CHARS) {
    candidate = {
      npc: { id: TUNDE_ID, name: pack.npc.name },
      relationship: pack.relationship,
      world: pack.world,
      location: pack.location,
      memories: [],
    };
  }
  if (JSON.stringify(candidate).length > MAX_CONTEXT_CHARS) {
    candidate = {
      npc: { id: TUNDE_ID, name: pack.npc.name },
      relationship: pack.relationship,
      world: pack.world,
      memories: [],
      location: { name: "", type: "" },
    };
  }
  return candidate;
}

function numberOr(value: unknown, fallback: number): number {
  if (typeof value === "number" && Number.isFinite(value)) return value;
  if (typeof value === "string") {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) return parsed;
  }
  return fallback;
}

interface GeminiOutcome {
  ok: boolean;
  errorCode?: string;
  errorMessage?: string;
  parsed?: unknown;
}

/** Extract the first balanced JSON object from a Gemini text part. */
function extractJson(rawText: string): unknown {
  const text = rawText.trim();
  if (text.length === 0) return null;

  const direct = tryParseJson(text);
  if (direct !== undefined) return direct;

  // Gemini occasionally wraps JSON in prose or ```json fences.
  const fenced = text.match(/```(?:json)?\s*([\s\S]*?)```/i);
  if (fenced?.[1]) {
    const parsedFence = tryParseJson(fenced[1].trim());
    if (parsedFence !== undefined) return parsedFence;
  }

  const start = text.indexOf("{");
  const end = text.lastIndexOf("}");
  if (start >= 0 && end > start) {
    const parsedSlice = tryParseJson(text.slice(start, end + 1));
    if (parsedSlice !== undefined) return parsedSlice;
  }
  return null;
}

function tryParseJson(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}

async function callGemini(
  playerMessage: string,
  contextPack: ContextPack,
): Promise<GeminiOutcome> {
  const url =
    `https://generativelanguage.googleapis.com/v1beta/models/${encodeURIComponent(GEMINI_MODEL)}:generateContent?key=${encodeURIComponent(GEMINI_API_KEY)}`;

  const body = {
    systemInstruction: { parts: [{ text: SYSTEM_PROMPT }] },
    contents: [
      {
        role: "user",
        parts: [
          {
            text: JSON.stringify({
              playerMessage,
              context: contextPack,
            }),
          },
        ],
      },
    ],
    generationConfig: {
      temperature: 0.8,
      maxOutputTokens: GEMINI_MAX_OUTPUT_TOKENS,
      responseMimeType: "application/json",
    },
  };

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), GEMINI_TIMEOUT_MS);
  try {
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      signal: controller.signal,
    });

    if (!res.ok) {
      return {
        ok: false,
        errorCode: "GEMINI_HTTP",
        errorMessage: `status ${res.status}`,
      };
    }

    const data = await res.json();
    const parts = data?.candidates?.[0]?.content?.parts;
    const rawText = Array.isArray(parts) && parts.length > 0 &&
        typeof parts[0]?.text === "string"
      ? parts[0].text
      : "";
    if (rawText.trim().length === 0) {
      return { ok: false, errorCode: "EMPTY_GEMINI_RESPONSE" };
    }

    const parsed = extractJson(rawText);
    if (parsed === null || parsed === undefined) {
      return { ok: false, errorCode: "INVALID_JSON" };
    }
    return { ok: true, parsed };
  } catch (error) {
    const isAbort =
      (error instanceof DOMException && error.name === "AbortError") ||
      (error instanceof Error && error.name === "AbortError");
    if (isAbort) {
      return { ok: false, errorCode: "GEMINI_TIMEOUT" };
    }
    return {
      ok: false,
      errorCode: "GEMINI_REQUEST_FAILED",
      errorMessage: error instanceof Error ? error.message : "unknown error",
    };
  } finally {
    clearTimeout(timer);
  }
}

interface CommandResult {
  requestId: string;
  type: string;
  success: boolean;
  errorCode?: string;
  errorMessage?: string;
}

/**
 * Translate a validated Gemini proposal into Phase A router commands.
 *
 * Only two command types are reachable from dialogue, and both are state
 * changes that MUST travel through `naad_execute_command`:
 *  - COMMIT_MEMORY      (npcId, memoryType, summary, importance)
 *  - RELATIONSHIP_DELTA (targetId, trust/respect/affection/conflict, reason)
 *
 * Every other intent (INVITE_EVENT, OFFER_JOB, REQUEST_PAYMENT, MAKE_PLAN,
 * REFUSE, NONE) is conversation-only: it never maps to a mutation, because
 * the Phase A router has no command type that can safely express them.
 */
function buildCommandProposals(
  requestId: string,
  npcId: string,
  response: DialogueResponse,
): CommandProposal[] {
  const proposals: CommandProposal[] = [];

  const memory = response.memoryCandidate;
  if (
    memory &&
    memory.summary.trim().length >= 3 &&
    memory.importance >= MEMORY_IMPORTANCE_THRESHOLD
  ) {
    proposals.push({
      requestId: `${requestId}:memory`,
      type: "COMMIT_MEMORY",
      payload: {
        npcId,
        memoryType: memory.type,
        summary: memory.summary,
        importance: memory.importance,
      },
    });
  }

  const delta = response.suggestedRelationshipDelta;
  if (delta) {
    const hasDelta = DELTA_KEYS.some((key) => (delta[key] ?? 0) !== 0);
    if (hasDelta) {
      const payload: Record<string, unknown> = {
        targetId: npcId,
        reason: "npc-dialogue",
      };
      for (const key of DELTA_KEYS) {
        if (typeof delta[key] === "number") payload[key] = delta[key];
      }
      proposals.push({
        requestId: `${requestId}:relationship`,
        type: "RELATIONSHIP_DELTA",
        payload,
      });
    }
  }

  return proposals.slice(0, MAX_COMMANDS_PER_TURN);
}

/**
 * Execute a proposal through the guarded Phase A router.
 * The router owns validation, authorization and idempotency (command_receipts).
 */
async function executeCommand(
  accessToken: string,
  proposal: CommandProposal,
): Promise<CommandResult> {
  const result = await supabaseRpc(accessToken, "naad_execute_command", {
    p_request_id: proposal.requestId,
    p_type: proposal.type,
    p_payload: proposal.payload,
  });

  if (!result.ok) {
    return {
      requestId: proposal.requestId,
      type: proposal.type,
      success: false,
      errorCode: "COMMAND_ROUTER_ERROR",
      errorMessage: `status ${result.status}`,
    };
  }

  const data = isRecord(result.data) ? result.data : {};
  const success = data.success === true;
  return {
    requestId: proposal.requestId,
    type: proposal.type,
    success,
    errorCode: typeof data.errorCode === "string" ? data.errorCode : undefined,
    errorMessage: typeof data.errorMessage === "string"
      ? data.errorMessage
      : undefined,
  };
}

/**
 * Extract the bearer token from the Authorization header.
 * Returns null when the header is missing or malformed.
 */
function extractAccessToken(req: Request): string | null {
  const header = req.headers.get("Authorization") ?? "";
  const match = header.match(/^Bearer\s+(.+)$/i);
  const token = match?.[1]?.trim();
  if (!token || token.length === 0) return null;
  return token;
}

/**
 * Verify the JWT against Supabase Auth so we never trust the caller-supplied
 * identity. Returns the auth user id, or null when the token is not valid.
 */
async function resolveUserId(accessToken: string): Promise<string | null> {
  try {
    const res = await fetch(`${SUPABASE_URL}/auth/v1/user`, {
      headers: {
        apikey: SUPABASE_ANON_KEY,
        Authorization: `Bearer ${accessToken}`,
      },
    });
    if (!res.ok) return null;
    const user = await res.json();
    const id = isRecord(user) ? user.id : undefined;
    return isUuid(id) ? id : null;
  } catch {
    return null;
  }
}

interface ParsedRequest {
  requestId: string;
  npcId: string;
  playerMessage: string;
}

/**
 * Validate the incoming body. Rejects anything that is not a well-formed
 * request before a single network call is made.
 */
function parseRequestBody(body: unknown): ParsedRequest | { errorCode: string } {
  if (!isRecord(body)) return { errorCode: "INVALID_REQUEST" };

  const requestId = sanitizeRequestId(body.requestId);
  const npcId = typeof body.npcId === "string" ? body.npcId.trim() : "";
  const playerMessage = sanitizeText(body.playerMessage, MAX_PLAYER_MESSAGE);

  if (!isUuid(npcId)) return { errorCode: "INVALID_REQUEST" };
  if (playerMessage.length === 0) return { errorCode: "INVALID_REQUEST" };

  return { requestId, npcId, playerMessage };
}

/**
 * Assemble the Unity-facing envelope.
 *
 * Unity (SupabaseDialogueService.TalkToTundeAsync) reads:
 *   - top-level "success"                -> NaadJson.ReadSuccess
 *   - "response.dialogue"                -> Dialogue
 *   - "response.emotion"                 -> Emotion
 *   - "response.memoryCandidate.summary" -> MemorySummary
 *   - "response.memoryCandidate.type"    -> MemoryType
 *
 * Extra keys are additive and ignored by the current client.
 */
function buildSuccessEnvelope(
  requestId: string,
  response: DialogueResponse,
  commandResults: CommandResult[],
): Record<string, unknown> {
  return {
    requestId,
    success: true,
    response,
    // Kept for the pre-Phase-C client shape; identical to response.memoryCandidate.
    memoryProposal: response.memoryCandidate ?? null,
    // Only populated when NPC_DIALOGUE_APPLY_PROPOSALS=true.
    appliedCommands: commandResults,
  };
}

Deno.serve(async (req: Request): Promise<Response> => {
  if (req.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: CORS_HEADERS });
  }
  if (req.method !== "POST") {
    return fail("unknown", "METHOD_NOT_ALLOWED", 405);
  }

  // --- 1. Authentication ------------------------------------------------
  const accessToken = extractAccessToken(req);
  if (!accessToken) {
    return fail("unknown", "UNAUTHENTICATED", 401);
  }

  const userId = await resolveUserId(accessToken);
  if (!userId) {
    return fail("unknown", "INVALID_TOKEN", 401);
  }

  // --- 2. Request validation --------------------------------------------
  let rawBody: unknown;
  try {
    rawBody = await req.json();
  } catch {
    return fail("unknown", "INVALID_JSON", 400);
  }

  const parsed = parseRequestBody(rawBody);
  if ("errorCode" in parsed) {
    return fail("unknown", parsed.errorCode, 400);
  }
  const { requestId, npcId, playerMessage } = parsed;

  if (npcId !== TUNDE_ID) {
    return fail(requestId, "NPC_NOT_AI", 400);
  }
  if (!GEMINI_API_KEY) {
    return fail(requestId, "MISSING_GEMINI_KEY", 500);
  }

  // --- 3. Authenticated context load (read-only RPC) --------------------
  const ctxResult = await supabaseRpc(accessToken, "naad_npc_dialogue_context", {
    p_npc_id: npcId,
  });
  if (!ctxResult.ok) {
    return fail(requestId, "CONTEXT_FAILED", 502);
  }
  const ctxData = isRecord(ctxResult.data) ? ctxResult.data : {};
  if (ctxData.success !== true) {
    const code = typeof ctxData.errorCode === "string"
      ? ctxData.errorCode
      : "CONTEXT_FAILED";
    return fail(requestId, code, code === "NPC_NOT_FOUND" ? 404 : 502);
  }
  // Defence in depth: the RPC runs as auth.uid(); make sure it matches the
  // identity we resolved from the Authorization header.
  const ctxPlayerId = typeof ctxData.playerId === "string" ? ctxData.playerId : null;
  if (ctxPlayerId && ctxPlayerId !== userId) {
    return fail(requestId, "CONTEXT_MISMATCH", 403);
  }

  const contextPack = buildContextPack(ctxData);

  // --- 4. Bounded Gemini generation -------------------------------------
  const gemini = await callGemini(playerMessage, contextPack);
  if (!gemini.ok) {
    const status = gemini.errorCode === "GEMINI_TIMEOUT" ? 504 : 502;
    return fail(requestId, gemini.errorCode ?? "GEMINI_FAILED", status, gemini.errorMessage);
  }

  // --- 5. Strict output validation --------------------------------------
  const validated = validateDialogueResponse(gemini.parsed);
  if (!validated) {
    return fail(requestId, "SCHEMA_VALIDATION_FAILED", 502);
  }

  // --- 6. Optional guarded proposal application --------------------------
  const commandResults: CommandResult[] = [];
  if (APPLY_PROPOSALS) {
    const proposals = buildCommandProposals(requestId, npcId, validated);
    for (const proposal of proposals) {
      commandResults.push(await executeCommand(accessToken, proposal));
    }
  }

  return jsonResponse(buildSuccessEnvelope(requestId, validated, commandResults), 200);
});

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function clampDelta(n: unknown): number | undefined {
  if (typeof n !== "number" || !Number.isFinite(n)) return undefined;
  return Math.max(-10, Math.min(10, Math.round(n)));
}

function isEmotion(v: unknown): v is Emotion {
  return typeof v === "string" && (EMOTIONS as readonly string[]).includes(v);
}

function isIntentType(v: unknown): v is IntentType {
  return typeof v === "string" && (INTENT_TYPES as readonly string[]).includes(v);
}

function isMemoryType(v: unknown): v is MemoryType {
  return typeof v === "string" && (MEMORY_TYPES as readonly string[]).includes(v);
}

/** UUID shape only — anything else is rejected before it reaches Postgres. */
const UUID_RE =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function isUuid(v: unknown): v is string {
  return typeof v === "string" && UUID_RE.test(v);
}

