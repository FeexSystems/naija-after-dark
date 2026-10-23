# Unity External Assets Audit & Reuse Plan

## NAAD (Naija After Dark) — NPC Dialogue System

**Date**: 2026-10-22  
**Scope**: `game/unity/Assets/`  
**Goal**: Identify assets useful for the NPC dialogue system and plan extraction/move/reuse.

## 1. EXECUTIVE SUMMARY

| Category | Assets | Relevance | License Risk | Action |
|----------|--------|-----------|--------------|--------|
| Core project code | NAAD/ | Critical — already integrated | Proprietary | Keep in place |
| AI Dev Kit | Glitch9/AIDevKit/ | High — provider-agnostic LLM client, MCP | **AGPL-3.0-or-later** | Study only; do NOT merge |
| LLM for Unity | LLMUnity/ | High — local LLM, RAG, LLMCharacter | Apache 2.0 (permissive) | Safe to reuse |
| MCP plugins | Plugins/NuGet/ | Medium — McpPlugin bridge | Mixed (NuGet) | Keep as-is |
| 3D models | NAAD/Content/Models/ | Critical — 57 verified assets | Project-owned | Keep in place |
| Networking | NAAD/Networking/ | Critical — Supabase dialogue service | Project-owned | Keep in place |
| UI framework | NAAD/UI/ | High — dialogue UI needs TextMesh Pro | Project-owned | Keep in place |
| Text rendering | TextMesh Pro/ | High — dialogue/subtitle UI | Unity (built-in) | Keep as-is |
| Environments | PalmTreePack, Versatile Studio, Brick Project Studio | Low — environment dressing | Unknown | Review individually |
| Characters | Kevin Iglesias, Marcos Schultz | High — humanoid rigs, animation, camera/controllers | N/A | Configure Avatar; integrate rigs |

## 2. DETAILED ASSET INVENTORY

### 2.1 NAAD/ — Project-Owned Code (Critical, Keep In-Place)

| Path | Contents | Relevance |
|------|----------|-----------|
| `NAAD/Networking/Supabase/` | `SupabaseDialogueService.cs`, `NaadJson.cs`, `SupabaseAuthService.cs`, `SupabaseCommandService.cs`, `SupabaseHttp.cs`, `SupabaseConfig.cs` | **Critical** — Unity client for the Edge Function. `TalkToTundeAsync()` calls `/functions/v1/npc-dialogue`. `CommitMemoryAsync()` calls `naad_commit_npc_memory`. |
| `NAAD/Networking/Interfaces/` | `IDialogueService.cs`, `ICommandService.cs`, `IAuthService.cs`, etc. | **Critical** — Service contracts. `DialogueResult` has `MemorySummary`, `MemoryImportance`, `MemoryType` matching Edge Function output. |
| `NAAD/Content/Models/` | 57 verified assets (FBX + GLB), `naad_asset_manifest.json`, `naad_verify_report.json` | **Critical** — NPC characters (`CHR_*` humanoid), environments, props, vehicles. All 57 passed verification. |
| `NAAD/UI/` | `MainHudController.cs`, `DataContracts.cs`, screen views | High — dialogue UI integration point |
| `NAAD/Characters/Player/` | Player locomotion, input bridge, camera | Medium — player context for dialogue |
| `NAAD/Animation/` | `CharacterAnimationController.cs`, `AnimationStateManager.cs` | Medium — NPC gesture/emotion animation |

**Assets already verified**: All 57 models in `naad_verify_report.json` passed (`passed: 57, failed: 0`).

### 2.2 Glitch9/AIDevKit/ — AI DevKit v5.4.2 (High Value, AGPL License Risk)

**License Warning**: AGPL-3.0-or-later requires derivative works to also be AGPL with source available. **Do NOT merge Glitch9 source into proprietary NAAD client code without legal review.** Study patterns only; reimplement cleanly.

| Path | Contents | Relevance |
|------|----------|-----------|
| `Runtime/Application/AIClient/AIClient.cs` | Abstract AI client, provider-agnostic API | **High** — unified LLM client pattern |
| `Runtime/Application/UnifiedApi/UnifiedApi.cs` | Unified API routing across providers | High — abstracts multiple LLM backends |
| `Runtime/System/Service/Abstractions/IGenerativeService.cs` | Service abstractions | Medium — interface patterns |
| `Runtime/Settings/AIClientSettings.cs`, `OpenAISettings.asset` | Settings ScriptableObjects | Medium — settings pattern for API keys |
| `Documentation/AI Dev Kit 4.0.0.pdf` | Full documentation | High — integration patterns reference |
| `package.json` | `com.glitch9.aidevkit`, v5.4.2, **AGPL-3.0-or-later** | **License risk: strong copyleft** |



### 2.3 LLMUnity/ — LLM for Unity v3.0.3 (High Value, Apache 2.0)

| Path | Contents | Relevance |
|------|----------|-----------|
| `Runtime/LLMClient.cs` | MonoBehaviour LLM client (local + remote) | **High** — local LLM execution option |
| `Runtime/LLMCharacter.cs` (deprecated → LLMAgent) | NPC character wrapper | **High** — pattern for NPC dialogue agents |
| `Runtime/LLM.cs` | Local LLM server (llama.cpp) | **High** — on-device inference, no API key |
| `Runtime/RAG/` | RAG system with usearch vector search | High — memory/context retrieval |
| `Runtime/LlamaLib/` | llama.cpp C# bindings | High — local inference engine |
| `Samples/` | SimpleInteraction, MultipleCharacters, FunctionCalling, RAG, ChatBot, KnowledgeBaseGame | High — reference implementations |
| `package.json` | `ai.undream.llm`, v3.0.3, **Apache 2.0** | **Safe to reuse** |

**Key capability**: Local model execution (no server cost) or remote API. Valuable as Edge Function fallback.

### 2.4 Plugins/NuGet/ — Native Dependencies (Medium Value)

`McpPlugin.dll`, `McpPlugin.Common.dll` (MCP bridge for Glitch9), plus Microsoft SignalR/Extensions/Json/Grpc/R3. Referenced by Glitch9's asmdef files. Keep as-is if Glitch9 is used; not needed by NAAD's current Supabase-based architecture.

### 2.5 Resources/ — Configuration Assets

| Path | Contents |
|------|----------|
| `Resources/SupabaseConfig.asset` | Supabase project URL + anon key |
| `Resources/AIDevKit/AIDevKitSettings.asset` | Glitch9 settings |
| `Resources/AIDevKit/Settings/OpenAISettings.asset` | OpenAI API settings |

### 2.6 Third-Party Asset Packs (Low/Medium Value)

| Folder | Contents | Relevance |
|--------|----------|-----------|
| `Awbmecreations/` | Mobile Optimize-Free Low Poly Cars | Low — vehicles for street scenes |
| `Brick Project Studio/` | (not fully explored) | Unknown — likely environment/building |
| `Kevin Iglesias/` | (not fully explored) | Unknown |
| `Marcos Schultz/` | (not fully explored) | Unknown |
| `PalmTreePack/` | (not fully explored) | Low — environment vegetation |
| `Versatile Studio Assets/` | (not fully explored) | Unknown — likely characters/environments |
| `FreeFlyCamera/` | Camera utility | Low — camera for cutscenes/dialogue |
| `TextMesh Pro/` | Unity's TMP text rendering | **High** — dialogue/subtitle UI text |
| `XR/` | XR support | Low — VR/AR dialogue (not in scope) |

## 3. ASSETS RELEVANT TO THE NPC DIALOGUE SYSTEM (Priority Ranked)

### Tier 1 — Critical (Already Integrated, Keep)
| # | Asset | Why Critical |
|---|-------|-------------|
| 1 | `NAAD/Networking/Supabase/SupabaseDialogueService.cs` | Direct client for the Edge Function we rebuilt. `TalkToTundeAsync()` calls `/functions/v1/npc-dialogue` exactly matching our endpoint. |
| 2 | `NAAD/Networking/Interfaces/IDialogueService.cs` | `DialogueResult` with `MemorySummary`, `MemoryImportance`, `MemoryType` fields match Edge Function output schema. |
| 3 | `NAAD/Content/Models/` | 57 verified assets including NPC characters (`CHR_*` humanoid) and environments where dialogue occurs. |
| 4 | `TextMesh Pro/` | For rendering dialogue text/subtitles in Unity UI. |

### Tier 2 — High Value (Reference/Reuse Candidates)
| # | Asset | Why Valuable |
|---|-------|-------------|
| 5 | `LLMUnity/` | Local LLM fallback + RAG for NPC memory retrieval. Apache 2.0 (safe to reuse). |
| 6 | `Glitch9/AIDevKit/` | Provider-agnostic LLM client patterns. AGPL (study only, do not merge). |
| 7 | `Resources/OpenAISettings.asset` | Pattern for storing LLM API keys client-side. |
| 8 | `NAAD/UI/` | Dialogue UI integration point. |

### Tier 3 — Medium Value (Optional)
| # | Asset | Why Optional |
|---|-------|-------------|
| 9 | `Plugins/NuGet/McpPlugin.dll` | If we adopt MCP for tool-calling from dialogue. |
| 10 | `FreeFlyCamera/` | Camera for dialogue cutscenes. |
| 11 | `Awbmecreations/` | Low poly cars for street scenes where NPCs appear. |

### Tier 4 — Low Value (Ignore for Now)
| # | Asset | Reason |
|---|-------|--------|
| 12 | `Models/TreasureChest.glb` | Single prop, not dialogue-relevant. |
| 13 | `Scenes/Bootstrap.unity` | Default scene, will be replaced by proper locations. |
| 14 | `XR/`, `Adaptive Performance/`, `Unity Cloud/` | Not dialogue-critical. |


## 4. PLAN: EXTRACT / MOVE / REUSE

### Phase 1: Audit & Document (Week 1 — No Code Changes)

| Task | Owner | Deliverable |
|------|-------|-------------|
| 1.1 Confirm AGPL implications of Glitch9 with legal | Lead | License clearance memo |
| 1.2 Catalog all 57 NAAD models with preview images | Art lead | `NAAD/Content/Models/_previews/` index |
| 1.3 Map `SupabaseDialogueService.TalkToTundeAsync()` to Edge Function response fields | Dev | Integration test |
| 1.4 Review LLMUnity samples for dialogue patterns | Dev | Notes doc |


### Phase 2: Reuse — Edge Function Client Integration (Week 1-2)

**Goal**: Ensure the Unity client correctly consumes the Edge Function we rebuilt.

| Task | Source Asset | Action |
|------|-------------|--------|
| 2.1 Verify `TalkToTundeAsync()` URL matches Edge Function | `SupabaseDialogueService.cs` | Already points to `/functions/v1/npc-dialogue` ✓ |
| 2.2 Verify `DialogueResult` maps to Edge Function envelope | `IDialogueService.cs` | `Success`, `Dialogue`, `Emotion`, `MemorySummary`, `MemoryType` ✓ |
| 2.3 Add `MemoryImportance` parsing to `TalkToTundeAsync()` | `SupabaseDialogueService.cs` | Currently reads summary/type but not importance — add parsing |
| 2.4 Add UI canvas for dialogue display | `NAAD/UI/`, `TextMesh Pro/` | Create dialogue bubble/subtitle prefab |
| 2.5 Connect `InteractWithNpcAsync()` in `FirstNightController` | `NAAD/FirstNightController.cs` | Already references dialogue service — verify wiring |


### Phase 3: Reuse — Local LLM Fallback (Optional, Week 3-4)

**Goal**: Add LLMUnity as an offline/fallback dialogue backend.

| Task | Source Asset | Action |
|------|-------------|--------|
| 3.1 Evaluate LLMUnity `LLMClient` + `LLMCharacter` | `LLMUnity/Runtime/` | Wrap behind `IDialogueService` as `LLMDialogueService` |
| 3.2 Use RAG for NPC memory context | `LLMUnity/Runtime/RAG/` | Load NPC facts into vector index |
| 3.3 Bundle a small .gguf model | `LLMUnity/Resources/usearch/` | Download Q4_K_M model (e.g., Llama-3.2-1B) |
| 3.4 Add feature flag: Edge Function vs Local LLM | `SupabaseConfig.cs` | `useLocalLLM` toggle |

### Phase 4: Reuse — Glitch9 Patterns (Study Only, Week 4)

**Goal**: Learn from Glitch9 without violating AGPL.

| Task | Source Asset | Action |
|------|-------------|--------|
| 4.1 Study provider-agnostic API pattern | `AIClient.cs`, `UnifiedApi.cs` | Document pattern, reimplement cleanly in NAAD namespace if needed |
| 4.2 Study settings ScriptableObject pattern | `AIClientSettings.cs`, `OpenAISettings.asset` | Reimplement `LLMSettings.asset` in NAAD (Apache/MIT) |
| 4.3 **DO NOT** copy Glitch9 source into NAAD | — | AGPL would force NAAD to be AGPL |

### Phase 5: Extract — Character Assets for Dialogue NPCs (Week 2-3)

**Goal**: Prepare NPC character models for dialogue scenes.

| Task | Source Asset | Action |
|------|-------------|--------|
| 5.1 Identify dialogue-critical NPCs | `naad_asset_manifest.json` | `CHR_BeachVendor_BCH_v001`, `CHR_Bouncer_NCL_v001`, `CHR_Crowd01-03_STR_v001` |
| 5.2 Import FBX/GLB into Unity | `NAAD/Content/Models/` | Verify humanoid rig, configure Avatar |
| 5.3 Create dialogue animation clips | `NAAD/Animation/` | Greeting, talking, emotion gestures |
| 5.4 Place NPCs in scene blockouts | `NAAD/Content/BLOCKOUT_*.md` | Suya Spot, Nightclub, Beach, Apartment |



## 5. FILE OPERATIONS MAP

### Create (New Files)
| Destination | Source Pattern | Purpose |
|-------------|---------------|---------|
| `Assets/NAAD/Dialogue/LLMDialogueService.cs` | `LLMUnity/Runtime/LLMClient.cs`, `LLMCharacter.cs` | Local LLM fallback implementing `IDialogueService` |
| `Assets/NAAD/Dialogue/DialogueUI.cs` | `NAAD/UI/`, `TextMesh Pro/` | Dialogue bubble/subtitle renderer |
| `Assets/NAAD/Dialogue/LLMSettings.asset` | `Resources/OpenAISettings.asset` | LLM provider settings ScriptableObject |
| `Assets/NAAD/Dialogue/` folder | — | New dialogue subsystem root |
| `Assets/Scenes/Apartment.unity` | `PRODUCTION_IMPLEMENTATION_PLAN.md` item 2 | First dialogue location |
| `Assets/Scenes/SuyaSpot.unity` | `PRODUCTION_IMPLEMENTATION_PLAN.md` item 6 | Dialogue with Mama Seyi |

### Modify (Existing Files)
| File | Change |
|------|--------|
| `SupabaseDialogueService.cs` | Add `MemoryImportance` parsing; add fallback to local LLM |
| `FirstNightController.cs` | Wire `InteractWithNpcAsync()` to dialogue UI |
| `SupabaseConfig.cs` | Add `useLocalLLM` feature flag |
| `DialogueBubble.cs` | Create (new) — TextMesh Pro dialogue bubble UI |
| `DialogueUIController.cs` | Create (new) — Dialogue UI controller bridging service to UI |
| `TundeDialogue.cs` | Create (new) — Dialogue ScriptableObject with DETERMINISTIC/AI/LOCAL_LLM modes |


### Move (Reorganize)
| From | To | Reason |
|------|----|--------|
| `NAAD/Content/Models/*.fbx/.glb` | `NAAD/Content/Models/NPCs/` and `NAAD/Content/Models/Environments/` | Organize by category for dialogue scene setup |
| `Resources/OpenAISettings.asset` | `NAAD/Dialogue/Resources/` | Co-locate LLM settings |
| `Assets/Kevin Iglesias/` | `Assets/NAAD/Animation/Avatars/` (or reference only) | Humanoid rig reference for CHR_* Avatar configuration |
| `Brick Project Studio/Apartment Kit/` prefabs | `Assets/NAAD/Scenes/` or `Assets/NAAD/UI/` | Interior props for First Night indoor dialogues |
| `Marcos Schultz/MSAdvancedCameraController/` | Reference for Tunde NPC camera setup | Camera rig patterns for dialogue scenes |

### Keep As-Is (No Change)
| Asset | Reason |
|-------|--------|
| `Glitch9/AIDevKit/` | AGPL license — study only |
| `Plugins/NuGet/` | Only needed if Glitch9 is integrated |
| `Models/TreasureChest.glb` | Not dialogue-relevant |
| `Scenes/Bootstrap.unity` | Placeholder |
| `Kevin Iglesias/` | Reference-only — import as Avatar rig configuration study; do not merge into NAAD character pipeline without review |
| `Marcos Schultz/` | Reference-only — camera controller and vehicle models; evaluate for reuse but not critical path |


## 6. RISK ASSESSMENT

| Risk | Severity | Mitigation |
|------|----------|------------|
| **AGPL license on Glitch9** | High | Do not copy source; study patterns only; reimplement cleanly under MIT/Apache |
| **No humanoid NPC rigs verified** | Medium | 57 models verified but need Avatar configuration in Unity; `CHR_*` models marked `humanoid: true` |
| **Edge Function requires GEMINI_API_KEY** | Medium | LLMUnity local fallback as backup |
| **57 models need import testing** | Low | All passed `naad_verify_report.json` but Unity import not yet tested |
| **Dialogue UI not yet built** | Medium | Use `NAAD/UI/` framework; create minimal dialogue bubble first |


## 7. SUCCESS CRITERIA

| # | Criterion | Verification Method |
|---|-----------|--------------------|
| 1 | `SupabaseDialogueService.TalkToTundeAsync()` returns `DialogueResult` with all Edge Function fields (dialogue, emotion, memoryCandidate.summary, memoryCandidate.type, memoryCandidate.importance) | Unit test + integration test against deployed Edge Function |
| 2 | Dialogue UI renders NPC text with TextMesh Pro | Manual QA in Unity Editor |
| 3 | 57 NAAD models import cleanly into Unity (FBX + GLB) | Import test each model, check for missing textures/materials |
| 4 | First Night gameplay: player can talk to Tunde at Suya Spot/Nightclub/Beach | Playthrough test |
| 5 | (Optional) Local LLM fallback works without Edge Function | Disable Edge Function, verify dialogue still works |
| 6 | (Optional) LLMUnity RAG retrieves NPC memory context | Query RAG index, verify relevant NPC facts returned |

## 8. IMMEDIATE NEXT STEPS

| # | Action | Owner | Timeline |
|---|--------|-------|----------|
| 1 | Test `SupabaseDialogueService.TalkToTundeAsync()` integration with deployed Edge Function | Backend / Unity | 1 day |
| 2 | Configure Avatar for CHR_* humanoid models; test FBX/GLB import into Unity | Technical Artist | 2 days |
| 3 | Create minimal TextMesh Pro dialogue UI bubble prefab | UI Designer | 2 days |
| 4 | Set up `useLocalLLM` feature flag in `SupabaseConfig.cs` | Engineer | 1 day |
| 5 | Verify LLMUnity local fallback works without Edge Function (optional) | Engineer | 3 days |
| 6 | Create `Assets/NAAD/Dialogue/` root folder and first dialogue scripts | Engineer | 1 day |
| 7 | Explore and integrate external assets: Kevin Iglesias humanoid rigs as Avatar reference; Brick Project Studio interior props for First Night scenes; Marcos Schultz camera controllers | Technical Artist / Engineer | 2 days |
