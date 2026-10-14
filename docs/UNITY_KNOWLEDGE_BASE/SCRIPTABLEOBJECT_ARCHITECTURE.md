# ScriptableObject Architecture — Data, Events, Commands (NAAD-applied)

Source: Modular Architecture with ScriptableObjects (Unity 6), all patterns.
This is the FUTURE architecture backbone for NAAD config/content — adopt
incrementally, never as a big-bang rewrite.

## Data containers (do first)
- SOs = project-level assets (not in hierarchy, no Transform), own YAML file,
  smaller than MonoBehaviour+GameObject+Transform serialization.
- Use for: item/venue stats, difficulty/progression curves, spawn tables,
  dialogue data, district visuals, audio clip groups.
- `[CreateAssetMenu]` per type; Inspector-editable; designers balance without code.
- Pitfalls: Editor Play-mode edits PERSIST on the asset (reset or Reload);
  runtime changes in builds are NOT saved; null refs before Destroy or GC stalls
  (SOs have native+managed halves); don't serialize SOs as "scratch".

## Extendable enums + delegates
- Empty SO asset = designer-friendly enum (compare by reference: same asset =
  same type). Add fields/methods later (e.g. `GameItem.weakness`, `IsWinner`)
  — impossible with C# enums.
- Pluggable behavior: SO holds `System.Action`/UnityAction delegate + data;
  MonoBehaviours reference the SO asset and call it (audio delegates, AI verbs).

## Observer via event channels (do second)
- SO in the middle: publisher → `EventChannelSO.Raise()` → subscribers.
  Project-level, survives scene loads, kills singleton spiderwebs.
- Recipe: `event UnityAction<T>/System.Action<T>` (event keyword = only SO
  invokes) + public `Raise(T)` + debug log hook. One channel per topic
  (`NightEnergyChanged`, `PresenceUpdated`, `PromptChanged`).
- UnityAction shows in Inspector (artist-friendly); System.Action is leaner.

## Command + RuntimeSet + InputReader (do third)
- Command: C# class or SO wrapping an action (`TravelCommand`, `PurchaseCommand`)
  with `Execute/Undo`; queueable, loggable, server-sendable — maps 1:1 to
  NAAD's validated-command model.
- RuntimeSet: SO holding `List<T>` of live objects (interactables in range,
  guests present); members register OnEnable/OnDisable. No Find-calls.
- InputReader: SO event channel fed by Input System actions; gameplay subscribes
  to `interactPressed` instead of polling keys — fixes the current legacy-Input
  tech debt cleanly.

## Applies to NAAD (adoption order)
1. `NightDistrictIds` + venue tuning → SO config assets.
2. `InteractionSystem` prompt/target → event channels; interactables → RuntimeSet.
3. Input → InputReader SO (new Input System) keeping `TryInteract()` seam.
4. Backend commands → Command objects serialized to game-api.
Never: push authoritative money/rep/state into SOs — Supabase owns truth.
