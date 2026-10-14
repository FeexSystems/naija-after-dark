# Designer Playbook — Modular Gameplay Systems (NAAD-applied)

Source: Game Designer Playbook §§ Modular gameplay / Input / UI+Events /
Levels / Cinemachine+Timeline / Juice.

## Pitch → prototype → marketable
- Pitch = genre + feel + refs, 20–30s; one-pager for stakeholders; playable
  demo/trailer with PLACEHOLDER-labeled art. Prototype with Asset Store, swap
  to custom art after funding (exactly the NAAD Gate B placeholder strategy).
- Vertical slice > breadth. This repo's gate discipline ("implement only the
  current gate; no gate passes without verification") is the playbook rule.

## Modular gameplay systems
- Build swappable systems (movement, interaction, travel, economy hooks) behind
  small seams (`TryInteract()`, `TravelCommand`, scene anchors), not bespoke
  per-scene code. Scenes are data; systems are code.
- Prefabs = reusable GameObjects; Variants = district skins; Nested = compound
  assemblies (stand = structure + grill + seating).

## Input (guide: new Input System; NAAD: legacy + UI fallback)
- Guide default: Input System package + character controller + Cinemachine.
- NAAD reality: `com.unity.inputsystem 1.20.1` INSTALLED but gameplay reads
  legacy `UnityEngine.Input` (fully-qualified — `NAAD.Gameplay.Input`
  namespace shadows bare `Input`; VirtualJoystick documents this).
- Rule: new gameplay input uses Input System actions; keep the
  `TryInteract()` seam so E-key, touch button, and future actions share one path.

## UI + UnityEvents; levels; cameras
- UI buttons raise UnityEvents → `TryInteract`/commands; prompts read from the
  same `CurrentPrompt` the HUD shows (no drift).
- Levels: grid snapping, gizmos for invisible volumes (anchors draw 0.75m
  trigger spheres), FBX Exporter round-trip with artists.
- Cameras: Cinemachine virtual cams + Timeline cutscenes for First Night beats
  (future); keep camera-tag contract (`MainCamera` validated per scene).

## Juice (microinteractions, randomness, feel)
- Small: prompt fade, button press scale, grill ember flicker, stage light pulse.
  Cheap, pooled, no per-frame allocation.

## Applies to NAAD
FirstNightController + HUD + InteractionSystem already follow this: one
interaction path, prompt contract, anchor-driven travel. Keep it.
