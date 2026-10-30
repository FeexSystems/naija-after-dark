# MVP Placeholder Animation Clips (Gate C)

These eight `.anim` assets exist so Gate C verification can bind states and enforce
`AnimationClip.length > 0`. They are **not** production Mixamo takes.

| Clip | Path | Role |
|------|------|------|
| Idle | Locomotion/Idle.anim | loop hold |
| Walk | Locomotion/Walk.anim | loop |
| Run | Locomotion/Run.anim | loop |
| Retrieve | Phone/Retrieve.anim | one-shot |
| Hold | Phone/Hold.anim | loop |
| Browse | Phone/Browse.anim | loop |
| PutAway | Phone/PutAway.anim | one-shot |
| Greeting | Social/Greeting.anim | one-shot |

## Replace with Mixamo

1. Download humanoid FBX takes from https://www.mixamo.com (Adobe account).
2. Import into Unity; set Rig = Humanoid.
3. Extract/rename clips onto these exact paths (preserve `.meta` GUIDs when replacing).
4. Re-run `NAAD/Gate C/Configure Player Rig Wiring` and `NAAD/Verify Animation Assets`.

Do not delete the `.meta` files when swapping motion data — GUID stability keeps
AnimatorController references intact.
