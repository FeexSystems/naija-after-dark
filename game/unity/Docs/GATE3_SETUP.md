# Gate 3 — Unity setup checklist

## In Unity Editor

1. Create **3D URP** project (LTS) with `Assets/NAAD` present.
2. Create scene `Assets/NAAD/Content/Bootstrap.unity` (or `Assets/Scenes/Bootstrap.unity`).
3. Empty GameObject → add `NAADBootstrap`.
4. Optional: empty GameObject → add `NAADApplicationRoot` (Bootstrap will create one if missing).
5. File → Build Settings → add Bootstrap scene at index 0.
6. Optional: add empty `Main` scene so SceneLoader has a target (or leave stub skip).
7. Press Play.

Expected Console logs:

```
[NAAD][Root] Application root composed (stub services)
[NAAD][Bootstrap] BOOT
[NAAD][Bootstrap] AUTH
[NAAD][State] Phase → Auth
...
[NAAD][Bootstrap] PLAYER
[NAAD][Bootstrap] WORLD
[NAAD][Bootstrap] SCENE
[NAAD][Bootstrap] Bootstrap sequence complete
```

## Pass condition

Unity launches **BOOT → AUTH → PLAYER → WORLD → SCENE** without errors (stubs OK for Gate 3).

Real Supabase auth is **Gate 4**.
