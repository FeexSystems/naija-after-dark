# Gates 7–10 — Night District vertical slice

## Gate 7 — First World

```
LAGOS METRO
  └── Night District
        ├── Apartment
        ├── Street
        ├── Suya Spot
        ├── Nightclub
        └── Beach
```

Seeded in migration `0005_night_district_world.sql`.

## Gate 8 — NPCs

| NPC | Role | Dialogue |
|-----|------|----------|
| Tunde | Promoter | AI (Gate 9) |
| Mama Seyi | Suya vendor | Deterministic |
| Amaka | Creative | Deterministic |
| Emeka | Developer | Deterministic |
| Dami | Socialite | Deterministic |

## Gate 9 — Gemini

```
Unity → Edge Function npc-dialogue → context RPC → Gemini → validate → response
```

- API key only on server
- No DB mutation from Gemini
- Schema: `ai/schemas/npc-dialogue-response.schema.json`

Deploy:

```bash
supabase secrets set GEMINI_API_KEY=your_key
supabase functions deploy npc-dialogue
```

## Gate 10 — Memory

1. AI returns optional `memoryCandidate`
2. Client/server calls `naad_commit_npc_memory` after validation
3. Dedup within 24h
4. Retrieved via `naad_npc_dialogue_context` (top 5 by importance)

## Economy touch

`naad_buy(request_id, sku, location_id)` — SUYA_PLATE ₦5,000 at Suya Spot only.
