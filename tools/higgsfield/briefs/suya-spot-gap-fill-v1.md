# Suya Spot — Higgsfield Generation Brief v1

**Target Unity scene:** `Assets/NAAD/Scenes/SuyaSpot.unity`  
**District:** Mainland (`ML`)  
**Venue code:** `SYU`  
**Visual direction:** grounded contemporary Nigerian street realism, stylized-real 3D, consistent with the existing NAAD low-poly/mobile content pack.  
**Coordinate convention:** metres, Y-up, forward +Z.  
**Output:** separate, reusable GLB static meshes unless the asset specifically requires a rig. Do not return a full scene as one combined mesh.

## Before generating

Inspect the existing tracked model inventory in `game/unity/Assets/NAAD/Content/Models/naad_asset_manifest.json`. Do not regenerate the existing suya shell, vendor characters, grill, cart, stools, tables, skewers or other already-covered props. This brief only fills gaps.

## Shared constraints

- Mobile-first topology; prefer clean, low-poly silhouettes and simple materials.
- Real-world scale, clean transforms, origin at logical placement point, no negative scale.
- Separate moving/interactive parts from static geometry.
- No embedded lights, cameras, particle systems, scripts, colliders or baked scene dependencies.
- Use texture atlases where useful; avoid unnecessary 4K textures.
- No brand logos, watermarks, invented readable text or culturally inaccurate signage.
- Keep meshes manifold where practical; avoid hidden internal geometry.
- Provide front, side and three-quarter preview renders and disclose approximate triangle count.
- Export GLB; preserve source project file if available. Do not claim a rig or animation is production-ready without a separate Unity retarget/import test.

## Individual prompts

### 1. `SYU_CanopyFrame_ML_v001`
Create a modular open-sided steel canopy frame for a compact Nigerian roadside suya stand, footprint approximately 4m × 6m, height 2.8m. Four to six practical steel support posts, cross-members and simple attachment points for a removable canopy. No fabric, grill, counter, ground plane or surrounding street. One mesh per logical frame assembly, real-world scale, pivot at centre of footprint.

### 2. `SYU_CanopyFabric_ML_v001`
Create a separate weathered red-and-cream striped fabric canopy fitted to a 4m × 6m suya stand frame. Subtle sag and folds, restrained fabric thickness, simple UVs. No metal supports, no signage, no lighting, no background.

### 3. `PRP_MenuBoard_ML_v001`
Create a freestanding hand-painted menu board used at a Nigerian suya stand. Wood or painted metal frame, blank dark writing surface with no generated words or prices. Keep the writing surface as a separate material slot so Unity can render localized menu content.

### 4. `PRP_SpiceTinSet_ML_v001`
Create a reusable set of small stainless-steel spice tins and shallow bowls for a suya preparation counter. Separate the containers into a small number of meshes for layout variation. No labels or legible text.

### 5. `PRP_Tongs_ML_v001`
Create one pair of stainless-steel grilling tongs with believable handheld scale and a simple hinge. Static prop; no rig. Clean silhouette and modest detail.

### 6. `PRP_CharcoalEmbers_ML_v001`
Create a shallow removable charcoal/ember bed insert sized to fit the existing NAAD grill. Geometry only, with separate dark charcoal and ember material regions. Do not create smoke, flames, lights or particle systems; those are Unity effects.

### 7. `PRP_MonoblocChair_ML_v001`
Create a single everyday Nigerian plastic monobloc chair, stackable silhouette, real-world dimensions and mobile-friendly topology. No person, no floor, no table.

### 8. `PRP_PlateSuya_ML_v001`
Create a static food presentation prop: serving plate with grilled suya skewers, sliced onions and tomatoes. Realistic food forms but restrained geometry and texture memory. No packaging, labels or text.

## Acceptance checklist

An export is not approved merely because it looks good in a Higgsfield preview. Each file must pass:
1. Correct naming and matching manifest category/district.
2. File opens and imports cleanly.
3. Scale/orientation/pivot inspection in Unity.
4. Mesh, UV, material and texture inspection.
5. Triangle and texture budget review on target Android tier.
6. Collider/LOD decision documented; add Unity colliders in Unity rather than baking them into art meshes unless justified.
7. Rights/usage evidence recorded.
8. Preview reviewed in the actual SuyaSpot scene with existing assets.
9. Existing interaction points and scene references remain intact.
10. Unity Editor compile, scene validation and Android smoke build pass.

## Prompt discipline

Generate one asset at a time, using the exact asset name as the job label where possible. Record the Higgsfield job ID and prompt version in the manifest. Do not generate the full pack as one combined output.
