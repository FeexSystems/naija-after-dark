# NAAD Scene Setup Guide

This guide explains how to use the editor scripts to set up the NAAD Unity project scenes with all required components.

## Prerequisites

1. Unity Editor must be open with the project loaded
2. Project must compile without errors (should be fixed after recent commits)
3. Run setup scripts in the order listed below

## Phase 1: Core Scene Structure

### SetupScenes.cs

Adds core components to all five base scenes:

**Menu Location:** `NAAD → Setup Scenes`

**Commands:**
- `Setup All Scenes` - Sets up all five scenes at once
- `Setup Apartment Scene` - Individual scene setup
- `Setup CityStreet Scene`
- `Setup SuyaSpot Scene`
- `Setup Nightclub Scene`
- `Setup Beach Scene`

**Components Added:**
- PlayerSpawn GameObject (at position 0,1,0)
- Player GameObject with PlayerLocomotionController
- WorldClockService GameObject
- ThirdPersonCamera (set as MainCamera)
- MobileControls GameObject with MobileTouchControls

**Usage:** Run `NAAD → Setup All Scenes` first to establish the base structure.

---

## Phase 2: Interaction Points

### SetupInteractionPoints.cs

Adds interaction points with Interactable components to each scene.

**Menu Location:** `NAAD → Setup Interaction Points`

**Commands:**
- `All Scenes` - Adds interaction points to all scenes
- `Apartment` - Individual scene setup
- `CityStreet`
- `SuyaSpot`
- `Nightclub`
- `Beach`

**Scene-Specific Interaction Points:**

**Apartment:**
- Door_Apartment_To_Street (Exit to Street)
- Bed (Sleep)
- Phone (Check Messages)
- Fridge (Get Food)

**CityStreet:**
- Door_Street_To_Apartment (Enter Apartment)
- Door_Street_To_SuyaSpot (Enter Suya Spot)
- Door_Street_To_Nightclub (Enter Nightclub)
- Taxi_Stand (Call Taxi)
- Street_Vendor (Talk to Vendor)

**SuyaSpot:**
- Door_SuyaSpot_To_Street (Exit to Street)
- Suya_Stand (Order Suya)
- Table_1, Table_2 (Sit)
- Beverage_Stand (Get Drink)

**Nightclub:**
- Door_Nightclub_To_Street (Exit to Street)
- Bar (Order Drink)
- DJ_Booth (Request Song)
- Dance_Floor_Center (Dance)
- VIP_Area (Enter VIP)
- Restroom (Use Restroom)

**Beach:**
- Beach_Vendor (Buy Refreshments)
- Beach_Chair_1, Beach_Chair_2 (Relax)
- Water_Edge (Swim)
- Beach_Volleyball (Play Volleyball)

**Visual Indicators:** Orange spheres mark interaction points for easy identification.

**Usage:** Run `NAAD → Setup Interaction Points → All Scenes` after core setup.

---

## Phase 3: World Clock Lighting

### SetupWorldClockLighting.cs

Sets up WorldClockPresenter and scene-specific lighting.

**Menu Location:** `NAAD → Setup World Clock Lighting`

**Commands:**
- `All Scenes` - Sets up lighting for all scenes
- `Apartment` - Individual scene setup
- `CityStreet`
- `SuyaSpot`
- `Nightclub`
- `Beach`

**Scene-Specific Lighting:**

**Apartment:**
- Cool ambient lighting
- Directional light (sun/moon)
- NightLightingProfile configuration

**CityStreet:**
- Street lights (5 point lights along street)
- Warm street light color
- Connected to WorldClockService

**SuyaSpot:**
- Warm, inviting point light
- Orange color for suya stand atmosphere

**Nightclub:**
- Dynamic colored lights (red, blue, magenta, cyan)
- Multiple point lights for club atmosphere

**Beach:**
- Natural directional light (sun/moon)
- Bright sunlight configuration

**Usage:** Run `NAAD → Setup World Clock Lighting → All Scenes` after interaction points.

---

## Phase 4: Scene Transitions

### SetupSceneTransitions.cs

Registers scenes in Build Settings and adds SceneLoader components.

**Menu Location:** `NAAD → Setup Scene Transitions`

**Commands:**
- `Register All Scenes` - Adds all 5 scenes to Build Settings
- `Add SceneLoaders to All Scenes` - Adds SceneLoader component to each scene

**Components Added:**
- SceneLoader component to each scene
- Scenes registered in Build Settings
- Async scene loading capability

**Usage:** Run both commands after lighting setup.

---

## Phase 5: Creative Scenes

### SetupCreativeScenes.cs

Implements creative features from NAAD_CREATIVE_SCENES_PLAN.md.

**Menu Location:** `NAAD → Setup Creative Scenes`

**Commands:**
- `All Creative Features` - Sets up all creative scenes at once
- `SC-1 Occupied Nightclub`
- `SC-4 Opportunities`
- `SC-5 Weather on Street`
- `SC-6 Beach by Period`
- `SC-7 Apartment Aftermath`

**Creative Features:**

**SC-1: Occupied Nightclub (T3/T4)**
- CrowdManager for room presence
- NPC crowd members (5 NPCs)
- Dance floor crowd visualization (8 dancers)

**SC-4: Opportunities (T4)**
- OpportunityManager
- Opportunity board/kiosk with visual indicator (cyan cube)

**SC-5: Weather on Street (T1/T2)**
- WeatherSystem with rain particle system
- Atmospheric fog configuration

**SC-6: Beach by Period (T1/T4)**
- BeachEventManager connected to WorldClockService
- Time-dependent beach objects (day/night)
- Bonfire and beach party area (night only)

**SC-7: Apartment Aftermath (T5)**
- AftermathManager
- Message display with visual indicator (cyan phone)
- Receipt pile with visual indicator (yellow papers)

**Placeholder Components:** Some components (CrowdManager, NPCController, etc.) are placeholders and need full runtime implementation.

**Usage:** Run `NAAD → Setup Creative Scenes → All Creative Features` after base scene setup.

---

## Complete Setup Workflow

Run in this order:

1. **NAAD → Setup All Scenes** (Phase 1)
2. **NAAD → Setup Interaction Points → All Scenes** (Phase 2)
3. **NAAD → Setup World Clock Lighting → All Scenes** (Phase 3)
4. **NAAD → Setup Scene Transitions → Register All Scenes** (Phase 4a)
5. **NAAD → Setup Scene Transitions → Add SceneLoaders to All Scenes** (Phase 4b)
6. **NAAD → Setup Creative Scenes → All Creative Features** (Phase 5)

After running all scripts, all five base scenes (Apartment, CityStreet, SuyaSpot, Nightclub, Beach) will have:
- Core gameplay components (player, camera, mobile controls)
- Interaction points with visual indicators
- World clock lighting integration
- Scene transition capability
- Creative scene features

---

## Manual Adjustments

After running the scripts, you may need to manually adjust:

1. **Player spawn positions** - Move PlayerSpawn to appropriate locations in each scene
2. **Camera follow target** - Connect ThirdPersonCamera's follow target to Player GameObject
3. **Interaction point positions** - Adjust positions to match actual scene geometry
4. **Light positions** - Fine-tune light positions for best visual results
5. **NPC crowd positions** - Adjust NPC positions to avoid overlaps

---

## Runtime Implementation Notes

The following components included in the scripts are placeholders and need full runtime implementation:

- `CrowdManager` - Room presence and crowd density management
- `NPCController` - NPC behavior and AI
- `OpportunityManager` - Career and job opportunity system
- `WeatherSystem` - Weather effect management
- `BeachEventManager` - Time-based beach event logic
- `AftermathManager` - Message and receipt display logic

These are included in the editor scripts to establish the scene structure. Full implementation should be added to the runtime codebase.

---

## Troubleshooting

**Error: "WorldClockService not found"**
- Ensure you ran Phase 1 (SetupScenes) first
- The script checks for WorldClockService before adding dependent components

**Error: "Scene not found"**
- Verify all five base scenes exist in Assets/NAAD/Scenes/
- Check scene file names match the script paths

**Visual indicators not visible**
- Check Scene view camera position
- Ensure objects are not hidden by other geometry
- Check if Game view is in play mode

**Compilation errors after setup**
- Some placeholder components may need full implementation
- Check Console for specific error messages
- Disable problematic scripts if needed (as done earlier with Editor files)

---

## Notes

- All scripts save scenes automatically after making changes
- Scripts are non-destructive - running them multiple times will add duplicate objects
- Visual indicators (spheres/cubes) are for development identification and can be removed
- Scene positions are defaults and should be adjusted for actual scene geometry
- The setup follows the recommended sequence from NAAD_CREATIVE_SCENES_PLAN.md
