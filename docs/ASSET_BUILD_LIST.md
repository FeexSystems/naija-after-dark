# NAAD Asset Build List

**Last Updated:** 2026-10-20
**Purpose:** Comprehensive list of 3D assets needed for NAAD vertical slice

## Asset Naming Convention

All assets must follow: `<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>`

**Categories:**
- `APT` - Apartment
- `CHR` - Character
- `INT` - Interior
- `NCL` - Nightclub
- `PRP` - Prop
- `STR` - Street
- `SYU` - Suya
- `BCH` - Beach
- `VHC` - Vehicle

**Districts:**
- `ND` - Night District
- `ML` - Mainland Streets
- `IS` - Island Nightlife
- `CR` - Creative
- `CS` - Coastal

**Sizes (optional):** `S`, `M`, `L`, `XL`

---

## Priority 1: Gate C - Player Character (BLOCKER)

### Character Rig & Animation
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `CHR_Player_ND_v001` | CHR | ND | - | **BLOCKER** | Humanoid rig, Mixamo or custom, full body |
| `CHR_Player_ND_v001` | CHR | ND | - | **BLOCKER** | FBX with avatar setup |

### Animation Clips (MVP Set - 8 required)
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `ANI_Idle_ND_v001` | CHR | ND | - | **BLOCKER** | Idle state, loopable |
| `ANI_Walk_ND_v001` | CHR | ND | - | **BLOCKER** | Forward walk, loopable |
| `ANI_Run_ND_v001` | CHR | ND | - | **BLOCKER** | Forward run, loopable |
| `ANI_TurnLeft_ND_v001` | CHR | ND | - | **BLOCKER** | 90° left turn |
| `ANI_TurnRight_ND_v001` | CHR | ND | - | **BLOCKER** | 90° right turn |
| `ANI_Greet_ND_v001` | CHR | ND | - | **BLOCKER** | Social greeting animation |
| `ANI_Phone_ND_v001` | CHR | ND | - | **BLOCKER** | Using phone interaction |
| `ANI_Dance_ND_v001` | CHR | ND | - | **BLOCKER** | Basic dance move for nightclub |

### Player Prefab Components
- Animator Controller (script-driven parameters)
- Upper Body Mask layer
- CharacterController
- PlayerLocomotionController
- Animation bridge/state manager

---

## Priority 2: Apartment (APT) - Night District

### Interior Props
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `PRP_Bed_ND_v001` | PRP | ND | M | Needed | Player bed, spawn point |
| `PRP_Sofa_ND_v001` | PRP | ND | L | Needed | Living room seating |
| `PRP_Table_ND_v001` | PRP | ND | M | Needed | Coffee/dining table |
| `PRP_Chair_ND_v001` | PRP | ND | S | Needed | Dining chair |
| `PRP_TV_ND_v001` | PRP | ND | M | Needed | Television prop |
| `PRP_Lamp_ND_v001` | PRP | ND | S | Needed | Floor/table lamp |
| `PRP_Wardrobe_ND_v001` | PRP | ND | L | Needed | Storage furniture |
| `PRP_Window_ND_v001` | PRP | ND | M | Needed | Window frame + glass |
| `PRP_Door_ND_v001` | PRP | ND | M | Needed | Apartment door |

### Apartment Structure
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `INT_Apartment_ND_v001` | INT | ND | XL | Placeholder | Room shell, walls, floor, ceiling |

---

## Priority 3: Street (STR) - Mainland Streets

### Street Props
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `PRP_LampPost_ML_v001` | PRP | ML | M | Needed | Street lighting |
| `PRP_Bench_ML_v001` | PRP | ML | M | Needed | Public seating |
| `PRP_Bin_ML_v001` | PRP | ML | S | Needed | Trash/recycling bin |
| `PRP_Signage_ML_v001` | PRP | ML | M | Needed | Street signs, shop signs |
| `PRP_VendorCart_ML_v001` | PRP | ML | M | Needed | Street vendor stall |
| `PRP_TrafficLight_ML_v001` | PRP | ML | M | Needed | Traffic signal |
| `PRP_Pothole_ML_v001` | PRP | ND | S | Needed | Nigerian street detail |

### Street Structure
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `STR_CityStreet_ML_v001` | STR | ML | XL | Placeholder | Road sections, sidewalks, curbs |

---

## Priority 4: Suya Spot (SYU) - Mainland Streets

### Suya Props
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `PRP_Grill_ML_v001` | PRP | ML | M | Built | Suya grill station (764 tris) |
| `PRP_Table_ML_v001` | PRP | ML | M | Built | Outdoor dining table (236 tris) |
| `PRP_Stool_ML_v001` | PRP | ML | S | Built | Tall stool for grill (296 tris) |
| `PRP_Fan_ML_v001` | PRP | ML | M | Built | Ceiling/floor fan (656 tris) |
| `PRP_Cooler_ML_v001` | PRP | ML | S | Built | Drink cooler (252 tris) |
| `PRP_Skewer_ML_v001` | PRP | ML | S | Built | Suya skewer prop (462 tris) |
| `PRP_Napkin_ML_v001` | PRP | ML | S | Built | Napkin holder (152 tris) |

### Suya Structure
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `SYU_SuyaSpot_ML_v001` | SYU | ML | L | Built | Stall structure, parasol, banners (1900 tris) |

---

## Priority 5: Nightclub (NCL) - Island Nightlife

### Nightclub Props
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `PRP_Bar_IS_v001` | PRP | IS | L | Needed | Club bar counter |
| `PRP_BarStool_IS_v001` | PRP | IS | S | Needed | Bar seating |
| `PRP_DanceFloor_IS_v001` | PRP | IS | XL | Needed | LED dance floor |
| `PRP_Speaker_IS_v001` | PRP | IS | M | Needed | Sound system speakers |
| `PRP_DJBooth_IS_v001` | PRP | IS | M | Needed | DJ station |
| `PRP_Table_IS_v001` | PRP | IS | M | Needed | VIP table |
| `PRP_Sofa_IS_v001` | PRP | IS | L | Needed | VIP booth seating |
| `PRP_Laser_IS_v001` | PRP | IS | S | Needed | Laser light fixture |
| `PRP_SmokeMachine_IS_v001` | PRP | IS | S | Needed | Fog effect prop |
| `PRP_BottleService_IS_v001` | PRP | IS | S | Needed | Champagne bottle prop |

### Nightclub Structure
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `NCL_Nightclub_IS_v001` | NCL | IS | XL | Placeholder | Club interior, booths, stage |

---

## Priority 6: Beach (BCH) - Coastal

### Beach Props
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `PRP_PalmTree_CS_v001` | PRP | CS | L | Needed | Palm tree (multiple variants) |
| `PRP_BeachChair_CS_v001` | PRP | CS | M | Needed | Lounge chair |
| `PRP_Umbrella_CS_v001` | PRP | CS | M | Needed | Beach umbrella |
| `PRP_Bonfire_CS_v001` | PRP | CS | M | Needed | Late-night bonfire |
| `PRP_DrinkStand_CS_v001` | PRP | CS | M | Needed | Beach bar stall |
| `PRP_Speaker_CS_v001` | PRP | CS | M | Needed | Portable speaker |
| `PRP_Surfboard_CS_v001` | PRP | CS | L | Needed | Decorative surfboard |

### Beach Structure
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------| b ------|--------|-------|
| `BCH_Beach_CS_v001` | BCH | CS | XL | Placeholder | Sand, water line, shoreline |

---

## Priority 7: NPC Characters (Future)

### Main NPCs
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `CHR_Tunde_ND_v001` | CHR | ND | - | Future | AI NPC, suya spot encounter |
| `CHR_MamaSeyi_ND_v001` | CHR | ND | - | Future | Deterministic NPC |
| `CHR_Amaka_ND_v001` | CHR | ND | - | Future | Deterministic NPC |
| `CHR_Emeka_ND_v001` | CHR | ND | - | Future | Deterministic NPC |
| `CHR_Dami_ND_v001` | CHR | ND | - | Future | Deterministic NPC |

### Crowd NPCs
| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `CHR_Crowd01_ND_v001` | CHR | ND | - | Future | Generic pedestrian |
| `CHR_Crowd02_ND_v001` | CHR | ND | - | Future | Generic pedestrian variant |
| `CHR_Crowd03_ND_v001` | CHR | ND | - | Future | Generic pedestrian variant |

---

## Priority 8: Vehicles (Future)

| Asset Name | Category | District | Size | Status | Notes |
|------------|----------|----------|------|--------|-------|
| `VHC_KekeNapep_ML_v001` | VHC | ML | M | Future | Tricycle taxi |
| `VHC_Bus_ML_v001` | VHC | ML | XL | Future | City bus |
| `VHC_Car_ML_v001` | VHC | ML | L | Future | Private car |

---

## Asset Production Workflow

1. **Model in Blender** - Create low-poly asset
2. **Apply NAAD Naming** - Use `<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>` format
3. **Export FBX** - Via NAAD Blender add-on
4. **Upload to Unity Cloud** - Auto-tagged with district/venue
5. **Import to Unity** - Via intake folder or Unity Cloud sync
6. **Create Prefab** - Set up components, colliders, materials
7. **Register in Scene** - Add to appropriate NAAD scene

---

## Asset Status Legend

- **BLOCKER** - Gate C cannot pass without this
- **Needed** - Required for vertical slice completeness
- **Future** - Planned for post-vertical-slice expansion
- **Placeholder** - Exists as primitive/box placeholder, needs real asset

---

## Export Targets

All assets should export to:
- Primary: `game/unity/Assets/NAAD/Content/Models/`
- Intake: `game/unity/Assets/Diagnostics/AssetImports/` (auto-sync)

Unity Cloud Asset Manager collections:
- `ND/APT` - Night District / Apartment
- `ML/STR` - Mainland / Street
- `ML/SYU` - Mainland / Suya
- `IS/NCL` - Island / Nightclub
- `CS/BCH` - Coastal / Beach

---

## Related Documentation

- [Blender Integration Guide](tools/blender-integration.md)
- [Asset Naming Convention](tools/unity-asset-pipeline/naad_addon/rules.py)
- [Gate C - Player Rig & Animation](GATE_C_PLAYER_RIG_ANIMATION.md)
- [Gate Status](GATE_STATUS.md)
