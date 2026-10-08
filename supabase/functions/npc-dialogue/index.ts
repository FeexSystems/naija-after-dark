/**
 * Edge Function: npc-dialogue (Gate 9)
 * Unity → JWT → this function → Gemini → validated JSON
 * Does NOT mutate the database.
 */

const SUPABASE_URL = Deno.env.get("SUPABASE_URL")!;
const SUPABASE_ANON_KEY = Deno.env.get("SUPABASE_ANON_KEY")!;
const GEMINI_API_KEY = Deno.env.get("GEMINI_API_KEY") ?? "";
const TUNDE_ID = "e1111111-1111-1111-1111-111111111101";

const SYSTEM_PROMPT = `You are Tunde, an event promoter in Lagos nightlife (NAIJA AFTER DARK).
Speak natural Nigerian English. Stay in character.
Never claim to change money or world time.
Return ONLY JSON: {"dialogue":"string","emotion":"neutral","intent":{"type":"NONE"},"memoryCandidate":{"summary":"...","importance":0.5,"type":"INTRO"}}`;

function validate(raw: unknown) {
  if (!raw || typeof raw !== "object") return null;
  const o = raw as Record<string, unknown>;
  if (typeof o.dialogue !== "string" || !o.dialogue.trim()) return null;
  const emotions = ["happy", "excited", "neutral", "angry", "sad", "nervous"];
  const emotion = typeof o.emotion === "string" && emotions.includes(o.emotion) ? o.emotion : "neutral";
  const out: Record<string, unknown> = {
    dialogue: String(o.dialogue).trim().slice(0, 500),
    emotion,
  };
  if (o.memoryCandidate && typeof o.memoryCandidate === "object") {
    const m = o.memoryCandidate as Record<string, unknown>;
    if (typeof m.summary === "string" && m.summary.trim().length >= 3) {
      out.memoryCandidate = {
        summary: m.summary.trim().slice(0, 280),
        importance: typeof m.importance === "number" ? Math.max(0, Math.min(1, m.importance)) : 0.5,
        type: typeof m.type === "string" ? m.type : "OTHER",
      };
    }
  }
  if (o.intent && typeof o.intent === "object") out.intent = o.intent;
  return out;
}

Deno.serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response(null, {
      headers: {
        "Access-Control-Allow-Origin": "*",
        "Access-Control-Allow-Headers": "authorization, content-type, apikey",
      },
    });
  }

  const accessToken = (req.headers.get("Authorization") ?? "").replace(/^Bearer\s+/i, "");
  if (!accessToken) {
    return Response.json({ success: false, errorCode: "UNAUTHENTICATED" }, { status: 401 });
  }

  let body: { requestId?: string; npcId?: string; playerMessage?: string };
  try {
    body = await req.json();
  } catch {
    return Response.json({ success: false, errorCode: "INVALID_JSON" }, { status: 400 });
  }

  const requestId = body.requestId ?? crypto.randomUUID();
  const npcId = body.npcId ?? "";
  const playerMessage = (body.playerMessage ?? "").trim().slice(0, 500);

  if (!npcId || !playerMessage) {
    return Response.json({ requestId, success: false, errorCode: "INVALID_REQUEST" }, { status: 400 });
  }
  if (npcId !== TUNDE_ID) {
    return Response.json({ requestId, success: false, errorCode: "NPC_NOT_AI" }, { status: 400 });
  }
  if (!GEMINI_API_KEY) {
    return Response.json({ requestId, success: false, errorCode: "MISSING_GEMINI_KEY" }, { status: 500 });
  }

  const ctxRes = await fetch(`${SUPABASE_URL}/rest/v1/rpc/naad_npc_dialogue_context`, {
    method: "POST",
    headers: {
      apikey: SUPABASE_ANON_KEY,
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ p_npc_id: npcId }),
  });
  if (!ctxRes.ok) {
    return Response.json({ requestId, success: false, errorCode: "CONTEXT_FAILED" }, { status: 400 });
  }
  const context = await ctxRes.json();

  const model = Deno.env.get("GEMINI_MODEL") ?? "gemini-3.8-flash";
  const gUrl = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${GEMINI_API_KEY}`;
  const gRes = await fetch(gUrl, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      contents: [{ role: "user", parts: [{ text: `${SYSTEM_PROMPT}\n\nINPUT:${JSON.stringify({ playerMessage, context })}` }] }],
      generationConfig: { temperature: 0.8, maxOutputTokens: 512, responseMimeType: "application/json" },
    }),
  });

  if (!gRes.ok) {
    return Response.json({ requestId, success: false, errorCode: "GEMINI_HTTP", errorMessage: String(gRes.status) }, { status: 502 });
  }

  const gData = await gRes.json();
  const rawText = gData?.candidates?.[0]?.content?.parts?.[0]?.text ?? "";
  let parsed: unknown;
  try {
    parsed = JSON.parse(rawText);
  } catch {
    const m = rawText.match(/\{[\s\S]*\}/);
    if (!m) return Response.json({ requestId, success: false, errorCode: "INVALID_JSON" }, { status: 502 });
    try {
      parsed = JSON.parse(m[0]);
    } catch {
      return Response.json({ requestId, success: false, errorCode: "INVALID_JSON" }, { status: 502 });
    }
  }

  const response = validate(parsed);
  if (!response) {
    return Response.json({ requestId, success: false, errorCode: "SCHEMA_VALIDATION_FAILED" }, { status: 502 });
  }

  return Response.json({
    requestId,
    success: true,
    response,
    memoryProposal: response.memoryCandidate ?? null,
  });
});
