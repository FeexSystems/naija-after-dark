# Apartment Visual Blockout

## Overview

The apartment blockout serves as a key residential location in the NAAD megacity. It includes both exterior street-facing elements and interior living spaces. This blockout focuses on silhouette, composition, and player interaction points.

## Exterior Blockout

### Building Structure

#### Dimensions
- **Height**: 3 stories (Mainland), 5 stories (Island), 2 stories (Coastal)
- **Width**: 12-15 meters per unit
- **Depth**: 8-10 meters per unit
- **Floor Height**: 3 meters per story

#### Facade Composition
```
┌─────────────────────────────┐
│         Roof Terrace         │  (Optional, Island variant)
├─────────────────────────────┤
│  [Window] [Balcony] [Window] │  Floor 3
├─────────────────────────────┤
│  [Window] [Balcony] [Window] │  Floor 2
├─────────────────────────────┤
│  [Door]   [Window]  [Window] │  Floor 1 (Ground)
├─────────────────────────────┤
│      Street Level Shop       │  (Optional, Mainland variant)
└─────────────────────────────┘
```

### Exterior Components

#### Walls
- **Main Walls**: Flat concrete or painted plaster
- **Texture**: Smooth with subtle weathering
- **Color**: 
  - Mainland: Warm beige, light yellow, pale orange
  - Island: White, light gray, pastel colors
  - Creative: Exposed brick, concrete gray
  - Coastal: White, light blue, sandy beige

#### Windows
- **Small Windows**: 1.2m x 1.5m (standard residential)
- **Large Windows**: 2.0m x 2.5m (floor-to-ceiling, Island variant)
- **Placement**: Regular grid pattern, staggered for visual interest
- **Frames**: Metal (Island), Wood (Mainland), Aluminum (Creative)

#### Balconies
- **Depth**: 1.5-2.0 meters
- **Railing**: Metal bars (Island), wrought iron (Mainland), glass (Creative)
- **Floor**: Concrete tiles (standard), wood planks (Coastal)
- **Placement**: Every unit or alternating units

#### Doors
- **Main Entrance**: Single or double door, 1.0m x 2.1m
- **Material**: Metal (security), Wood (residential), Glass (Island)
- **Location**: Ground floor, street-facing

#### Roof
- **Flat Roof**: Standard for most buildings
- **Parapet**: 0.5m height around perimeter
- **Roof Access**: Hatch or stairs (optional)
- **Roof Terrace**: Optional Island variant with seating

### Street-Level Elements

#### Shopfront (Mainland Variant)
- **Rolling Shutters**: Metal, often colorful
- **Glass Display**: Large windows for products
- **Awning**: Fabric canopy over entrance
- **Signage**: Box signs, banners

#### Utility Elements
- **AC Units**: Wall-mounted on exterior
- **Satellite Dishes**: Roof-mounted
- **Utility Poles**: Street-side
- **Cables**: Visible overhead (Mainland)

### Exterior Interaction Points

#### Player Access
- **Main Entrance**: Door with collision mesh
- **Balcony Access**: From interior rooms
- **Stairwell**: External or internal stairs
- **Elevator**: (Island variant only)

#### Exterior Activities
- **Balcony Interaction**: Stand, lean, socialize
- **Street-Level Shop**: Enter, browse, purchase
- **Building Entrance**: Enter lobby, access apartments

## Interior Blockout

### Floor Plan Layout

#### Standard Apartment Layout (1-Bedroom)
```
┌─────────────────────────────┐
│   [Balcony Door]   Balcony  │
├─────────────────────────────┤
│  Living Room    [Window]    │
│  [Sofa] [Table]  [TV]       │
├─────────────────────────────┤
│  Kitchen       [Window]     │
│  [Counter] [Stove] [Fridge] │
├─────────────────────────────┤
│  Bedroom       [Window]     │
│  [Bed] [Wardrobe]           │
├─────────────────────────────┤
│  Bathroom                   │
│  [Shower] [Toilet] [Sink]   │
└─────────────────────────────┘
```

#### Larger Apartment Layout (2-Bedroom, Island Variant)
```
┌─────────────────────────────┐
│   [Balcony Door]   Balcony  │
├─────────────────────────────┤
│  Living Room    [Window]    │
│  [Sofa] [Table]  [TV]       │
├─────────────────────────────┤
│  Kitchen       [Window]     │
│  [Counter] [Stove] [Fridge] │
├──────────────┬──────────────┤
│  Bedroom 1   │  Bedroom 2   │
│  [Bed]       │  [Bed]       │
├──────────────┴──────────────┤
│  Bathroom                   │
│  [Shower] [Toilet] [Sink]   │
└─────────────────────────────┘
```

### Interior Components

#### Walls
- **Interior Walls**: 0.15m thickness
- **Material**: Painted drywall or plaster
- **Color**: White, light gray, warm beige
- **Doors**: Standard interior doors (0.8m x 2.0m)

#### Floors
- **Living Areas**: Tile or wood planks
- **Kitchen**: Tile (easy to clean)
- **Bedroom**: Wood or carpet
- **Bathroom**: Tile (water-resistant)

#### Ceilings
- **Height**: 2.7-3.0 meters
- **Material**: Painted plaster or drywall
- **Lighting**: Ceiling-mounted fixtures

#### Windows
- **Placement**: Exterior walls only
- **Size**: As per exterior blockout
- **Curtains/Blinds**: Optional detail

### Furniture Layout

#### Living Room
- **Sofa**: 2-3 seater, against wall
- **Coffee Table**: In front of sofa
- **TV Stand**: Opposite sofa
- **Side Table**: Next to sofa
- **Lamp**: Table or floor lamp
- **Rug**: Under coffee table

#### Kitchen
- **Counter**: L-shaped or straight
- **Stove**: Along counter
- **Refrigerator**: Corner placement
- **Sink**: In counter
- **Cabinets**: Above and below counter

#### Bedroom
- **Bed**: Against wall, centered
- **Wardrobe**: Along wall
- **Nightstand**: Next to bed
- **Lamp**: On nightstand
- **Dresser**: Optional

#### Bathroom
- **Shower**: Corner or along wall
- **Toilet**: Along wall
- **Sink**: With vanity or pedestal
- **Mirror**: Above sink

### Interior Interaction Points

#### Player Activities
- **Sit on Sofa**: Living room
- **Sleep in Bed**: Bedroom
- **Cook at Stove**: Kitchen
- **Use Bathroom**: Bathroom
- **Watch TV**: Living room
- **Look out Window**: Any room with window

#### Interactive Objects
- **Television**: Turn on/off, change channel
- **Refrigerator**: Open, take food/drink
- **Stove**: Cook (if implemented)
- **Bed**: Sleep, rest
- **Sofa**: Sit, relax
- **Phone**: Use (if present)

## District Variants

### Mainland Streets Apartment
- **Exterior**: Painted concrete, colorful shopfronts
- **Windows**: Smaller, metal frames
- **Balconies**: Wrought iron railings
- **Interior**: Basic furnishings, warm colors
- **Street Level**: Often has shop or kiosk

### Island Nightlife Apartment
- **Exterior**: Modern, glass facades, clean lines
- **Windows**: Large floor-to-ceiling
- **Balconies**: Glass railings, modern design
- **Interior**: Premium furnishings, modern design
- **Street Level**: Lobby or high-end shop

### Creative District Apartment
- **Exterior**: Industrial conversion, exposed elements
- **Windows**: Large, industrial style
- **Balconies**: Minimalist design
- **Interior**: Modern, artistic furnishings
- **Street Level**: Gallery or studio space

### Coastal Apartment
- **Exterior**: White, weathered, tropical elements
- **Windows**: Large, ocean-facing
- **Balconies**: Wood railings, tropical plants
- **Interior**: Beach-themed, relaxed furnishings
- **Street Level**: Open to beach or promenade

## Technical Specifications

### Collision Meshes
- **Walls**: Full collision
- **Floors**: Walkable surface
- **Furniture**: Collision where appropriate
- **Windows**: Collision (unless openable)
- **Doors**: Collision (unless openable)

### Navigation
- **NavMesh**: Bake for interior and exterior
- **Agent Height**: 2.0 meters
- **Agent Radius**: 0.5 meters
- **Step Height**: 0.4 meters
- **Slope Limit**: 45 degrees

### Lighting
- **Interior**: Baked lighting for static elements
- **Windows**: Light probes for natural light
- **Artificial Lights**: Ceiling fixtures, lamps
- **Day/Night**: Interior lighting responds to time

### Performance
- **LODs**: For distant buildings
- **Occlusion**: For interior spaces
- **Light Baking**: For static interior lighting
- **Material Sharing**: Across apartment variants

## Blockout Priority

### Phase 1: Core Structure
1. Building exterior shell
2. Basic facade (walls, windows, doors)
3. Interior floor plan
4. Basic furniture placement
5. Collision meshes

### Phase 2: Detail
1. Balconies and railings
2. Exterior details (AC units, cables)
3. Interior furniture refinement
4. Lighting setup
5. Material application

### Phase 3: Polish
1. District-specific variants
2. Weathering and variation
3. Interactive elements
4. NavMesh optimization
5. Performance optimization

## Testing Checklist

- [ ] Player can enter and exit apartment
- [ ] All rooms are accessible
- [ ] Furniture has appropriate collision
- [ ] Windows provide exterior view
- [ ] Lighting looks correct day/night
- [ ] NavMesh allows navigation
- [ ] Performance is acceptable
- [ ] District variants are distinct
- [ ] Interactive objects work correctly
