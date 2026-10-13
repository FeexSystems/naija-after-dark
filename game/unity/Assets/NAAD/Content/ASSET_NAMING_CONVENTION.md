# NAAD Environment Asset Naming Convention

## Overview

This document establishes a consistent naming convention for all environment assets in the NAAD project. Consistent naming ensures clarity, reduces errors, and improves workflow efficiency.

## Naming Structure

### Format
`[Category]_[Subcategory]_[Variant]_[Detail]_[Suffix]`

### Components
- **Category**: High-level asset type (e.g., APT, STR, SHP)
- **Subcategory**: Specific asset within category (e.g., Building, Window, Door)
- **Variant**: District or style variant (e.g., Mainland, Island, Creative, Coastal)
- **Detail**: Additional specification (e.g., Small, Large, Open, Closed)
- **Suffix**: Asset type (e.g., FBX, Prefab, Mat)

## Category Codes

### Residential
- **APT**: Apartment
- **RES**: Residential (general)

### Streets
- **STR**: Street
- **RD**: Road
- **CUR**: Curb
- **SWK**: Sidewalk

### Commercial
- **SHP**: Shop
- **KSK**: Kiosk
- **MKT**: Market

### Utilities
- **UTL**: Utility
- **PWR**: Power
- **WTR**: Water

### Traffic
- **SGN**: Signage
- **BRR**: Barrier
- **TRF**: Traffic

### Food
- **SYU**: Suya
- **FOD**: Food (general)
- **BAR**: Bar

### Nightlife
- **NCL**: Nightclub
- **BAR**: Bar (nightlife)
- **STG**: Stage

### Interior
- **INT**: Interior
- **FRN**: Furniture
- **DEC**: Decor

### Beach
- **BCH**: Beach
- **OCE**: Ocean
- **PAL**: Palm

## Subcategory Examples

### Apartment (APT)
- `APT_Building` - Building structure
- `APT_Balcony` - Balcony module
- `APT_Window` - Window
- `APT_Door` - Door
- `APT_Roof` - Roof
- `APT_Stairs` - Stairs

### Street (STR)
- `STR_Sidewalk` - Sidewalk module
- `STR_Curb` - Curb
- `STR_Drain` - Drain
- `STR_Streetlight` - Street light

### Shop (SHP)
- `SHP_Front` - Shopfront
- `SHP_Signage` - Signage
- `SHP_Awning` - Awning
- `SHP_Door` - Door

### Utility (UTL)
- `UTL_Pole` - Utility pole
- `UTL_Cable` - Cable
- `UTL_Generator` - Generator
- `UTL_AC` - Air conditioner

### Signage (SGN)
- `SGN_Post` - Sign post
- `SGN_Stop` - Stop sign
- `SGN_Speed` - Speed limit sign

### Suya (SYU)
- `SYU_Stand` - Stand structure
- `SYU_Grill` - Grill
- `SYU_Counter` - Counter
- `SYU_Seating` - Seating

### Nightclub (NCL)
- `NCL_Entrance` - Entrance
- `NCL_Stage` - Stage
- `NCL_Booth` - Booth
- `NCL_Bar` - Bar
- `NCL_Lighting` - Lighting

### Interior (INT)
- `INT_Wall` - Wall
- `INT_Floor` - Floor
- `INT_Ceiling` - Ceiling
- `INT_Door` - Door
- `INT_Window` - Window
- `INT_Furniture` - Furniture

### Beach (BCH)
- `BCH_Sand` - Sand surface
- `BCH_Water` - Water surface
- `BCH_Cabana` - Cabana
- `BCH_Palm` - Palm tree
- `BCH_Jetty` - Jetty

## Variant Codes

### District Variants
- **ML**: Mainland
- **IS**: Island
- **CR**: Creative
- **CS**: Coastal

### Style Variants
- **STD**: Standard
- **PRE**: Premium
- **BSC**: Basic
- **IND**: Industrial
- **TRD**: Traditional
- **MOD**: Modern

### Size Variants
- **S**: Small
- **M**: Medium
- **L**: Large
- **XL**: Extra Large

### State Variants
- **OPN**: Open
- **CLS**: Closed
- **DMG**: Damaged
- **NEW**: New
- **WTH**: Weathered

## Detail Codes

### Window Details
- **Sml**: Small
- **Lrg**: Large
- **Dbl**: Double
- **Pnl**: Paneled
- **Gls**: Glass

### Door Details
- **Sng**: Single
- **Dbl**: Double
- **Sld**: Sliding
- **Gls**: Glass
- **Mtl**: Metal
- **Wd**: Wood

### Material Details
- **Cnc**: Concrete
- **Brk**: Brick
- **Mtl**: Metal
- **Wd**: Wood
- **Gls**: Glass
- **Fbr**: Fabric

## Suffix Codes

### Asset Types
- **FBX**: 3D model file
- **OBJ**: 3D model file (alternative)
- **Prefab**: Unity prefab
- **Mat**: Material
- **Tex**: Texture
- **Mesh**: Mesh asset

### LOD Levels
- **LOD0**: Highest detail
- **LOD1**: Medium detail
- **LOD2**: Low detail
- **LOD3**: Lowest detail

## Complete Examples

### Apartment Assets
- `APT_Building_ML_STD_FBX` - Mainland standard apartment building (FBX)
- `APT_Building_ML_STD_Prefab` - Mainland standard apartment building (Prefab)
- `APT_Window_Sml_Gls_Mat` - Small glass window material
- `APT_Balcony_IS_MOD_Prefab` - Island modern balcony prefab
- `APT_Door_Dbl_Mtl_Prefab` - Double metal door prefab

### Street Assets
- `STR_Sidewalk_Straight_STD_FBX` - Straight sidewalk (FBX)
- `STR_Curb_Corner_STD_FBX` - Corner curb (FBX)
- `STR_Streetlight_IS_DEC_Prefab` - Island decorative streetlight prefab
- `STR_Drain_Open_STD_FBX` - Open drain (FBX)

### Shop Assets
- `SHP_Front_Gls_ML_Prefab` - Mainland glass shopfront prefab
- `SHP_Signage_Box_IS_Prefab` - Island box signage prefab
- `SHP_Awning_Fabric_ML_Prefab` - Mainland fabric awning prefab

### Utility Assets
- `UTL_Pole_Concrete_ML_FBX` - Mainland concrete pole (FBX)
- `UTL_Generator_Portable_ML_Prefab` - Mainland portable generator prefab
- `UTL_AC_Wall_IS_Prefab` - Island wall AC unit prefab

### Suya Assets
- `SYU_Stand_ML_TRD_Prefab` - Mainland traditional suya stand prefab
- `SYU_Grill_Open_ML_Prefab` - Mainland open grill prefab
- `SYU_Seating_Bench_ML_Prefab` - Mainland bench seating prefab

### Nightclub Assets
- `NCL_Entrance_IS_PRE_Prefab` - Island premium entrance prefab
- `NCL_Stage_Platform_IS_Prefab` - Island stage platform prefab
- `NCL_Booth_VIP_IS_Prefab` - Island VIP booth prefab
- `NCL_Bar_Counter_IS_Prefab` - Island bar counter prefab

### Interior Assets
- `INT_Wall_Straight_STD_FBX` - Straight wall (FBX)
- `INT_Floor_Tile_STD_Mat` - Tile floor material
- `INT_Furniture_Sofa_STD_Prefab` - Standard sofa prefab
- `INT_Lighting_Ceiling_STD_Prefab` - Standard ceiling light prefab

### Beach Assets
- `BCH_Sand_Surface_CS_FBX` - Coastal sand surface (FBX)
- `BCH_Cabana_Structure_CS_Prefab` - Coastal cabana structure prefab
- `BCH_Palm_Tree_CS_Prefab` - Coastal palm tree prefab
- `BCH_Jetty_Wood_CS_Prefab` - Coastal wooden jetty prefab

## Directory Structure

### Root Structure
```
Assets/NAAD/Content/
├── Models/
│   ├── Apartment/
│   ├── Street/
│   ├── Commercial/
│   ├── Utilities/
│   ├── Traffic/
│   ├── Food/
│   ├── Nightlife/
│   ├── Interior/
│   └── Beach/
├── Materials/
│   ├── Apartment/
│   ├── Street/
│   ├── Commercial/
│   ├── Utilities/
│   ├── Traffic/
│   ├── Food/
│   ├── Nightlife/
│   ├── Interior/
│   └── Beach/
├── Prefabs/
│   ├── Apartment/
│   ├── Street/
│   ├── Commercial/
│   ├── Utilities/
│   ├── Traffic/
│   ├── Food/
│   ├── Nightlife/
│   ├── Interior/
│   └── Beach/
├── Textures/
│   ├── Apartment/
│   ├── Street/
│   ├── Commercial/
│   ├── Utilities/
│   ├── Traffic/
│   ├── Food/
│   ├── Nightlife/
│   ├── Interior/
│   └── Beach/
└── Scenes/
    ├── Blockout/
    ├── Mainland/
    ├── Island/
    ├── Creative/
    └── Coastal/
```

### Prefab Organization

#### Modular Prefabs
- Single reusable components (e.g., window, door, chair)
- Named with full convention
- Stored in appropriate category folder

#### Composite Prefabs
- Assembled from modular prefabs (e.g., building with windows)
- Named with full convention
- Include `_Comp` suffix to indicate composite

#### Scene Prefabs
- Complete environment blocks (e.g., street section)
- Named with district prefix
- Stored in Scenes folder

## Material Naming

### Format
`MAT_[Category]_[Material]_[Variant]`

### Examples
- `MAT_Concrete_Grey_STD` - Standard grey concrete
- `MAT_Concrete_Weathered_ML` - Mainland weathered concrete
- `MAT_Brick_Red_STD` - Standard red brick
- `MAT_Metal_Raw_STD` - Standard raw metal
- `MAT_Wood_Dark_STD` - Standard dark wood
- `MAT_Glass_Clear_STD` - Standard clear glass
- `MAT_Neon_Pink_IS` - Island pink neon

## Texture Naming

### Format
`TEX_[Category]_[Material]_[Type]_[Resolution]`

### Types
- **Albedo**: Color map
- **Normal**: Normal map
- **Rough**: Roughness map
- **Metal**: Metallic map
- **AO**: Ambient occlusion map
- **Emiss**: Emissive map

### Examples
- `TEX_Concrete_Grey_Albedo_2K` - 2K concrete albedo
- `TEX_Concrete_Grey_Normal_2K` - 2K concrete normal
- `TEX_Brick_Red_Albedo_1K` - 1K brick albedo
- `TEX_Neon_Pink_Emiss_512` - 512px neon emissive

## LOD Naming

### Format
`[AssetName]_LOD[N]`

### Examples
- `APT_Building_ML_STD_LOD0` - Highest detail
- `APT_Building_ML_STD_LOD1` - Medium detail
- `APT_Building_ML_STD_LOD2` - Low detail
- `APT_Building_ML_STD_LOD3` - Lowest detail

## Version Control

### Version Suffixes
When iterating on assets, use version suffixes:
- `v01` - First version
- `v02` - Second version
- etc.

### Example
- `APT_Building_ML_STD_v01_Prefab`
- `APT_Building_ML_STD_v02_Prefab`

## Special Cases

### Collision Meshes
- Append `_Col` to name
- Example: `APT_Building_ML_STD_Col_FBX`

### Proxy Meshes
- Append `_Proxy` to name
- Example: `APT_Building_ML_STD_Proxy_FBX`

### Trigger Volumes
- Append `_Trig` to name
- Example: `STR_Door_ML_STD_Trig_Prefab`

## Best Practices

### Do
- Use consistent capitalization (PascalCase for readability)
- Keep names descriptive but concise
- Use underscores to separate components
- Follow the established category codes
- Update naming convention when adding new categories

### Don't
- Use spaces in filenames
- Use special characters (except underscores)
- Use ambiguous abbreviations
- Mix naming conventions
- Create duplicate names

## Implementation Checklist

- [ ] All team members trained on naming convention
- [ ] Asset templates created with correct naming
- [ ] Directory structure established
- [ ] Naming convention documented in project wiki
- [ ] Regular audits for naming compliance
- [ ] Automated naming validation (if possible)
- [ ] Legacy assets renamed to match convention
