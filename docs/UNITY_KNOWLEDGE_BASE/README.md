# Unity Knowledge Base — Naija After Dark

Distilled from 5 Unity field guides (439 pages, extracted Oct 2026) into
project-ready memory for this repo (`game/unity`, Unity 6000.6.5f1, Built-in RP).

## Sources (raw text in `_tmp_pdftext/`, distilled here)
| # | Guide | Pages | Chars |
|---|-------|-------|-------|
| 1 | Unity Game Dev Field Guide (2020 LTS ed.) | 79 | ~110k |
| 2 | Intro to DOTS, Unity 6 ed. | 60 | ~102k |
| 3 | Game Designer Playbook (2021) | 103 | ~150k |
| 4 | Animation in Unity (2022 LTS ed.) | 121 | ~123k |
| 5 | Modular Architecture with ScriptableObjects (Unity 6) | 76 | ~90k |

## Files
- `FIELD_GUIDE_CORE.md` — project org, prefabs, scenes, LTS/version lock
- `SCRIPTING_PERFORMANCE_JOBS_BURST.md` — GC, Burst/Jobs, profiling
- `DESIGNER_PLAYBOOK_SYSTEMS.md` — modular gameplay, Input, UI Events, Cinemachine/Timeline, juice
- `SCRIPTABLEOBJECT_ARCHITECTURE.md` — data containers, event channels, Runtime Sets, InputReader, Command
- `DOTS_UNITY6_GUIDE.md` — when (not) to use DOTS; Netcode authoritative model
- `ANIMATION_UNITY.md` — import → Humanoid → Animator → Rigging → Timeline
- `UI_TOOLKIT_VS_UGUI.md` — UGUI (current) vs UI Toolkit (future) + Unity Services mapping
- `GUARDRAILS.md` — enforceable repo rules (START HERE for agents)
- `AGENTIC_SKILLS_AND_REASONING.md` — memories, reasoning flows, skill wiring
- `SOURCES.md` — provenance + page pointers

## How to use
1. Agents: read `GUARDRAILS.md` + `AGENTIC_SKILLS_AND_REASONING.md` before any Unity change.
2. Humans: pick the topic file; each ends with "Applies to NAAD".
3. Gate B note: this project is **Built-in RP** (`m_CustomRenderPipeline:0`);
   URP guidance is marked FUTURE — do not mass-convert shaders now.
