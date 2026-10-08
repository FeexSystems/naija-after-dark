/**
 * NAAD AI Orchestrator core (Gate 9)
 * - Server-side only
 * - Gemini credentials never reach Unity
 * - Does NOT mutate authoritative state
 * - Returns validated dialogue + optional proposals
 */

import { validateDialogueResponse } from "./validate";
import type { DialogueRequest, NPCDialogueResponse } from "./types";
import { TUNDE_ID } from "./types";

const SYSTEM_PROMPT = `You are Tunde, an event promoter in Lagos nightlife (NAIJA AFTER DARK).
Personality: street-smart, warm, hustling, loyal once trust is earned.
Speak natural Nigerian English. Stay in character.
Never claim to change money, inventory, or world time.
Never invent past events not in provided memories.
Return ONLY valid JSON:
{"dialogue":"string","emotion":"happy|excited|neutral|angry|sad|nervous","intent":{"type":"NONE"},"suggestedRelationshipDelta":{},"memoryCandidate":{"summary":"...","importance":0.5,"type":"INTRO"}}
memoryCandidate only if something important and factual happened.`;

export interface OrchestratorDeps {
  geminiApiKey: string;
  geminiModel?: string;
  fetchContext: (npcId: string, accessToken: string) => Promise<unknown>;
  log?: (level: string, msg: string, meta?: Record<string, unknown>) => void;
}

export interface OrchestratorResult {
  requestId: string;
  success: boolean;
  errorCode?: string;
  errorMessage?: string;
  response?: NPCDialogueResponse;
  /** Proposal only — caller must run naad_commit_npc_memory if accepted */
  memoryProposal?: NPCDialogueResponse["memoryCandidate"];
  tokenEstimate?: number;
}

export async function runNpcDialogue(
  deps: OrchestratorDeps,
  accessToken: string,
  req: DialogueRequest
): Promise<OrchestratorResult> {
  const log = deps.log ?? (() => undefined);
  const started = Date.now();

  if (!req.requestId || !req.npcId || !req.playerMessage?.trim()) {
    return {
      requestId: req.requestId || "",
      success: false,
      errorCode: "INVALID_REQUEST",
      errorMessage: "requestId, npcId, playerMessage required",
    };
  }

  if (!deps.geminiApiKey) {
    return {
      requestId: req.requestId,
      success: false,
      errorCode: "MISSING_GEMINI_KEY",
      errorMessage: "GEMINI_API_KEY not configured on server",
    };
  }

  // Only Tunde is AI in Gate 9
  if (req.npcId !== TUNDE_ID) {
    return {
      requestId: req.requestId,
      success: false,
      errorCode: "NPC_NOT_AI",
      errorMessage: "Use deterministic dialogue for this NPC",
    };
  }

  let context: unknown;
  try {
    context = await deps.fetchContext(req.npcId, accessToken);
  } catch (e) {
    log("error", "context_fetch_failed", { error: String(e) });
    return {
      requestId: req.requestId,
      success: false,
      errorCode: "CONTEXT_FAILED",
      errorMessage: "Failed to load NPC context",
    };
  }

  const userPrompt = JSON.stringify({
    playerMessage: req.playerMessage.trim().slice(0, 500),
    context,
  });

  const model = deps.geminiModel || "gemini-2.0-flash";
  const url = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${deps.geminiApiKey}`;

  const body = {
    contents: [
      {
        role: "user",
        parts: [{ text: `${SYSTEM_PROMPT}\n\nINPUT:\n${userPrompt}` }],
      },
    ],
    generationConfig: {
      temperature: 0.8,
      maxOutputTokens: 512,
      responseMimeType: "application/json",
    },
  };

  let rawText = "";
  try {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 15000);
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      signal: controller.signal,
    });
    clearTimeout(timeout);

    if (!res.ok) {
      const errBody = await res.text();
      log("error", "gemini_http_error", { status: res.status, body: errBody.slice(0, 300) });
      return {
        requestId: req.requestId,
        success: false,
        errorCode: "GEMINI_HTTP",
        errorMessage: `Gemini HTTP ${res.status}`,
      };
    }

    const data = (await res.json()) as {
      candidates?: Array<{ content?: { parts?: Array<{ text?: string }> } }>;
    };
    rawText = data.candidates?.[0]?.content?.parts?.[0]?.text ?? "";
  } catch (e) {
    log("error", "gemini_fetch_failed", { error: String(e) });
    return {
      requestId: req.requestId,
      success: false,
      errorCode: "GEMINI_TIMEOUT_OR_NETWORK",
      errorMessage: String(e),
    };
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(rawText);
  } catch {
    // try extract JSON object
    const match = rawText.match(/\{[\s\S]*\}/);
    if (!match) {
      return {
        requestId: req.requestId,
        success: false,
        errorCode: "INVALID_JSON",
        errorMessage: "Gemini returned non-JSON",
      };
    }
    try {
      parsed = JSON.parse(match[0]);
    } catch {
      return {
        requestId: req.requestId,
        success: false,
        errorCode: "INVALID_JSON",
        errorMessage: "Could not parse Gemini JSON",
      };
    }
  }

  const validated = validateDialogueResponse(parsed);
  if (!validated) {
    return {
      requestId: req.requestId,
      success: false,
      errorCode: "SCHEMA_VALIDATION_FAILED",
      errorMessage: "Response failed schema validation",
    };
  }

  log("info", "dialogue_ok", {
    requestId: req.requestId,
    ms: Date.now() - started,
    hasMemory: Boolean(validated.memoryCandidate),
  });

  return {
    requestId: req.requestId,
    success: true,
    response: validated,
    memoryProposal: validated.memoryCandidate,
    tokenEstimate: Math.ceil((SYSTEM_PROMPT.length + userPrompt.length + rawText.length) / 4),
  };
}
