# World Model ↔ PostgreSQL mapping (SPEC v0.1)

Authority: **PostgreSQL / Supabase**.  
JSON Schema and TypeScript describe contracts; they do not store state.

## Live tables (Gate 1 migration `0001_initial_world_model.sql`)

| Entity | Table | Notes |
|--------|-------|--------|
| Player | `public.players` | PK `id` → `auth.users(id)` |
| Wallet | `public.wallets` | PK `player_id`; balance ≥ 0; currency NGN |
| Location | `public.locations` | `district_id` optional, no FK yet |
| Npc | `public.npcs` | FK `current_location_id` → locations |
| Relationship | `public.relationships` | unique (subject_id, target_id); scores 0–100 |
| WorldState | `public.world_state` | singleton `id = 1`; column `game_time` |

### Column name mapping (API ↔ DB)

| TypeScript / JSON | Postgres column |
|-------------------|-----------------|
| `displayName` | `display_name` |
| `playerId` | `player_id` |
| `currentLocationId` | `current_location_id` |
| `subjectId` / `targetId` | `subject_id` / `target_id` |
| `relationshipType` | `relationship_type` |
| `gameTime` | `game_time` |
| `trafficLevel` | `traffic_level` |
| `nightlifeLevel` | `nightlife_level` |
| `createdAt` / `updatedAt` | `created_at` / `updated_at` |

## Planned tables (not migrated yet)

| Entity | Proposed table | Key constraints (when migrated) |
|--------|----------------|----------------------------------|
| District | `public.districts` | PK uuid |
| Business | `public.businesses` | FK location_id, optional owner_npc_id |
| Vehicle | `public.vehicles` | optional owner_player_id |
| Property | `public.properties` | FK location_id, tenure check |
| Item | `public.items` | unique sku |
| Inventory | `public.inventories` | unique (player_id, item_id); qty ≥ 0 |
| WalletTransaction | `public.wallet_transactions` | append-only; amount > 0 |
| Job | `public.jobs` | FK player_id |
| Event | `public.events` | FK location_id; start < end |
| Mission | `public.missions` | FK player_id |
| Reputation | `public.reputations` | PK (player_id, domain) |
| NpcMemory | `public.npc_memories` | FK npc_id, player_id |
| Message | `public.messages` | FK recipient_player_id |
| SocialPost | `public.social_posts` | — |
| GameSession | `public.game_sessions` | FK player_id |

## Ownership & mutation rules

- **Wallets / inventory / reputation / progression:** server commands only.
- **NPC memory:** AI proposes → validator → insert. Gemini never writes directly.
- **World clock:** server simulation owns `game_time`. Unity interpolates presentation.
- **RLS (live):** players and wallets are owner-read; no authenticated UPDATE on wallets.

## Idempotency

Future command handlers must accept `requestId` and treat ledger writes as idempotent on that key.
