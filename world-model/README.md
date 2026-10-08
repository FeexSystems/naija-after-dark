# NAAD World Model — SPEC v0.1

**The World Model is authoritative.**  
Unity renders and interacts with the World Model.  
Gemini interprets the World Model but cannot directly mutate authoritative state.

## Where does every piece of game state live?

| Concern | Authoritative store | Client may |
|---------|---------------------|------------|
| Identity / account | `auth.users` + `public.players` | Read own profile |
| Money | `public.wallets` (+ ledger later) | Read own balance only |
| Inventory / items | `public.inventories` + `public.items` (future) | Read own |
| Location of player | session + server command result | Display only |
| NPC identity / mood / job | `public.npcs` | Read |
| NPC memory | `public.npc_memories` (future) | Never write |
| Relationships | `public.relationships` | Read participant rows |
| World clock / weather | `public.world_state` | Read / interpolate presentation |
| Events / missions | `public.events` / `public.missions` (future) | Read; accept via command |
| Messages / social | `public.messages` / `public.social_posts` (future) | Read own; send via command |
| Reputation | derived + `public.reputations` (future) | Read only |
| AI dialogue | ephemeral (orchestrator); memory candidates validated server-side | Display only |

**Unity never owns money, inventory, reputation, or progression.**  
**Gemini never writes these tables.**

## Layout

```
world-model/
├── README.md                 # this file — ownership map
├── schemas/                  # JSON Schema (Draft 2020-12)
├── types/                    # TypeScript interfaces
├── mappings/
│   └── postgresql.md         # table ↔ schema mapping
└── diagrams/
    └── er.mmd                # Mermaid ER
```

## Design rules (v0.1)

1. Every field has a gameplay purpose.
2. No speculative columns.
3. IDs are UUIDs unless singleton (`world_state.id = 1`).
4. Money is `bigint` minor units (kobo-style integer NGN).
5. Relationship scores are integers 0–100.
6. Enums are closed strings in schema; Postgres uses `text` + check or enum later.
7. Timestamps are `timestamptz`.
8. Mutations go through validated server commands + domain events (Gate 5+).

## Entity index

| Entity | Schema | Live in Postgres (Gate 1) |
|--------|--------|---------------------------|
| Player | `player.schema.json` | yes |
| Wallet | `wallet.schema.json` | yes |
| WalletTransaction | `wallet-transaction.schema.json` | no |
| Location | `location.schema.json` | yes |
| District | `district.schema.json` | no |
| Npc | `npc.schema.json` | yes |
| NpcMemory | `npc-memory.schema.json` | no |
| Relationship | `relationship.schema.json` | yes |
| WorldState | `world-state.schema.json` | yes |
| Weather | enum on WorldState | yes (text) |
| Business | `business.schema.json` | no |
| Vehicle | `vehicle.schema.json` | no |
| Property | `property.schema.json` | no |
| Item | `item.schema.json` | no |
| Inventory | `inventory.schema.json` | no |
| Job | `job.schema.json` | no |
| Event | `event.schema.json` | no |
| Mission | `mission.schema.json` | no |
| Reputation | `reputation.schema.json` | no |
| Message | `message.schema.json` | no |
| SocialPost | `social-post.schema.json` | no |
| GameSession | `game-session.schema.json` | no |
