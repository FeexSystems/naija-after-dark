# Gate 5 — Command system

## Rule

Unity never does `UPDATE wallets` or `UPDATE player_locations`.  
It sends a **command** with a **requestId**. The server validates and mutates.

```
Unity
  → GameCommand (TRAVEL + requestId)
  → PostgREST RPC naad_travel
  → validate
  → mutate player_locations
  → domain_events PLAYER_TRAVELED
  → command_receipts (idempotent)
  → CommandResult
```

## TRAVEL pass path

Apartment (`a1111111-...101`) → Suya Spot (`a1111111-...102`)

1. Authenticate (Gate 4)
2. Call `TravelCommand.TravelToSuyaSpotAsync()` or RPC directly
3. Server moves player if destination exists and is different
4. Replaying the same `requestId` returns the stored receipt

## Seed location IDs

| Name | Id |
|------|-----|
| Apartment | `a1111111-1111-1111-1111-111111111101` |
| Suya Spot | `a1111111-1111-1111-1111-111111111102` |

## SQL check

```sql
select * from public.player_locations where player_id = auth.uid();
select * from public.domain_events where event_type = 'PLAYER_TRAVELED' order by created_at desc limit 5;
```
