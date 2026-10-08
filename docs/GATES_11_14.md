# Gates 11–14

## Gate 11 — Economy

| SKU | Price |
|-----|-------|
| SUYA_PLATE | ₦5,000 |
| TRANSPORT_SHORT | ₦4,000 |
| CLUB_TICKET | ₦20,000 |
| BEACH_DRINK | ₦3,000 |
| SOFT_DRINK | ₦1,500 |

RPCs: `naad_get_wallet`, `naad_spend(request_id, sku, location_id?)`

## Gate 12 — Relationships

`naad_apply_relationship_delta` — server clamps ±10 per call, scores 0–100.  
AI only **suggests**; this RPC **applies**.

## Gate 13 — Phone

`naad_seed_tunde_beach_invite` → message with GO / ASK_DETAILS / DECLINE  
`naad_respond_message` → records choice + relationship nudge

## Gate 14 — First Night

```
naad_start_night
  → seed Tunde invite
  → period NIGHT
  → player loop (travel, buy, talk)
naad_complete_night
  → summary (spent, people, trust)
  → return Apartment
  → AFTER_HOURS
```
