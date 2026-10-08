# Gates 15–20

## 15 Multiplayer rooms

- `game_rooms` + `room_presence` (ephemeral pose/emote only)
- Persistent state stays in wallets / relationships / etc.
- RPCs: `naad_join_room`, `naad_leave_room`, `naad_update_presence`, `naad_list_room_presence`

Realtime fan-out can later use Supabase Realtime on `room_presence`.

## 16 Mobile optimization

Process gate — profile on device before optimizing:

- FPS, RAM, CPU, GPU, battery, latency, download size, load times
- Low / mid / high Android + iPhone

No schema changes.

## 17 Content expansion

- Locations: Restaurant, Lounge, Hotel, ATM, Gas Station (+ original five)
- Careers: Developer, DJ, Driver, Event Promoter, Restaurant Worker
- `naad_set_career`

## 18 Social graph

- `opportunities` table
- `naad_refresh_opportunities` — Tunde trust ≥ 55 → “Meet the DJ”
- `naad_accept_opportunity`

## 19 Events

- `world_events` + `event_attendance`
- Seed: Beach Party, Club Night
- `naad_list_events`, `naad_attend_event`

## 20 World reactivity

- Attendance raises `nightlife_level` and `traffic_level`
- Domain event `WORLD_REACTIVITY`
