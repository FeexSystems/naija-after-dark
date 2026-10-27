# NAAD Cloud Code Modules

Server-authoritative C# modules for NAAD. Built with the Cloud Code C# SDK (v3.0.0-exp.1).

## Architecture

```
Unity (untrusted client)
   ↓  call module function
Cloud Code Runtime (open-source .NET 9)
   ↓  validate + apply
Supabase / PostgreSQL  (authoritative state)
```

## Constraints

- **No UnityEngine** — Cloud Code uses the open-source .NET runtime, not the Unity scripting backend.
- **One module class per entry point** — the Cloud Code runtime discovers public methods by [CloudCodeModule] attribute.
- **Type-safe contracts** — the Editor generates client bindings at deploy time; game code calls modules through generated partial classes, avoiding misspelled method names.

## Module Structure

```
CloudCodeModules/
├── NNPCCommunication_MODULE.csproj
├── README.md
└── Shared/
    ├── INpcCommunicationModule.cs      # Contract (method signatures)
    ├── NpcCommunicationModule.cs       # Implementation + [CloudCodeModule]
    ├── Models/                         # DTOs shared between client and server
    └── NpcMemoryStore.cs               # In-memory store for testing
```

## Local Development

### 1. Create a module

```bash
# Via CLI
unity modules create --name NNPCCommunication --sln NNPCCommunication_MODULE.csproj
```

### 2. Run locally

```bash
# Start a local Cloud Code server for iteration
unity cloud-code server start --module NNPCCommunication
```

### 3. Deploy

```bash
# Via UGS CLI
ugs cloud-code deploy NNPCCommunication --project <project-id>

# Or via Unity Editor Deployment window
```

### 4. Client usage

```csharp
// Generated partial class (NNPCCommunicationModule.g.cs) is created at deploy time
var module = CloudCodeModule<INpcCommunicationModule>.Instance;
var response = await module.TalkToTundeAsync(playerMessage, requestId);
```

## CI/CD

Cloud Code module deployment is integrated into the CI pipeline in `.github/workflows/naad-unity.yml`. See `CloudCodeModules/docs/CloudCode_Modules.md` for the automated deployment workflow.

## Security

- **No service-role keys** in module code or Unity client
- **Validation before persistence** — all server-side state changes validate input before applying
- **Input bounds** — all incoming data clamped to defined ranges
