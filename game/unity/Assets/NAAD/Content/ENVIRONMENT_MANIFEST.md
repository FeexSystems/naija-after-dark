# NAAD Nigerian Megacity Environment Manifest

## Audit Results

**Current State** (as of 2026-10-14, Gate B Phase-1 validated headless):
- NAAD-authored environment prefabs: **6 placeholders** in `Assets/NAAD/Prefabs/Environment/` (`RdSurfaceAsphalt`, `StrSidewalkStraight`, `SyuStandStructure`, `SyuGrillOpen`, `NclEntranceDoorLarge`, `NclStagePlatform`), tagged `NAAD.World.NaAdPlaceholderTag`, wired into `CityStreet.unity` via `NaadScenePipeline` (10 instances), `BuildApartmentAndCityStreet` + `ValidateScenes` PASS in Unity 6000.6.5f1 batchmode
- Vendor packs imported locally: **6 packs, ~1087 MB, 419 prefabs** (all gitignored; see `ASSET_INTAKE_MAPPING.md` §2)
- Environment scenes: **2 of 7** blockouts authored (`Apartment`, `CityStreet`) plus `Bootstrap`
- Content/ directory: **10 documents** (was empty when this manifest was first written)

> The original audit below recorded the pre-intake state. For the current per-asset
> mapping of these 126 IDs to real vendor prefabs, see **`ASSET_INTAKE_MAPPING.md`**.

Original audit (superseded):
**Current State**: No existing environment assets.
- No 3D models (FBX, OBJ, etc.)
- No materials
- No environment prefabs
- No environment scenes
- Content/ directory is empty

## Visual Target

**Inspiration**: Contemporary Lagos, Nigeria  
**Style**: Cinematic realism with mobile-friendly rendering  
**Scale**: Compact, connected environments (not geographically exact map)

## Modular Asset Family

### 1. Residential Streets and Apartment Buildings

#### Apartment Buildings
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `APT_Building_Low` | Low-Rise Apartment | 2-3 story residential building | HIGH | MISSING |
| `APT_Building_Medium` | Medium-Rise Apartment | 4-6 story residential building | HIGH | MISSING |
| `APT_Building_High` | High-Rise Apartment | 7+ story residential building | MEDIUM | MISSING |
| `APT_Balcony` | Balcony Module | Protruding balcony with railing | HIGH | MISSING |
| `APT_Window_Small` | Small Window | Standard residential window | HIGH | MISSING |
| `APT_Window_Large` | Large Window | Floor-to-ceiling window | MEDIUM | MISSING |
| `APT_Door_Main` | Main Entrance Door | Apartment building entrance | HIGH | MISSING |
| `APT_Stairs_External` | External Stairs | Staircase to upper floors | MEDIUM | MISSING |
| `APT_Roof_Flat` | Flat Roof | Standard flat roof module | HIGH | MISSING |
| `APT_Roof_Terrace` | Roof Terrace | Accessible rooftop space | LOW | MISSING |

#### Residential Streets
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `STR_Sidewalk_Straight` | Straight Sidewalk | Standard sidewalk module | HIGH | CONFIRMED placeholder (`Prefabs/Environment/StrSidewalkStraight.prefab`) |
| `STR_Sidewalk_Corner` | Corner Sidewalk | Curved sidewalk for corners | HIGH | MISSING |
| `STR_Curb_Straight` | Straight Curb | Road curb module | HIGH | MISSING |
| `STR_Curb_Corner` | Corner Curb | Curbed corner | HIGH | MISSING |
| `STR_Drain_Open` | Open Drain | Open roadside drain | MEDIUM | MISSING |
| `STR_Drain_Covered` | Covered Drain | Covered roadside drain | MEDIUM | MISSING |
| `STR_Streetlight_Standard` | Standard Streetlight | Pole-mounted street light | HIGH | MISSING |
| `STR_Streetlight_Decorative` | Decorative Streetlight | Ornamental street light | LOW | MISSING |

### 2. Shopfronts and Kiosks

#### Shopfronts
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `SHP_Front_Glass` | Glass Shopfront | Full glass storefront | HIGH | MISSING |
| `SHP_Front_Rolling` | Rolling Shutter | Metal rolling shutter | HIGH | MISSING |
| `SHP_Front_Mixed` | Mixed Shopfront | Glass + solid sections | HIGH | MISSING |
| `SHP_Signage_Box` | Box Signage | Illuminated box sign | HIGH | MISSING |
| `SHP_Signage_Banner` | Banner Signage | Fabric banner sign | MEDIUM | MISSING |
| `SHP_Awning` | Shop Awning | Fabric awning over entrance | MEDIUM | MISSING |
| `SHP_Door_Single` | Single Shop Door | Standard shop entrance | HIGH | MISSING |
| `SHP_Door_Double` | Double Shop Door | Wide shop entrance | MEDIUM | MISSING |

#### Kiosks
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `KSK_Structure_Standard` | Standard Kiosk | Small standalone kiosk | HIGH | MISSING |
| `KSK_Structure_Large` | Large Kiosk | Bigger kiosk structure | MEDIUM | MISSING |
| `KSK_Counter` | Kiosk Counter | Service counter module | HIGH | MISSING |
| `KSK_Roof_Metal` | Metal Roof | Corrugated metal roof | HIGH | MISSING |
| `KSK_Roof_Fabric` | Fabric Roof | Tarp/fabric canopy | MEDIUM | MISSING |

### 3. Roads, Curbs, Drains and Sidewalks

#### Roads
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `RD_Surface_Asphalt` | Asphalt Road | Standard asphalt road surface | HIGH | CONFIRMED placeholder (`Prefabs/Environment/RdSurfaceAsphalt.prefab`) |
| `RD_Surface_Paved` | Paved Road | Brick/paved road surface | MEDIUM | MISSING |
| `RD_Marking_Center` | Center Line | Road center line marking | HIGH | MISSING |
| `RD_Marking_Crosswalk` | Crosswalk | Pedestrian crossing marking | HIGH | MISSING |
| `RD_Marking_Stop` | Stop Line | Stop line marking | HIGH | MISSING |
| `RD_Pothole_Small` | Small Pothole | Minor road damage detail | LOW | MISSING |
| `RD_Pothole_Large` | Large Pothole | Major road damage detail | LOW | MISSING |

### 4. Utility Poles, Cables, Generators and Air Conditioners

#### Utilities
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `UTL_Pole_Concrete` | Concrete Pole | Utility pole (concrete) | HIGH | MISSING |
| `UTL_Pole_Wood` | Wooden Pole | Utility pole (wood) | MEDIUM | MISSING |
| `UTL_Cable_Bundle` | Cable Bundle | Group of utility cables | HIGH | MISSING |
| `UTL_Transformer` | Transformer Box | Electrical transformer | MEDIUM | MISSING |
| `UTL_Generator_Portable` | Portable Generator | Small backup generator | HIGH | MISSING |
| `UTL_Generator_Large` | Large Generator | Industrial generator | MEDIUM | MISSING |
| `UTL_AC_Wall` | Wall AC Unit wall-mounted | Wall-mounted air conditioner | HIGH | MISSING |
| `UTL_AC_Standing` | Standing AC Unit | Floor-standing AC unit | MEDIUM | MISSING |
| `UTL_Dish_Satellite` | Satellite Dish | Roof-mounted satellite dish | MEDIUM | MISSING |

### 5. Traffic Signage and Barriers

#### Signage
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `SGN_Post_Metal` | Metal Sign Post | Standard sign post | HIGH | MISSING |
| `SGN_Stop` | Stop Sign | Stop traffic sign | HIGH | MISSING |
| `SGN_Yield` | Yield Sign | Yield traffic sign | MEDIUM | MISSING |
| `SGN_Speed` | Speed Limit Sign | Speed limit sign | MEDIUM | MISSING |
| `SGN_Pedestrian` | Pedestrian Sign | Pedestrian crossing sign | MEDIUM | MISSING |
| `SGN_NoParking` | No Parking Sign | No parking sign | MEDIUM | MISSING |
| `SGN_OneWay` | One Way Sign | Directional sign | MEDIUM | MISSING |

#### Barriers
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `BRR_Concrete_Jersey` | Jersey Barrier | Concrete traffic barrier | MEDIUM | MISSING |
| `BRR_Metal_Guardrail` | Metal Guardrail | Metal roadside guardrail | MEDIUM | MISSING |
| `BRR_TrafficCone` | Traffic Cone | Orange traffic cone | LOW | MISSING |
| `BRR_Bollard` | Bollard | Security bollard | LOW | MISSING |

### 6. Suya Stands and Outdoor Seating

#### Suya Stands
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `SYU_Stand_Structure` | Suya Stand Structure | Main stand frame | HIGH | CONFIRMED placeholder (`Prefabs/Environment/SyuStandStructure.prefab`) |
| `SYU_Grill_Open` | Open Grill | Open charcoal grill | HIGH | CONFIRMED placeholder (`Prefabs/Environment/SyuGrillOpen.prefab`) |
| `SYU_Grill_Covered` | Covered Grill | Grill with metal cover | HIGH | MISSING |
| `SYU_Counter_Wood` | Wooden Counter | Service counter | HIGH | MISSING |
| `SYU_Seating_Bench` | Wooden Bench | Simple wooden bench | HIGH | MISSING |
| `SYU_Seating_Chair` | Plastic Chair | Plastic outdoor chair | HIGH | MISSING |
| `SYU_Table_Round` | Round Table | Small round table | HIGH | MISSING |
| `SYU_Table_Rect` | Rectangular Table | Larger dining table | MEDIUM | MISSING |
| `SYU_Canopy_Fabric` | Fabric Canopy | Shade canopy | MEDIUM | MISSING |
| `SYU_Lantern_String` | String Lanterns | Decorative light string | MEDIUM | MISSING |

### 7. Nightclub Entrance and Stage

#### Nightclub Entrance
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|-------- |
| `NCL_Entrance_Door_Large` | Large Entrance Door | Double glass doors | HIGH | CONFIRMED placeholder (`Prefabs/Environment/NclEntranceDoorLarge.prefab`) |
| `NCL_Entrance_Rope` | Velvet Rope | Queue barrier rope | HIGH | MISSING |
| `NCL_Entrance_Stanchion` | Stanchion Post | Rope support post | HIGH | MISSING |
| `NCL_Entrance_Canopy` | Entrance Canopy | Covered entrance area | HIGH | MISSING |
| `NCL_Signage_Neon` | Neon Signage | Illuminated neon sign | HIGH | MISSING |
| `NCL_Signage_LED` | LED Signage | LED display sign | MEDIUM | MISSING |
| `NCL_Bouncer_Stand` | Bouncer Stand | Security checkpoint | MEDIUM | MISSING |

#### Nightclub Stage
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|-------- |
| `NCL_Stage_Platform` | Stage Platform | Raised stage floor | HIGH | CONFIRMED placeholder (`Prefabs/Environment/NclStagePlatform.prefab`) |
| `NCL_Stage_Railing` | Stage Railing | Safety railing | HIGH | MISSING |
| `NCL_Stage_Steps` | Stage Steps | Access stairs | HIGH | MISSING |
| `NCL_Booth_VIP` | VIP Booth | Seated VIP area | HIGH | MISSING |
| `NCL_Booth_Standard` | Standard Booth | Regular seating booth | HIGH | MISSING |
| `NCL_Table_Hightop` | High-top Table | Tall cocktail table | HIGH | MISSING |
| `NCL_Bar_Counter` | Bar Counter | Service bar | HIGH | MISSING |
| `NCL_Bar_Shelves` | Bar Shelves | Bottle storage | MEDIUM | MISSING |
| `NCL_Speaker_Large` | Large Speaker | PA speaker stack | HIGH | MISSING |
| `NCL_Speaker_Monitor` | Monitor Speaker | Stage monitor | HIGH | MISSING |
| `NCL_Lighting_Truss` | Lighting Truss | Overhead lighting rig | HIGH | MISSING |
| `NCL_Lighting_Spot` | Spot Light | Focused spotlight | HIGH | MISSING |
| `NCL_Lighting_Strobe` | Strobe Light | Strobe effect light | MEDIUM | MISSING |
| `NCL_Fog_Machine` | Fog Machine | Smoke/fog generator | MEDIUM | MISSING |
| `NCL_DanceFloor_LED` | LED Dance Floor | Illuminated floor panels | MEDIUM | MISSING |

### 8. Apartment Interiors

#### Interior Modules
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|-------- |
| `INT_Wall_Straight` | Straight Wall | Standard interior wall | HIGH | MISSING |
| `INT_Wall_Corner` | Corner Wall | Corner wall module | HIGH | MISSING |
| `INT_Floor_Tile` | Tile Floor | Tiled floor surface | HIGH | MISSING |
| `INT_Floor_Wood` | Wood Floor | Wooden floor surface | HIGH | MISSING |
| `INT_Ceiling_Flat` | Flat Ceiling | Standard ceiling | HIGH | MISSING |
| `INT_Door_Interior` | Interior Door | Room door | HIGH | MISSING |
| `INT_Window_Interior` | Interior Window | Room window | HIGH | MISSING |
| `INT_Furniture_Sofa` | Sofa | Living room sofa | HIGH | MISSING |
| `INT_Furniture_Chair` | Chair | Dining/living chair | HIGH | MISSING |
| `INT_Furniture_Table` | Table | Dining/coffee table | HIGH | MISSING |
| `INT_Furniture_Bed` | Bed | Bedroom bed | HIGH | MISSING |
| `INT_Furniture_Shelf` | Shelf | Wall-mounted shelf | MEDIUM | MISSING |
| `INT_Kitchen_Counter` | Kitchen Counter | Kitchen workspace | HIGH | MISSING |
| `INT_Kitchen_Stove` | Stove | Cooking stove | MEDIUM | MISSING |
| `INT_Kitchen_Fridge` | Refrigerator | Kitchen fridge | MEDIUM | MISSING |
| `INT_Lighting_Ceiling` | Ceiling Light | Overhead light fixture | HIGH | MISSING |
| `INT_Lighting_Table` | Table Lamp | Table lamp | MEDIUM | MISSING |
| `INT_Decor_Rug` | Rug | Floor rug/carpet | MEDIUM | MISSING |
| `INT_Decor_Plant` | Potted Plant | Indoor plant | MEDIUM | MISSING |
| `INT_Decor_Picture` | Wall Picture | Framed artwork | LOW | MISSING |

### 9. Beach Resort, Cabanas, Sand, Shoreline, Palm Trees and Jetty

#### Beach Elements
| Asset ID | Name | Description | Priority | Status |
|----------|------|-------------|----------|--------|
| `BCH_Sand_Surface` | Sand Surface | Beach sand terrain | HIGH | MISSING |
| `BCH_Water_Surface` | Water Surface | Ocean water | HIGH | MISSING |
| `BCH_Shoreline_Wet` | Wet Sand | Waterline wet sand | HIGH | MISSING |
| `BCH_Shoreline_Dry` | Dry Sand | Upper beach dry sand | HIGH | MISSING |
| `BCH_Cabana_Structure` | Cabana Structure | Beach cabana frame | HIGH | MISSING |
| `BCH_Cabana_Fabric` | Cabana Fabric | Cabana fabric covering | HIGH | MISSING |
| `BCH_Cabana_Floor` | Cabana Floor | Wooden platform floor | HIGH | MISSING |
| `BCH_Lounger_Chair` | Beach Lounger | Adjustable beach chair | HIGH | MISSING |
| `BCH_Lounger_Umbrella` | Beach Umbrella | Shade umbrella | HIGH | MISSING |
| `BCH_Palm_Tree` | Palm Tree | Tropical palm tree | HIGH | MISSING |
| `BCH_Palm_Sapling` | Palm Sapling | Small palm tree | MEDIUM | MISSING |
| `BCH_Jetty_Wood` | Wooden Jetty | Pier structure | HIGH | MISSING |
| `BCH_Jetty_Railing` | Jetty Railing | Safety railing | HIGH | MISSING |
| `BCH_Jetty_Piling` | Jetty Piling | Support pilings | MEDIUM | MISSING |
| `BCH_Buoy` | Water Buoy | Navigation marker | LOW | MISSING |
| `BCH_Lifeguard_Stand` | Lifeguard Stand | Elevated watch tower | MEDIUM | MISSING |

## Material Requirements

### Base Materials
- `MAT_Concrete_Grey` - Standard concrete
- `MAT_Concrete_Weathered` - Aged concrete
- `MAT_Brick_Red` - Red brick
- `MAT_Brick_Painted` - Painted brick
- `MAT_Metal_Raw` - Raw metal
- `MAT_Metal_Painted` - Painted metal
- `MAT_Metal_Rusted` - Rusted metal
- `MAT_Wood_Dark` - Dark wood
- `MAT_Wood_Light` - Light wood
- `MAT_Glass_Clear` - Clear glass
- `MAT_Glass_Tinted` - Tinted glass
- `MAT_Fabric_Canvas` - Canvas fabric
- `MAT_Fabric_Plastic` - Plastic fabric
- `MAT_Asphalt` - Road asphalt
- `MAT_Sand_Beach` - Beach sand
- `MAT_Water_Ocean` - Ocean water
- `MAT_Foliage_Palm` - Palm foliage
- `MAT_Neon_Color` - Emissive neon material

## Performance Guidelines

### Target Platform
- Mobile-friendly rendering
- Target: 60 FPS on mid-range mobile devices

### Optimization Strategies
- Modular assets with LOD support
- Shared materials where possible
- Texture atlasing for small props
- Instanced rendering for repeated elements
- Occlusion culling for dense areas
- Light baking for static lighting

### Poly Count Targets
- Large buildings: 500-2000 triangles
- Medium props: 100-500 triangles
- Small props: 50-200 triangles
- Total scene target: <100k triangles

### Texture Resolution
- Large buildings: 1024x1024 or 2048x2048
- Medium props: 512x512 or 1024x1024
- Small props: 256x256 or 512x512
- Use texture compression (ASTC/ETC2 for mobile)

## Implementation Priority

### Phase 1: Core Blockouts (MVP)
1. Apartment building exterior and interior
2. Suya spot structure and seating
3. Nightclub entrance and stage
4. Basic road and sidewalk modules
5. Essential utility props

### Phase 2: District Expansion
1. Island nightlife district assets
2. Mainland street assets
3. Creative district assets
4. Coastal beach assets

### Phase 3: Polish and Detail
1. Additional props and details
2. Weathering and variation
3. Lighting profiles
4. Performance optimization
