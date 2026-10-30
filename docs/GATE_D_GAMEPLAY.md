# Gate D — Gameplay / interaction

## Scope

Script-driven third-person movement (existing `PlayerLocomotionController`) plus
world interaction over `SceneAnchor`s. Interactions **propose** commands; the
server remains authoritative.

## Components

| Component | Role |
|-----------|------|
| `PlayerLocomotionController` | CharacterController movement + animation hooks |
| `SceneAnchor` | Placeable interactable (Talk / Travel / Spend / Attend / Examine) |
| `PlayerInteractionController` | Proximity probe + `ICommandService` / `IDialogueService` dispatch |

## Setup

1. Player prefab: `CharacterController` + `PlayerLocomotionController` + `PlayerInteractionController`.
2. Place `SceneAnchor`s at exits, stalls, NPCs (set kind + target ids).
3. Press **E** (or Fire1) when in range.

## Trust

- Travel / Spend / Attend → `naad_execute_command`
- Talk → `npc-dialogue` Edge Function (Gemini proposals only)
- Examine is local presentation only
