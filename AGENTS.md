# NAIJA AFTER DARK - Agent Development Guide

## Project Overview
NAIJA AFTER DARK is a Nigerian nightlife life-simulation game built with:
- **Unity 6000.6.5f1** - Game client (untrusted, server-authoritative)
- **Supabase/PostgreSQL** - Authoritative backend (world state)
- **Gemini AI** - Dialogue interpretation (server-side only)
- **TypeScript** - Backend contracts and AI orchestrator

## Architecture
- **Server-authoritative**: Client sends commands; server validates and applies
- **World model**: PostgreSQL is source of truth for all game state
- **AI boundaries**: Gemini proposes but never directly mutates state
- **Trust model**: Unity (untrusted) → Edge Functions (trusted) → Postgres (authoritative)

## Directory Structure
```
naija-after-dark/
├── backend/
│   ├── ai-orchestrator/    # Gemini integration (TypeScript)
│   └── game-api/          # Command contracts (TypeScript)
├── game/unity/            # Unity client (C#)
│   └── Assets/NAAD/       # Project-specific code
├── supabase/              # Supabase configuration + migrations
├── world-model/           # JSON schemas + TypeScript types
├── tools/                 # Asset pipeline tools
├── docs/                  # Technical documentation
└── AGENTS.md              # This file
```

## Key Principles for Agents
1. **Never add secrets to Unity client** - Service-role keys and Gemini credentials stay server-side
2. **Never mutate authoritative state from client** - All changes go through `naad_execute_command`
3. **Ask before major architectural changes** - The trust model is intentional
4. **Follow gate-based development** - See docs/PROJECT_PLAN.md for roadmap
5. **Unity-specific guidance** - See game/unity/AGENTS.md for Unity development

## Important Files
- `README.md` - Project overview and vision
- `AGENTS.md` (this file) - Agent development guide
- `docs/PROJECT_PLAN.md` - Full roadmap and gate status
- `docs/ARCHITECTURE.md` - Architecture and trust boundaries
- `docs/GATE_STATUS.md` - Current gate verification status
- `game/unity/AGENTS.md` - Unity-specific agent guidance
- `world-model/README.md` - World model ownership map

## Development Workflow
1. **Read relevant docs** before making changes
2. **Check existing patterns** in the relevant module
3. **Maintain trust boundaries** - don't blur client/server lines
4. **Document decisions** in appropriate docs/
5. **Test against authoritative backend** - not local fakes

## Common Tasks
- **Backend change**: Update `backend/ai-orchestrator/` or `backend/game-api/`
- **Unity change**: Work in `game/unity/Assets/NAAD/`
- **Schema change**: Update `world-model/schemas/` + `supabase/migrations/`
- **New feature**: Follow docs/PROJECT_PLAN.md gate sequence
