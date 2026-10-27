# NAAD Cloud Code Modules — CI/CD Integration Guide

**Date**: 2026-10-25  
**Branch**: `chore/unity-mcp-scaffolding-and-gitignore`  
**Unity CLI**: Experimental (install from https://docs.unity.com/en-us/unity-cli)

## Overview

NAAD uses **server-authoritative Cloud Code C# modules** for dialogue and gameplay orchestration. Unlike the current single-edge-function-per-dialogue model, Cloud Code modules:

- Support **multiple entry points** in a single compiled artifact
- Enable **in-memory state caching** between endpoints for live NPC conversations
- Generate **type-safe client bindings** from the Editor, preventing misspelled method names at compile time
- Deploy via CLI, REST API, or Unity Editor Deployment window

## Architecture

```
Unity (untrusted client)
   ↓  call module function
Cloud Code Runtime (open-source .NET 9)
   ↓  validate + propose
Gemini (AI server)  ←  ONLY proposal; never writes DB
   ↓  validated by server
Supabase / PostgreSQL  (authoritative state)
```

### Trust Boundaries

| Component | Trust Level | Role | Mutation Rights |
|-----------|-------------|------|-----------------|
| Cloud Code C# modules | Trusted server | Validate + propose + apply | Yes (after validation) |
| Gemini | External AI | Reasoning / dialogue generation | None (proposal only) |
| Unity client | Untrusted | Render, input, presentation | None (sends commands only) |
| PostgreSQL / Supabase | Authoritative | Single source of truth | Full (via controlled RPCs) |

## Module Scaffolding

See `NNPCCommunication_MODULE.csproj` and `README.md` for the module project structure.

### Key Files

```
game/unity/Assets/NAAD/CloudCodeModules/
├── NNPCCommunication_MODULE.csproj   # .NET 9 C# module project SDK
├── README.md                         # Module structure, workflow, CI/CD
└── Shared/
    ├── INpcCommunicationModule.cs    # Contract (method signatures)
    ├── NpcCommunicationModule.cs     # Implementation + [CloudCodeModule]
    ├── Models/                       # DTOs shared between client and server
    └── NpcMemoryStore.cs             # In-memory store for testing
```

## Unity CLI Cloud Commands

```bash
# Start local Cloud Code server for iteration
unity cloud-code server start --module NNPCCommunication

# Deploy module to Cloud Code
unity cloud-code deploy NNPCCommunication --project <project-id>

# Check deployment status
unity cloud-code status NNPCCommunication

# Generate type-safe client bindings
unity cloud-code generate-bindings NNPCCommunication

# Verify module functions (PlayMode tests filtered to CloudCodeTests)
unity test --affected --test-filter "CloudCodeTests"
```

## CI/CD Integration

Automated module deployment is integrated into `.github/workflows/naad-unity.yml`.

### Deployment Job

```yaml
- name: Deploy Cloud Code module
  run: |
    unity cloud-code deploy NNPCCommunication --project ${{ secrets.CLOUD_CODE_PROJECT_ID }}
  env:
    CLOUD_CODE_PROJECT_ID: ${{ secrets.CLOUD_CODE_PROJECT_ID }}
```

### Type-Safe Binding Generation

```yaml
- name: Generate client bindings
  run: |
    unity cloud-code generate-bindings NNPCCommunication \
      --output game/unity/Assets/NAAD/CloudCode/Generated/
```

### Verification

```bash
# Run affected PlayMode tests filtered to CloudCodeTests
unity test --affected --test-filter "CloudCodeTests"
```

## Module Contract

### INpcCommunicationModule

```csharp
public interface INpcCommunicationModule
{
    Task<TalkToTundeResponse> TalkToTundeAsync(
        string playerMessage, string requestId);

    Task<List<NpcMemoryContext>> GetNpcMemoryContextAsync(
        string npcId, string playerMessage, string requestId);

    Task<bool> CommitNpcMemoryAsync(
        string requestId, string npcId, string memoryType,
        string summary, float importance);
}
```

### Module Implementation

```csharp
[CloudCodeModule]
public class NpcCommunicationModule : INpcCommunicationModule
{
    [CloudCodeFunction("TalkToTunde")]
    public async Task<TalkToTundeResponse> TalkToTundeAsync(
        string playerMessage, string requestId)
    {
        // 1. Validate input
        if (string.IsNullOrEmpty(playerMessage)) return Failure(requestId);

        // 2. Propose (Gemini only)
        var ai = await GeminiClient.ProposeAsync(playerMessage, requestId);

        // 3. Return validated response (no DB mutation from Gemini)
        return new TalkToTundeResponse
        {
            Dialogue = ai.Dialogue,
            Emotion = ai.Emotion,
            MemoryCandidate = ai.MemoryCandidate
        };
    }
}
```

## Entity Reference

- `NPC_ID`: PlayerId
- `REQUEST_ID`: Unique request identifier (idempotency)
- `MEMORY_TYPE`: PlayerId | MemoryType | Summary
- `CONTEXT`: Returned from `naad_npc_dialogue_context`

## Constraints

- **No UnityEngine** — Cloud Code uses the open-source .NET runtime, not the Unity scripting backend.
- **One module class per entry point** — the Cloud Code runtime discovers public methods by `[CloudCodeModule]` attribute.
- **Type-safe contracts** — the Editor generates client bindings at deploy time.
- **In-memory state caching** — enabled between endpoints for live NPC conversations.
- **Validation before persistence** — all server-side state changes validate input before applying.

## Local Development

```bash
# 1. Create a module
unity modules create --name NNPCCommunication --sln NNPCCommunication_MODULE.csproj

# 2. Run a local Cloud Code server
unity cloud-code server start --module NNPCCommunication

# 3. Edit code and test
cd CloudCodeModules && dotnet build

# 4. Deploy
cd ..
unity cloud-code deploy NNPCCommunication --project <project-id>

# 5. Verify
unity cloud-code status NNPCCommunication
```
