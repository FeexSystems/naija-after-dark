# UI — uGUI Today, UI Toolkit Tomorrow + Unity Services (NAAD-applied)

Sources: Unity 6.6 UI-system comparison; UI Toolkit advanced-dev guide
(Unity 6.0); Unity Services solutions overview. Pairs with NAAD
`UI/UI_FRAMEWORK_ARCHITECTURE.md`, `UI_COMPONENT_LIBRARY.md`, `VerifyUI.cs`.

## Today: uGUI is CORRECT for NAAD (Unity 6.6 recommendation)
- Runtime recommendation = uGUI; UI Toolkit = alternative for textureless,
  heavily-styled, multi-resolution HUDs; IMGUI = Editor-only/debug.
- uGUI wins for: MonoBehaviour refs, in-scene authoring, serialized UnityEvents,
  Animation-Clip-driven UI, timeline keyframed UI, small teams w/ GO workflows.
- NAAD already: `MainHudController` prompt contract, `SafeAreaFitter` (notch),
  44dp touch targets (`InteractButton`), EventSystem touch routing (mouse+device).
  Keep. Verify via `VerifyUI` in Batchmode.

## UI Toolkit (advanced-dev guide — adopt selectively, later)
- Stack: UXML (layout) + USS (style, web-like) + C# (logic) + UI Builder
  (visual authoring). Assets: prepare graphics/fonts (SDF), sprite atlas,
  SVG support, dynamic atlas, global styles, data binding, localization,
  custom controls, ListView virtualization.
- Perf: avoid USS/layout thrash (batch changes), virtualize lists, bind data
  not per-frame strings, reuse atlas, minimize redraw regions.
- Naming: BEM-like `block__element--state` for UXML/USS/C# consistency.
- Migrate uGUI→Toolkit ONLY when: HUD complexity explodes, designers need
  document workflows, or textureless/R2L/emoji/SVG demands appear. Migration is
  manual (no auto-converter); run both stacks in parallel per-screen.

## IMGUI: never for runtime HUD. Editor tooling + debug overlays only.

## Unity Services mapping (NAAD = Supabase, not UGS — note parity)
| Need | NAAD (current) | UGS equivalent | Rule |
|---|---|---|---|
| Auth | Supabase Auth (+anon key in client) | Authentication | anon in Unity OK; service-role NEVER leaves server |
| World truth | Postgres + game-api validation | Cloud Save + Cloud Code | client sends commands+requestId; server validates+mutates+emits event |
| Social/presence | Supabase realtime/rooms | Lobby + Relay + Friends | relay for P2P voice/packets only; state stays server-side |
| Multiplayer sim | NONE (social sim, async) | Netcode (dedicated or server-auth async via Cloud Code) | no Unity Netcode pkgs; chess-sample pattern (Cloud Code authority) if async PvP ever lands |
| Config/economy | game-api + world-model | Remote Config + Economy + CCD overrides | overrides are server data, never client constants |
| Voice | future | Vivox via Lobby integration | lobby owns membership; voice follows |

## Trust boundaries (both guides + ARCHITECTURE.md agree)
Unity client = UNTRUSTED. Money/inventory/rep/progression never accepted from
client. Every mutation = validated command → constraint check → domain event →
snapshot back. AI proposes, never mutates.
