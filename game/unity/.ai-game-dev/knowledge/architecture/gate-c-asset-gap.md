# Gate C Asset Sourcing — Blocked

**Finding (2026-11-02):** the eight Gate C animation clips cannot be sourced from
assets already in the project. Verified by exhaustive search, not assumption.

## What is in the repo

| Source | Contents | Usable for Gate C? |
|---|---|---|
| `Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx` | Humanoid rig (`animationType: 3`) with `clipAnimations: []` — **zero embedded clips** | No |
| `Assets/NAAD/Content/Models/*/CHR_*.fbx` | Static character models, no animation | No |
| `Assets/Kevin Iglesias/Human Animations/` | 84 FBX, but only **Idles** and **Work** (farming, fishing, mining, hammering, gathering, crafting, object-grip) | Only `HumanM@Idle01` resembles a required clip |
| `Assets/Brick Project Studio/` | Furniture/door `.anim` clips, non-humanoid | No |

Two further blockers on the Kevin Iglesias pack:

1. It is **gitignored** (`.gitignore:52-53`) — absent from every clean checkout, so
   anything wired from it breaks CI and fresh clones.
2. It is a medieval/crafting themed library. It has no walk, no run, no greeting and
   no phone animations at all.

## Consequence

Gate C passes its contract today on eight **placeholder** clips
(`MVP_PLACEHOLDER_CLIPS.md` states they are not production takes). A green Gate C
job means the *wiring* is correct, not that the player is animated.

## What is actually required

Real humanoid takes for: locomotion Idle/Walk/Run, phone Retrieve/Hold/Browse/
PutAway, social Greeting. The documented acquisition route is Mixamo or Unity
Starter Assets — see `Assets/NAAD/Animation/ASSET_ACQUISITION.md`. Whoever imports
them must preserve the existing `.meta` GUIDs so the AnimatorController bindings
survive (stability is the whole reason the clips are committed as `.anim`).

Related: [Mutation Surface](/architecture/mutation-surface.md).
