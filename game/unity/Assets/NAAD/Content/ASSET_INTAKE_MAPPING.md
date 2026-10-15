# NAAD Asset Intake & Rename Mapping

**Generated**: 2026-10-13
**Status**: Intake recorded, mapping complete, vendor packs untracked
**Scope**: Maps all 126 IDs in `ENVIRONMENT_MANIFEST.md` to real prefabs that exist in this project.

## 1. Naming Policy (decision of record)

| Asset origin | Naming rule | Location |
|--------------|-------------|----------|
| Vendor / Asset Store packs | **Keep original vendor names. Never rename.** Renaming breaks `.meta` GUIDs, scene references, and vendor updates. | `Assets/<Publisher>/...` (gitignored) |
| NAAD-authored prefabs | **Must follow `ASSET_NAMING_CONVENTION.md`** — `APT_*`, `STR_*`, `SHP_*`, `NCL_*`, `BCH_*`, etc. | `Assets/NAAD/Prefabs/Environment/` |

`Assets/NAAD/Prefabs/Environment/` contains **6 Gate B Phase-1 placeholders** (headless-validated 2026-10-14; see `ENVIRONMENT_MANIFEST.md` header). Any further prefab authored there must carry its manifest Asset ID as its name (for example `APT_Building_Low.prefab`).

The mapping in this document is the bridge: it records which vendor prefab satisfies each manifest ID, so scenes can reference the manifest ID concept while the bytes stay under the vendor's own name.

## 2. Vendor Pack Inventory

All six folders are **gitignored** (see repo-root `.gitignore` → "Vendor / Asset Store packages"). Nothing below is tracked by git.

| Publisher folder (`Assets/...`) | Contents | Size | Prefabs | FBX | Tracked |
|-------------------------------- |----------|-----:|--------:|----:|---------|
| `Brick Project Studio` | Apartment Kit (modular interiors/exteriors, furniture, props) | 352.0 MB | 325 | 36 | No |
| `Versatile Studio Assets` | Demo City (buildings, houses, lamps, streetscape) | 409.7 MB | 51 | 38 | No |
| `PalmTreePack` | Palm tree variants + shaders | 264.3 MB | 9 | 9 | No |
| `Kevin Iglesias` | Low-poly props (see §6 relevance flag) | 48.9 MB | 22 | 84 | No |
| `Marcos Schultz` | `MSACCMobileInputs`, `MSAdvancedCameraController` — **tools, not art** | 10.6 MB | 2 | 1 | No |
| `Awbmecreations` | Vehicles (cars, van, truck, plane) — out-of-manifest | 1.5 MB | 10 | 10 | No |
| **Total** | | **1087.0 MB** | **419** | **178** | **0 tracked** |

NAAD-authored assets under `Assets/NAAD/` total ~0.3 MB and are fully tracked.

### Re-import procedure (required on every fresh clone)

Because these packs are ignored, a fresh clone ships **without them**. To restore:

1. Unity Editor → **Window → Package Manager → My Assets**.
2. Download and import each pack listed above (search by publisher name).
3. Accept the default import roots so paths match §3 exactly — the project expects `Assets/Brick Project Studio/`, `Assets/Versatile Studio Assets/`, `Assets/PalmTreePack/`, etc.
4. Run **NAAD → Scenes → Validate Registered Scenes** (or `NAAD.Editor.NaadScenePipeline.ValidateScenes` in batch) to confirm scene references resolved.

No Asset Store package IDs are recorded in this repository; packs must be located by publisher/product name from the owning account.

## 3. Scene-Critical Dependencies

`Apartment.unity` and `CityStreet.unity` contain `PrefabInstance` records that reference vendor prefabs **by GUID**. These GUIDs are the hard dependency — they are read from each vendor prefab's `.meta` and cross-checked against the `.unity` files.

| Scene | Vendor prefab path (relative to `Assets/`) | GUID | Instances |
|-------|--------------------------------------------|------|-----------|
| `Apartment` | `Brick Project Studio/Apartment Kit/_Prefabs/Structures/Structure 01.prefab` | `49f576a03ece56a4e9c640ed081f6a68` | 1 |
| `CityStreet` | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/mid_house_1.prefab` | `a2dd7ebabc38532429e0b24b1e4b2207` | 2 |
| `CityStreet` | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/office_building_1.prefab` | `658e1637e781b8d4797e43c26338c9ab` | 1 |
| `CityStreet` | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/lamp_pole_single_orange.prefab` | `8dd0dd541acaff5418f32652ee44b2e7` | 2 |
| `CityStreet` | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/demo_city_by_versatile_studio.prefab` | `b64e7a25ece19a147b10b35a23680ac3` | 1 (inactive backdrop) |

**Fresh-clone impact:** without §2 re-import, these five prefabs are absent, so the scenes load with missing prefab references. `NaadScenePipeline.TryInstantiate` already degrades gracefully at *build* time (logs a warning and falls back to a primitive blockout), but the *committed* `.unity` files will still hold unresolvable GUIDs until the packs are re-imported. Scene validation (`ValidateScenes`) checks scene files, build-settings entries, anchors and camera only — it does **not** currently assert vendor prefab presence.

## 4. Manifest → Vendor Mapping (all 126 IDs)

Status legend:

- **CONFIRMED** — a vendor prefab directly fills this manifest slot.
- **PARTIAL** — a component exists but needs a variant, material swap, or scale/proportion work.
- **NONE** — no vendor match; must be NAAD-authored under `Prefabs/Environment/`.

| Category | CONFIRMED | PARTIAL | NONE | Subtotal |
|----------|----------:|--------:|-----:|---------:|
| APT — Apartment | 4 | 3 | 3 | 10 |
| STR — Street | 1 | 0 | 7 | 8 |
| RD — Road | 0 | 0 | 7 | 7 |
| SHP — Shopfront | 0 | 0 | 8 | 8 |
| KSK — Kiosk | 0 | 0 | 5 | 5 |
| UTL — Utility | 0 | 0 | 9 | 9 |
| SGN — Signage | 0 | 0 | 7 | 7 |
| BRR — Barrier | 0 | 1 | 3 | 4 |
| SYU — Suya | 0 | 4 | 6 | 10 |
| NCL — Nightclub | 0 | 1 | 21 | 22 |
| INT — Interior | 15 | 3 | 2 | 20 |
| BCH — Beach | 1 | 1 | 14 | 16 |
| **Total** | **21** | **13** | **92** | **126** |

All vendor paths below are relative to `Assets/` and were verified to exist on disk at time of writing.

### 4.1 APT — Apartment (10)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `APT_Building_Low` | CONFIRMED | `Brick Project Studio/Apartment Kit/_Prefabs/Structures/Structure 01.prefab` (composite; also `.../Apt Build Kit/Exteriors/Special/Ext_apt_01_01.prefab`, `Ext_apt_01_02.prefab`) |
| `APT_Building_Medium` | PARTIAL | Same kit stacked 4–6 storeys via `.../Exteriors/Ext_Apt_01/Ext_Apt_01_Wall_01.prefab` modules |
| `APT_Building_High` | NONE | — |
| `APT_Balcony` | NONE | No balcony prefab exists anywhere in the project (verified: 0 matches) |
| `APT_Window_Small` | CONFIRMED | `.../Exteriors/Ext_Apt_01/Ext_Apt_01_Wall_Window_01.prefab` |
| `APT_Window_Large` | NONE | Kit has no floor-to-ceiling window module |
| `APT_Door_Main` | CONFIRMED | `.../Interiors/Frames Windows Stairs & Doors/DoorFrame_Apt_01.prefab` (+ `DoorFrame_Apt_02/03/04`, `.../Int into Ext/IntExt_apt_DoorFrame_02_01.prefab`) |
| `APT_Stairs_External` | PARTIAL | `.../Interiors/Frames Windows Stairs & Doors/Int_apt_01_Staircase_01.prefab` is an *interior* staircase; exterior variant needs work |
| `APT_Roof_Flat` | CONFIRMED | `.../Exteriors/Ext_Apt_01/Ext_Apt_01_Roof_01.prefab` (+ `_Roof_02`, `_Roof_03`, `_Roof_Corner_01`, `_Roof_Edge_01`, `_Roof_Mid_01`) |
| `APT_Roof_Terrace` | PARTIAL | Roof modules exist; terrace furniture/railings missing |

### 4.2 STR — Street (8)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `STR_Streetlight_Standard` | CONFIRMED | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/lamp_pole_single_orange.prefab` (+ `_blue`, `_pink`, `_white`, `lamp_pole_dual_*`, `lamp_pole_small`) |
| `STR_Sidewalk_Straight` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/StrSidewalkStraight.prefab` (Gate B Phase-1) |
| `STR_Sidewalk_Corner` | NONE | — |
| `STR_Curb_Straight` | NONE | — |
| `STR_Curb_Corner` | NONE | — |
| `STR_Drain_Open` | NONE | — |
| `STR_Drain_Covered` | NONE | — |
| `STR_Streetlight_Decorative` | NONE | — |

### 4.3 RD — Road (7)

| Manifest ID | Status | Vendor / NAAD prefab |
|-------------|--------|----------------------|
| `RD_Surface_Asphalt` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/RdSurfaceAsphalt.prefab` (Gate B Phase-1) |
| `RD_Surface_Paved` | NONE | — |
| `RD_Marking_Center` | NONE | — |
| `RD_Marking_Crosswalk` | NONE | — |
| `RD_Marking_Stop` | NONE | — |
| `RD_Pothole_Small` | NONE | — |
| `RD_Pothole_Large` | NONE | — |

`CityStreet.unity` instantiates 3 `RD_Surface_Asphalt` road tiles (prefab `908f225a47d8c644995e46ec04d1ed46`) alongside the legacy primitive `StreetAsphalt` ground quad. Road geometry also exists baked inside `demo_city_by_versatile_studio.prefab`, which is not modular.

### 4.4 SHP — Shopfront (8)

All **NONE**: `SHP_Front_Glass`, `SHP_Front_Rolling`, `SHP_Front_Mixed`, `SHP_Signage_Box`, `SHP_Signage_Banner`, `SHP_Awning`, `SHP_Door_Single`, `SHP_Door_Double`.

### 4.5 KSK — Kiosk (5)

All **NONE**: `KSK_Structure_Standard`, `KSK_Structure_Large`, `KSK_Counter`, `KSK_Roof_Metal`, `KSK_Roof_Fabric`.

### 4.6 UTL — Utility (9)

All **NONE**: `UTL_Pole_Concrete`, `UTL_Pole_Wood`, `UTL_Cable_Bundle`, `UTL_Transformer`, `UTL_Generator_Portable`, `UTL_Generator_Large`, `UTL_AC_Wall`, `UTL_AC_Standing`, `UTL_Dish_Satellite`.

The vendor `lamp_pole_*` assets are street lighting, not utility poles — do not substitute.

### 4.7 SGN — Signage (7)

All **NONE**: `SGN_Post_Metal`, `SGN_Stop`, `SGN_Yield`, `SGN_Speed`, `SGN_Pedestrian`, `SGN_NoParking`, `SGN_OneWay`.

Note: `RR_Restroom_Men_Sign`, `RR_Restroom_Woman_Sign`, `RR_Restroom_Unisex_Sign` exist in the Apartment Kit but are restroom markers — not traffic signage.

### 4.8 BRR — Barrier (4)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `BRR_Concrete_Jersey` | PARTIAL | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/concrete_block.prefab` (block, not barrier profile) |
| `BRR_Metal_Guardrail` | NONE | — |
| `BRR_TrafficCone` | NONE | — |
| `BRR_Bollard` | NONE | — |

### 4.9 SYU — Suya (10)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `SYU_Seating_Bench` | PARTIAL | `Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/bench.prefab` (park bench; verify wood/material) |
| `SYU_Seating_Chair` | PARTIAL | `Brick Project Studio/.../Furniture/Living Room/Chair_Apt_01.prefab` (indoor chair reused outdoors) |
| `SYU_Table_Rect` | PARTIAL | `.../Furniture/Living Room/Table_Dining_Apt_01.prefab` |
| `SYU_Counter_Wood` | PARTIAL | `.../Furniture/Kitchen/Cabinets/Cabinet_Base_Sink_01.prefab` (needs re-skin) |
| `SYU_Stand_Structure` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/SyuStandStructure.prefab` (Gate B Phase-1) |
| `SYU_Grill_Open` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/SyuGrillOpen.prefab` (Gate B Phase-1) |
| `SYU_Grill_Covered` | NONE | — |
| `SYU_Table_Round` | NONE | — |
| `SYU_Canopy_Fabric` | NONE | — |
| `SYU_Lantern_String` | NONE | — |

### 4.10 NCL — Nightclub (22)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `NCL_Speaker_Large` | PARTIAL | `Brick Project Studio/.../Props/Electronics/Speaker Sub.prefab` |
| `NCL_Entrance_Door_Large` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/NclEntranceDoorLarge.prefab` (Gate B Phase-1) |
| `NCL_Entrance_Rope` | NONE | — |
| `NCL_Entrance_Stanchion` | NONE | — |
| `NCL_Entrance_Canopy` | NONE | — |
| `NCL_Signage_Neon` | NONE | — |
| `NCL_Signage_LED` | NONE | — |
| `NCL_Bouncer_Stand` | NONE | — |
| `NCL_Stage_Platform` | CONFIRMED placeholder | `NAAD/Prefabs/Environment/NclStagePlatform.prefab` (Gate B Phase-1) |
| `NCL_Stage_Railing` | NONE | — |
| `NCL_Stage_Steps` | NONE | — |
| `NCL_Booth_VIP` | NONE | — |
| `NCL_Booth_Standard` | NONE | — |
| `NCL_Table_Hightop` | NONE | — |
| `NCL_Bar_Counter` | NONE | — |
| `NCL_Bar_Shelves` | NONE | — |
| `NCL_Speaker_Monitor` | NONE | — |
| `NCL_Lighting_Truss` | NONE | — |
| `NCL_Lighting_Spot` | NONE | — |
| `NCL_Lighting_Strobe` | NONE | — |
| `NCL_Fog_Machine` | NONE | — |
| `NCL_DanceFloor_LED` | NONE | — |

### 4.11 INT — Interior (20)

Base path for this table: `Brick Project Studio/Apartment Kit/_Prefabs/`.

| Manifest ID | Status | Vendor prefab (relative to that base) |
|-------------|--------|----------------------------------------|
| `INT_Wall_Straight` | CONFIRMED | `Apt Build Kit/Interiors/Walls Frames & Doors/Apt_01/Int_Apt_01_Wall_01.prefab` (+ `_02`, `_03`, `_04`, `Int_Apt_01_Wall_Double_01`) |
| `INT_Wall_Corner` | PARTIAL | Wall `_02`/`_03`/`_04` variants may cover corners — needs art review |
| `INT_Floor_Tile` | CONFIRMED | `Apt Build Kit/Interiors/Flooring & Ceilings/Int_apt_01_Floor_01.prefab` (tile material) |
| `INT_Floor_Wood` | CONFIRMED | `.../Int_apt_02_Floor_01.prefab`, `Int_apt_03_Floor_01.prefab` (wood material) |
| `INT_Ceiling_Flat` | CONFIRMED | `.../Int_apt_01_Ceiling_01.prefab` |
| `INT_Door_Interior` | PARTIAL | `Apt Build Kit/Interiors/Frames Windows Stairs & Doors/DoorFrame_Apt_01.prefab` (+ `_02`–`_04`) — frame only, leaf needs confirmation |
| `INT_Window_Interior` | PARTIAL | Only exterior `Ext_Apt_01_Wall_Window_01.prefab` available |
| `INT_Furniture_Sofa` | CONFIRMED | `Furniture/Living Room/Sofa_Apt_01.prefab` (+ `Sofa_Apt_02`) |
| `INT_Furniture_Chair` | CONFIRMED | `Furniture/Living Room/Chair_Apt_01.prefab` |
| `INT_Furniture_Table` | CONFIRMED | `Furniture/Living Room/Table_Dining_Apt_01.prefab` (+ `Table_Coffee_01`, `Table_End_01/02`, `Table_Side_Apt_01`) |
| `INT_Furniture_Bed` | CONFIRMED | `Furniture/Bedroom/Bed_Apt_01_01.prefab` (+ `Bed_Apt_01_02`, `Bed_Apt_02_01`) |
| `INT_Furniture_Shelf` | CONFIRMED | `Furniture/Kitchen/Cabinets/Cabinet_Shelf_01.prefab` |
| `INT_Kitchen_Counter` | CONFIRMED | `Furniture/Kitchen/Cabinets/Cabinet_Base_Sink_01.prefab` (+ `Cabinet_Wall_DD_01/02/03`, `Cabinet_Wall_SD_01`) |
| `INT_Kitchen_Stove` | CONFIRMED | `Furniture/Kitchen/Appliances/Stove_01.prefab` |
| `INT_Kitchen_Fridge` | CONFIRMED | `Furniture/Kitchen/Appliances/Fridge_01.prefab` |
| `INT_Lighting_Ceiling` | CONFIRMED | `Props/Lighting/Lamp_Ceiling_Apt_01.prefab` (+ `_02`–`_05`) |
| `INT_Lighting_Table` | CONFIRMED | `Props/Lighting/Lamp_Table_Apt_01.prefab` (+ `_02`; also `Lamp_Floor_Apt_01/02`) |
| `INT_Decor_Rug` | CONFIRMED | `Props/Art/Rug_Apt_01.prefab` (+ `Rug_Apt_02`) |
| `INT_Decor_Plant` | NONE | — |
| `INT_Decor_Picture` | NONE | — |

Bathroom fixtures also exist but are not in the manifest: `Toilet_Apt_01`, `Shower_Base_01`, `TowelRack_Apt_01/02`, `RR_Sink_01` (all under the same base path).

### 4.12 BCH — Beach (16)

| Manifest ID | Status | Vendor prefab |
|-------------|--------|---------------|
| `BCH_Palm_Tree` | CONFIRMED | `PalmTreePack/Prefabs/1/Palm_1_1.prefab`, `PalmTreePack/Prefabs/2/Palm_2_1.prefab` |
| `BCH_Palm_Sapling` | PARTIAL | `PalmTreePack/Prefabs/.../Palm_1_2.prefab`, `Palm_1_3`, `Palm_1_4`, `Palm_2_2`, `Palm_2_3` (variants — confirm relative scale) |
| `BCH_Sand_Surface` | NONE | — |
| `BCH_Water_Surface` | NONE | — |
| `BCH_Shoreline_Wet` | NONE | — |
| `BCH_Shoreline_Dry` | NONE | — |
| `BCH_Cabana_Structure` | NONE | — |
| `BCH_Cabana_Floor` | NONE | — |
| `BCH_Cabana_Fabric` | NONE | — |
| `BCH_Lounger_Chair` | NONE | — |
| `BCH_Lounger_Umbrella` | NONE | — |
| `BCH_Jetty_Wood` | NONE | — |
| `BCH_Jetty_Piling` | NONE | — |
| `BCH_Jetty_Railing` | NONE | — |
| `BCH_Buoy` | NONE | — |
| `BCH_Lifeguard_Stand` | NONE | — |

## 5. Out-of-Manifest Vendor Assets

These exist in the packs but have no manifest slot. They are recorded so nobody re-imports blind:

| Asset | Vendor prefab | Suggested use |
|-------|---------------|---------------|
| City mega-prefab | `Versatile Studio Assets/.../demo_city_by_versatile_studio.prefab` | Distant backdrop only (already placed inactive in `CityStreet`) — not modular, do not use for blockouts |
| Houses | `mid_house_1..5`, `mid_house_1_2`, `mid_house_4_2`, `small_house_1..3` | Mainland street dressing; `mid_house_1` + `office_building_1` already in `CityStreet` |
| Offices | `office_building_1..4` (+ `_bgr`, `_with_base` variants) | Island / Creative district skylines |
| Industrial | `factory_building_big`, `factory_building_small`, `factory_chimney-stalk`, `highway_support` | Creative district |
| Vegetation | `tree_1`, `tree_2`, `tree_3`, `hedge_with_base` | Street greenery (manifest has no generic-tree slot) |
| Vehicles | `Awbmecreations` — `Classic Car_9`, `Hatchback Car_15`, `N Van_10`, `Pick Up_11`, `Police Car N_4`, `Sport Car_39`, `N_Muscle Car_10`, `Monster Truck_12`, `Military Vehicle_3`, `Air Plane_1` | Traffic dressing; manifest has `TRF`/`SGN` codes but no vehicle row — add manifest rows if these become required |
| Street furniture | `bench`, `concrete_block`, `building_lamp_blue/orange/pink/white` | See partial mappings in §4 |

## 6. Relevance Flag — Kevin Iglesias pack

`Assets/Kevin Iglesias` (48.9 MB, 22 prefabs) contains **medieval/fantasy** props: `Human_Anvil`, `Human_Farm`, `Human_GoldOre`, `Human_FallenTree`, `Human_BuildingCamp`, `Human_Axe`, `Human_Sword`, `Human_Plow`, plus `HumanM_CrafterVillager` / `HumanF_CrafterVillager` characters. These do **not** fit contemporary Lagos. Recommend treating the pack as out-of-scope for environment art and re-homing or removing it in a later cleanup, rather than mapping it to any manifest ID.

## 7. Gap Summary / Next Actions

1. **92 of 126 manifest IDs (73%) have no vendor source** — concentrated in roads (7/7), shopfronts (8/8), kiosks (5/5), utilities (9/9), signage (7/7), and nightclub (21/22). These are the NAAD-authored backlog under `Prefabs/Environment/`.
2. **Phase 1 MVP blockers with no vendor match:** `SYU_Stand_Structure`, `SYU_Grill_Open`, `NCL_Entrance_Door_Large`, `NCL_Stage_Platform`, `RD_Surface_Asphalt`, `STR_Sidewalk_Straight`.
3. **Highest-value existing coverage:** the interior set (15 CONFIRMED) fully outfits the Apartment scene.
4. **Document the re-import requirement** (§2) wherever a fresh-clone setup is described — scene integrity depends on it.
5. **Vendor-presence check (implemented):** `NaadScenePipeline.ValidateVendorIntake` (menu `NAAD/Scenes/Validate Vendor Intake`, or CLI `-executeMethod NAAD.Editor.NaadScenePipeline.ValidateVendorIntake`) reports the six pack roots and the five scene-critical §3 prefabs. It is **non-fatal by design** — it warns instead of throwing, so a fresh clone without the gitignored vendor packs still passes batchmode verify. It is deliberately *not* wired into `ValidateScenes`, which keeps failing only on real scene/build errors.

## 8. Maintenance

- Re-run this mapping whenever a vendor pack is added, upgraded or removed, or when `ENVIRONMENT_MANIFEST.md` gains rows.
- Update a row's Status only after verifying the prefab path exists on disk — do not mark CONFIRMED from memory.
- Keep counts in sync with §4: totals must sum to the manifest row count (126 at time of writing).