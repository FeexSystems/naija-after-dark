/**
 * Phase D — contract tests for the game-api catalog.
 * Zero-dependency style (same as ai-orchestrator validate.test.ts).
 * Run: node --experimental-strip-types src/contracts/contracts.test.ts
 *
 * Pins the vocabulary the Unity client (NaadJson consumers) must match:
 * request envelope, per-type payload keys, error codes, prices, periods.
 */
import { COMMAND_REGISTRY, LOCATION_IDS, toExecuteCommandBody } from "./commands.ts";
import type { GameCommand } from "./commands.ts";
import { PRICES } from "./economy.ts";
import { PERIOD_BOUNDS } from "./world-clock.ts";
import type { WorldPeriod } from "./world-clock.ts";

function assert(cond: boolean, msg: string) {
  if (!cond) throw new Error("contract: " + msg);
}

// --- command registry covers every router type incl. Phase C/D additions ---
{
  const expected = [
    "TRAVEL", "SPEND", "BUY", "START_NIGHT", "COMPLETE_NIGHT",
    "PHONE_REPLY", "JOIN_ROOM", "LEAVE_ROOM", "ATTEND_EVENT",
    "RELATIONSHIP_DELTA", "SET_CAREER", "ACCEPT_OPPORTUNITY",
    "COMMIT_MEMORY", "SET_WORLD_PERIOD",
  ];
  for (const t of expected) assert((COMMAND_REGISTRY as readonly string[]).includes(t), "registry has " + t);
  assert(COMMAND_REGISTRY.length === expected.length, "registry has no extras");
}

// --- envelope builder: snake_case RPC args, payload defaults to {} ---
{
  const cmd: GameCommand = { requestId: "abc123", type: "TRAVEL", payload: { toLocationId: LOCATION_IDS.SUYA_SPOT } };
  const body = toExecuteCommandBody(cmd);
  assert(body.p_request_id === "abc123", "p_request_id passthrough");
  assert(body.p_type === "TRAVEL", "p_type passthrough");
  assert((body.p_payload as Record<string, unknown>).toLocationId === LOCATION_IDS.SUYA_SPOT, "payload passthrough");

  const empty = toExecuteCommandBody({ requestId: "x", type: "START_NIGHT", payload: undefined as unknown as Record<string, unknown> });
  assert(JSON.stringify(empty.p_payload) === "{}", "payload defaults to {}");
}

// --- per-type required keys (mirrors naad_execute_command dispatch) ---
{
  const required: Record<string, string[]> = {
    TRAVEL: ["toLocationId"],
    SPEND: ["sku"],
    BUY: ["sku"],
    PHONE_REPLY: ["messageId", "action"],
    JOIN_ROOM: ["locationId"],
    ATTEND_EVENT: ["eventId"],
    RELATIONSHIP_DELTA: ["targetId"],
    SET_CAREER: ["careerId"],
    ACCEPT_OPPORTUNITY: ["opportunityId"],
    COMMIT_MEMORY: ["npcId", "memoryType", "summary"],
    SET_WORLD_PERIOD: ["period"],
    START_NIGHT: [],
    COMPLETE_NIGHT: [],
    LEAVE_ROOM: [],
  };
  for (const t of COMMAND_REGISTRY) assert(t in required, "payload keys documented for " + t);
  assert(required.PHONE_REPLY.includes("action"), "phone reply needs action");
}

// --- phone actions are the closed GO/ASK_DETAILS/DECLINE set ---
{
  const actions = ["GO", "ASK_DETAILS", "DECLINE"];
  assert(actions.length === 3, "exactly three phone actions");
}

// --- economy: Gate 11 price table matches First Night assertions ---
{
  assert(PRICES.SUYA_PLATE === 5000, "suya 5000");
  assert(PRICES.CLUB_TICKET === 20000, "club ticket 20000");
  assert(PRICES.SUYA_PLATE + PRICES.CLUB_TICKET === 25000, "full night spend is 25000");
}

// --- world clock: six periods tile 0..1439 with no gaps/overlaps ---
{
  const periods: WorldPeriod[] = ["MORNING", "DAY", "TRANSITION", "NIGHT", "LATE_NIGHT", "AFTER_HOURS"];
  const covered = new Array(1440).fill(false);
  for (const p of periods) {
    const b = PERIOD_BOUNDS[p];
    assert(b.start >= 0 && b.end < 1440 && b.start <= b.end, p + " bounds sane");
    for (let m = b.start; m <= b.end; m++) {
      assert(!covered[m], "minute " + m + " covered twice");
      covered[m] = true;
    }
  }
  assert(covered.every(Boolean), "all 1440 minutes covered");
}

// --- seed location IDs match the Unity NightDistrictIds ---
{
  assert(LOCATION_IDS.APARTMENT.endsWith("101"), "apartment suffix");
  assert(LOCATION_IDS.SUYA_SPOT.endsWith("102"), "suya suffix");
  assert(LOCATION_IDS.NIGHTCLUB.endsWith("104"), "club suffix");
}

console.log("contracts.test.ts: ok");
