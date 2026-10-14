# Scripting + Performance — GC, Burst/Jobs, Profiling (NAAD-applied)

Sources: Field Guide §§ Memory / Multithreading / Profiling; DOTS guide §§
About performance / Job System / Burst / Evaluating DOTS.

## GC (Boehm, non-compacting, stop-the-world)
- Value types (struct/int/float) copy cheap; reference types (class/string/array)
  live on heap via pointers. Prefer structs for hot data.
- GC spikes = allocator scanning gaps; non-compacting → fragmentation → heap
  expansion. Avoid per-frame `new` (esp. in `Update`), string concat, LINQ
  closures, boxing, `FindObjects`/`GetComponent` in loops. Cache refs in
  `Awake`; use `OverlapSphereNonAlloc` (as `InteractionSystem` does), pools.
- `ScriptableObject` values persist after Play mode — never treat them as
  scratch runtime state without reset.

## Burst + Jobs (use WITHOUT adopting full ECS)
- Burst-compiled jobs split work across cores; integrate into existing
  MonoBehaviour projects with only data-copy restructuring. Entities/ECS
  imposes whole-project structure — do NOT adopt for NAAD now.
- Rule: keep hot loops (crowd/seating/presence scans) job-able: unmanaged
  data, no managed refs inside job structs.

## Profiling workflow
- Profile on target device (Android mid-range), not just Editor Play mode.
- Markers: PlayerLoop/main thread, `Update/FixedUpdate/LateUpdate`, coroutines,
  rendering, physics, audio, memory. Fix in order: render (textures/mesh/shader
  cost, batching, culling, LOD) → physics (avoid complex mesh colliders) →
  script CPU.
- Mobile budget (per ENVIRONMENT_MANIFEST): <100k tris/scene; large buildings
  500–2000 tris; props 50–500; textures 256–2K + ASTC/ETC2; shared materials,
  atlasing, GPU instancing for repeats, occlusion culling, baked lighting.

## Applies to NAAD
- `InteractionSystem`: NonAlloc buffers, cached HUD, gizmo-bounded scan radius.
- Genesis→Supabase traffic stays in validated commands, never per-frame polls.
- DOTS decision: Jobs/Burst yes when needed; Entities/Netcode-for-Entities NO
  (authoritative server is Supabase/game-api, not Unity Netcode).
