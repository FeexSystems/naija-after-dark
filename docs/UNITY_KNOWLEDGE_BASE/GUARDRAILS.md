# GUARDRAILS — Enforceable Rules for Agents (READ FIRST)

Violating any MUST is a failed task. Cite the rule ID in your summary.

## G1. Gate discipline (MUST)
- Implement ONLY the current gate. No skipping, no drive-by refactors.
- No gate "passes" without its headless verification succeeding
  (`ValidateScenes` / `Verify*` Batchmode PASS in log).

## G2. Authoritative world model (MUST)
- Unity NEVER mutates money/inventory/reputation/progression. Client sends
  `command + requestId`; game-api validates; Postgres constrains; domain event
  returns. Service-role keys + Gemini creds NEVER in Unity or repo.
- Gemini/ai-orchestrator proposes; never on mutation path (ARCHITECTURE.md).

## G3. Prefab-first, vendor-optional (MUST)
- `TryInstantiate` + `CreateBox` fallback: scenes MUST build headless with
  vendor packs absent. Never hard-require `Assets/<Publisher>/` paths.
- Placeholder tags MUST be runtime `NAAD.World.NaAdPlaceholderTag`
  (GUID 38f62356be014c1f9cecd3c75a2557e3). NEVER Editor-assembly scripts on
  prefabs (breaks YAML: `m_Script:{fileID:0}`).

## G4. Namespace + API traps (MUST)
- Inside `NAAD.Gameplay.Input`, ALWAYS write `UnityEngine.Input.*`
  (bare `Input` resolves to the namespace → CS0234). Same for any `Input`
  namespace shadowing.
- `NaadPlaceholderTag.AssetId/Description` need setters (author sets them).
- Batchmode method names are case-sensitive:
  `NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders` (note `Naad`, not `NaAd`).

## G5. Render pipeline freeze (MUST)
- Project is Built-in RP. Shader lookup: `URP/Lit ?? Standard`. Do NOT
  mass-convert materials/scenes to URP this gate.

## G6. Naming + serialization (MUST)
- Asset names: `[Category]_[Subcat]_[Variant]_[Detail]`
  (ASSET_NAMING_CONVENTION.md). No spaces, no special chars but `_`.
- Force Text YAML; every asset + `.meta` committed together; never commit
  `Library/ Temp/ obj/ Build/ Logs/` or `*.log`; vendor `Assets/<Publisher>/`
  stays gitignored.

## G7. Scene contracts (MUST)
- Only load registered scenes (UNITY_SCENE_REGISTRY.md; index 0 = Bootstrap).
- Every production scene: PlayerSpawn + ExitTravel anchors + `MainCamera` tag
  (ValidateScenes enforces).

## G8. Performance + UI (SHOULD)
- No per-frame `new`/LINQ/string-concat in hot paths; NonAlloc + pooling;
  cache components; mobile budget <100k tris, ASTC/ETC2, shared mats.
- Runtime HUD = uGUI (`MainHudController` prompt contract, SafeArea, 44dp);
  IMGUI never ships; UI Toolkit only for future Editor tooling/complex HUD.

## G9. Forbidden this gate (MUST NOT)
Entities/ECS, subscenes/baking, Entities Graphics/Physics, Netcode packages
(either), UI Toolkit runtime migration, URP conversion, new Unity version.

## G10. Evidence (MUST)
Summary MUST list: files changed, Batchmode commands run, log PASS lines
(`error CS:0`, `Saved … (6/6)`, `Validation passed`), `git status` scope,
remaining risks. No log, no pass.
