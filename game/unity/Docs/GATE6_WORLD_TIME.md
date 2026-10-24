# Gate 6 — World Time

## Rule

Server owns `game_time`. Unity only presents / interpolates.

| Period | Hours | Nightlife bias |
|--------|-------|----------------|
| MORNING | 06:00–08:59 | 15 |
| DAY | 09:00–15:59 | 25 |
| TRANSITION (sunset) | 16:00–18:59 | 55 |
| NIGHT | 19:00–23:59 | 80 |
| LATE_NIGHT | 00:00–02:59 | 90 |
| AFTER_HOURS | 03:00–05:59 | 60 |

## RPCs

- `naad_get_world_clock()` — read snapshot
- `naad_advance_world_time()` — advance by real elapsed × time_scale
- `naad_set_world_period(period)` — demo/test jump

## Unity

Attach `WorldClockPresenter` to a scene object.  
Enable `demoCycleOnStart` for DAY → TRANSITION → NIGHT → LATE_NIGHT.

**Status:** the presenter exists (`Assets/NAAD/Gameplay/World/`) but is not attached
to any scene or prefab yet, so the pass condition below cannot be demonstrated from a
build until it is.

### Offline parity
`StubWorldStateService` (used when Supabase is not configured) must return the same
nightlife bias per period as `naad_nightlife_for_period`. This is enforced by
`NAAD.Editor.WorldClockContractVerifier.RunVerification` (CI job
`verify-world-clock`), which fails when the two tables drift.

## Pass condition

Demonstrate DAY → SUNSET → NIGHT → LATE NIGHT with ambient intensity / nightlife changes.
