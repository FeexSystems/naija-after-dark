# DOTS Unity 6 — When (Not) To Use It (NAAD decision record)

Source: Intro to DOTS (Unity 6 ed.), esp. Evaluating / Misconceptions / Netcode.

## What DOTS is (and isn't)
- DOTS ≠ ECS ≠ Entities ≠ data-oriented design. DOTS = Job System + Burst +
  Collections + Mathematics + Entities + optional Netcode/Physics/Graphics.
- Jobs/Burst usable WITHOUT Entities (just copy data in/out of unmanaged
  collections). Entities imposes project-wide structure — selective use is hard.
- Entities advantages beyond cache: cheaper create/destroy (no pooling needed
  en masse), unmanaged (no GC), Burst-accessible, en-masse queries. Costs:
  hierarchies/ordered relations are awkward; not optimal for everything.

## Use Entities when (guide's 4 reasons) — NAAD score
1. Many static elements (Megacity-scale env) — NO (compact blockouts, <100k tris).
2. Many dynamic heavy behaviors (100s of pathfinding units) — NO (dozens of NPCs).
3. Prefer ECS reasoning/maintainability — MAYBE later for crowds.
4. Competitive fast-action netcode w/ prediction — NO (turn-based social sim;
   authority = Supabase, not Netcode).
Verdict: DO NOT ADOPT Entities for NAAD vertical slice. Revisit only if crowds
exceed MonoBehaviour budgets AND profiling proves CPU-simulation bound (not GPU).

## Misconceptions that matter here
- "DOTS makes any game faster" — false; GPU-bound games gain nothing.
- "Every new project should use Entities" — false.
- "ECS benefits are just performance" — procedural style can aid reasoning, but
  cost is real; don't pay it without the problem.

## Netcode model (maps to NAAD trust model)
- Netcode for Entities = authoritative server + client prediction (server sims,
  clients send input, server snapshots back). Netcode for GameObjects = simpler,
  less anti-cheat, less scale.
- NAAD equivalent: **Supabase/game-api IS the authoritative server**; Unity is
  untrusted (ARCHITECTURE.md). Unity Netcode packages: NOT INSTALLED, NOT NEEDED.
  Gemini/ai-orchestrator = proposals only, never mutation path (matches guide's
  "authoritative server dictates what is happening").

## Applies to NAAD
Approved: C# Jobs + Burst for isolated hot loops. Forbidden this gate: Entities,
subscenes/baking, Entities Graphics/Physics, either Netcode package.
