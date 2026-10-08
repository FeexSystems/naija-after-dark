# Gate 16 — Mobile optimization checklist

Do **not** optimize blindly. Measure first.

## Devices

- [ ] Low-end Android
- [ ] Mid-range Android
- [ ] High-end Android
- [ ] iPhone

## Metrics

| Metric | Target (initial) | Measured |
|--------|------------------|----------|
| FPS (district) | ≥ 30 | |
| RAM | < 1.2 GB | |
| Cold start | < 8 s | |
| Scene load | < 3 s | |
| APK/AAB size | track | |
| Battery / 30 min | track | |
| Network RTT commands | < 500 ms p95 | |

## Rules

1. Profile before changing art/code.
2. Keep URP mobile-friendly settings.
3. No server secrets in client builds.
4. Prefer texture/atlas and LODs only after data justifies them.
