# NAIJA AFTER DARK — Development Guide

## Gate Discipline

Implement only the current gate.  
Do not skip ahead.  
Do not claim a gate passes unless verification succeeds.

## Before Any Code Change

1. Inspect existing architecture
2. Identify relevant files
3. Identify dependencies
4. Identify existing contracts
5. Avoid duplicating existing functionality

## After Implementation

- Run tests
- Run typecheck
- Run build
- Report changed files
- Report commands executed
- Report failures
- Report remaining risks

## Local Setup (Gate 0 target)

Required:

- Unity Hub + Unity LTS + Android Build Support
- Git + GitHub CLI
- Node.js LTS
- Supabase CLI
- VS Code or Windsurf

Optional initially:

- Docker Desktop

Do not install Kubernetes or complex cloud stacks yet.

## Repository Commands (reference)

```bash
# status must be clean after structure commit
git status

# future branch pattern
git checkout -b feat/naad-gate-XX-description
```

## Architecture Rules Reminder

See `docs/ARCHITECTURE.md` and root `README.md`.

The World Model is authoritative.  
Unity renders and interacts with the World Model.  
Gemini interprets the World Model but cannot directly mutate authoritative state.

## Solo Developer Loop

1. Define schema
2. Define server command
3. Define validation
4. Define domain event
5. Implement backend
6. Implement Unity client
7. Add AI only if necessary
8. Test
9. Commit
10. Next gate

Never start with UI and work backward.
