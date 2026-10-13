# Suya Spot Visual Blockout

## Overview

The Suya Spot is a quintessential Nigerian street food location where grilled meat (suya) is prepared and served. This blockout focuses on creating an authentic, interactive street food environment with clear player interaction points.

## Location Context

### District Placement
- **Primary**: Mainland Streets (most common)
- **Secondary**: Creative District (modern food truck variant)
- **Tertiary**: Coastal (beachside suya spot)

### Street Integration
- **Placement**: Along busy streets, near intersections
- **Size**: 4m x 6m footprint (compact)
- **Orientation**: Open to street, facing pedestrian flow
- **Accessibility**: Direct street access, no barriers

## Structure Layout

### Overall Composition
```
┌─────────────────────────────────┐
│         [Canopy/Fabric]         │
│  ┌───────────────────────────┐  │
│  │   [Grill]   [Grill]       │  │
│  │   [Counter]   [Counter]   │  │
│  │                           │  │
│  │  [Seating Area]           │  │
│  │  [Table] [Chair] [Table]  │  │
│  └───────────────────────────┘  │
│         [Street Side]            │
└─────────────────────────────────┘
```

### Components

#### Main Structure
- **Frame**: Metal poles (square or round steel)
- **Height**: 2.5-3.0 meters (head clearance)
- **Width**: 4 meters
- **Depth**: 6 meters
- **Roof**: Corrugated metal or fabric canopy
- **Open Sides**: No walls, open to street

#### Grill Station
- **Grill Type**: Open charcoal grill
- **Size**: 1.0m x 0.5m per grill
- **Height**: 0.9 meters (counter height)
- **Material**: Metal frame, wire mesh
- **Charcoal**: Visible under grill
- **Smoke**: Particle effect (optional)
- **Placement**: Along one side of structure

#### Counter
- **Length**: 2-3 meters
- **Height**: 0.9 meters
- **Depth**: 0.6 meters
- **Material**: Wood planks or metal sheet
- **Surface**: Food preparation area
- **Placement**: In front of grills

#### Seating Area
- **Tables**: Round or rectangular
- **Chairs**: Plastic chairs or wooden benches
- **Capacity**: 6-8 people
- **Arrangement**: Casual, flexible
- **Placement**: Opposite grill station

#### Canopy/Shade
- **Type**: Fabric tarp or metal roof
- **Height**: 2.8 meters
- **Coverage**: Full structure
- **Color**: Striped (red/white) or solid (orange/yellow)
- **Support**: Metal frame poles

## Detailed Component Specifications

### Grill Station

#### Open Grill
- **Frame**: Square steel tubing (2.5cm)
- **Grate**: Steel wire mesh (1cm grid)
- **Charcoal Bed**: 5cm depth
- **Ash Pan**: Under grate
- **Handles**: On sides for moving
- **Ventilation**: Open sides for airflow

#### Grill Accessories
- **Tongs**: Hanging on side
- **Knife**: For cutting meat
- **Cutting Board**: On counter
- **Spice Containers**: Small bowls
- **Meat Skewers**: Visible on counter

### Counter

#### Counter Surface
- **Material**: Wood planks (rough finish)
- **Thickness**: 3cm
- **Support**: Metal frame underneath
- **Cleanliness**: Worn, used appearance

#### Counter Elements
- **Serving Area**: Front section for customers
- **Prep Area**: Back section for staff
- **Payment Area**: Small section for transactions
- **Storage**: Shelves underneath

### Seating

#### Tables
- **Round Tables**: 0.8m diameter, 4 chairs
- **Rectangular Tables**: 1.2m x 0.6m, 4-6 chairs
- **Height**: 0.75 meters
- **Material**: Wood or metal
- **Stability**: Weighted or anchored

#### Chairs
- **Plastic Chairs**: Monobloc style (common in Nigeria)
- **Wooden Benches**: Simple plank benches
- **Height**: 0.45 meters
- **Stackable**: For storage

#### Benches
- **Length**: 1.5-2.0 meters
- **Height**: 0.45 meters
- **Material**: Wood planks on metal frame
- **Capacity**: 3-4 people

### Lighting

#### Evening Lighting
- **String Lanterns**: Hung from canopy
- **Bulb Type**: Warm white LED
- **Spacing**: 1 meter apart
- **Power**: Extension cord to nearby building

#### Ambient Light
- **Grill Glow**: Emissive from charcoal
- **Street Light**: Nearby street lamp
- **Shop Light**: Interior of nearby shop

## District Variants

### Mainland Streets Suya Spot
- **Style**: Traditional, authentic
- **Materials**: Wood, metal, fabric
- **Colors**: Warm oranges, reds, yellows
- **Details**: Worn, weathered, used appearance
- **Seating**: Plastic chairs, wooden benches
- **Canopy**: Striped fabric tarp
- **Grills**: 2-3 open charcoal grills

### Creative District Suya Spot
- **Style**: Modern, food truck aesthetic
- **Materials**: Metal, glass, modern materials
- **Colors**: Cool blues, greens, modern palette
- **Details**: Clean, branded, designed
- **Seating**: Modern outdoor furniture
- **Canopy**: Metal roof with branding
- **Grills**: Modern gas grills or electric

### Coastal Suya Spot
- **Style**: Beachside, relaxed
- **Materials**: Wood, thatch, natural materials
- **Colors**: Sandy beiges, tropical greens
- **Details**: Weathered by salt air
- **Seating**: Beach chairs, low tables
- **Canopy**: Thatch or fabric
- **Grills**: Open charcoal grills

## Player Interaction Points

### Ordering
- **Approach Counter**: Walk to serving area
- **View Menu**: Menu board or verbal menu
- **Place Order**: Interact with vendor
- **Pay**: Cash or mobile payment

### Waiting
- **Stand Near Counter**: While food is prepared
- **Watch Grilling**: Observe cooking process
- **Socialize**: Talk to other customers
- **Check Phone**: While waiting

### Eating
- **Sit at Table**: Assigned or choose available
- **Receive Food**: Vendor brings to table
- **Eat**: Consume food (animation)
- **Socialize**: Talk to companions

### Social Activities
- **Meet Friends**: Gather at suya spot
- **People Watch**: Observe street life
- **Date Night**: Casual dining
- **Late Night Snack**: After nightlife

## Technical Specifications

### Collision Meshes
- **Structure Frame**: Collision
- **Counter**: Collision (top surface walkable)
- **Tables**: Collision
- **Chairs**: Collision
- **Grills**: Collision (hot surface warning)
- **Canopy**: No collision (visual only)

### Navigation
- **NavMesh**: Bake for seating area
- **Agent Height**: 2.0 meters
- **Agent Radius**: 0.5 meters
- **Step Height**: 0.4 meters
- **Slope Limit**: 45 degrees
- **Walkable Areas**: Around structure, between tables

### Lighting
- **Grill Glow**: Emissive material
- **String Lights**: Point lights
- **Street Light**: Environmental
- **Baked Lighting**: For static structure
- **Realtime**: For grill glow and string lights

### Effects
- **Smoke**: Particle system from grills
- **Heat Shimmer**: Optional heat distortion
- **Sound**: Sizzling, chatter, street noise
- **Smell**: (Visual only - steam particles)

## Performance Considerations

### Optimization
- **LODs**: For distant suya spots
- **Instancing**: For chairs and tables
- **Light Baking**: For structure
- **Particle Limits**: Limit smoke particles
- **Material Sharing**: Across suya spot variants

### Mobile Optimization
- **Reduced Poly Count**: Simplify geometry
- **Texture Atlasing**: For small props
- **Light Probe**: Instead of realtime lights
- **Occlusion**: For seating area
- **Draw Call Batching**: For repeated elements

## Blockout Priority

### Phase 1: Core Structure
1. Main frame and canopy
2. Grill station
3. Counter
4. Basic seating (1 table, 2 chairs)
5. Collision meshes

### Phase 2: Detail
1. Additional seating
2. Grill accessories
3. Counter elements
4. Lighting setup
5. Material application

### Phase 3: Polish
1. District-specific variants
2. Weathering and variation
3. Interactive elements
4. Particle effects
5. Performance optimization

## Testing Checklist

- [ ] Player can approach counter
- [ ] Player can order food
- [ ] Player can sit at table
- [ ] Player can eat (animation)
- [ ] Navigation works around structure
- [ ] Collision is appropriate
- [ ] Lighting looks correct day/night
- [ ] Smoke effect works (if implemented)
- [ ] Performance is acceptable
- [ ] District variants are distinct
- [ ] Social interactions work correctly

## Authenticity Notes

### Nigerian Suya Culture
- **Grilling Method**: Open charcoal grill is traditional
- **Spices**: Suya spice mix (yaji) is essential
- **Serving**: Wrapped in newspaper or foil
- **Eating**: By hand, with onions and tomatoes
- **Social**: Important gathering place
- **Timing**: Popular evening and late-night food

### Visual Authenticity
- **Smoke**: Visible smoke from charcoal grills
- **Spices**: Visible spice containers
- **Meat**: Skewers of meat visible
- **Crowd**: Often busy with customers
- **Street Integration**: Part of street life, not isolated

### Cultural Elements
- **Music**: Often playing from nearby speakers
- **Conversation**: Lively social atmosphere
- **Vendor Interaction**: Friendly, informal
- **Payment**: Cash is common, mobile payment growing
- **Timing**: Peak hours: evening to late night
