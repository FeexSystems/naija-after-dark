# NAAD Lighting Profiles

## Overview

The NAAD megacity uses three distinct lighting profiles to create atmospheric variety and support different gameplay scenarios. Each profile is optimized for mobile performance while maintaining cinematic realism.

## 1. Day Profile - Humid Daylight

### Visual Characteristics
- **Time**: 10:00 AM - 4:00 PM
- **Atmosphere**: Bright, humid, hazy
- **Color Temperature**: 5500K (neutral white)
- **Intensity**: High direct sunlight, soft ambient

### Sun Settings
- **Direction**: 45-60 degrees from horizon
- **Color**: Slightly warm (RGB: 1.0, 0.98, 0.95)
- **Intensity**: 1.2 - 1.5
- **Shadow Type**: Soft shadows
- **Shadow Strength**: 0.7 - 0.8
- **Shadow Distance**: 50-100m

### Ambient Lighting
- **Sky Color**: Light blue (RGB: 0.6, 0.7, 0.9)
- **Equator Color**: Light gray-blue (RGB: 0.5, 0.55, 0.7)
- **Ground Color**: Warm beige (RGB: 0.4, 0.35, 0.3)
- **Ambient Intensity**: 1.0
- **Ambient Mode**: Baked

### Fog Settings
- **Fog Mode**: Exponential
- **Fog Color**: Light gray-blue (RGB: 0.7, 0.75, 0.8)
- **Fog Density**: 0.005 - 0.008
- **Fog Start**: 0m
- **Fog End**: 300m

### Material Guidelines

#### Reflective Surfaces
- **Metal**: High specular, low roughness
- **Glass**: High transparency, moderate reflection
- **Water**: High reflection, slight refraction
- **Wet Surfaces**: Increased specular, darker diffuse

#### Non-Reflective Surfaces
- **Concrete**: Medium roughness, matte finish
- **Brick**: High roughness, low specular
- **Wood**: Medium roughness, subtle grain
- **Fabric**: High roughness, no specular

#### Emissive Materials
- **Neon Signs**: Off or very low intensity (barely visible)
- **LED Displays**: Off or low brightness
- **Windows**: Slight interior glow visible

### District-Specific Adjustments

#### Mainland Streets
- Increased haze (higher fog density)
- Slightly warmer ambient (urban heat)
- Deeper shadows from buildings
- More contrast between sun and shade

#### Island Nightlife
- Cleaner, less hazy atmosphere
- Brighter, crisper lighting
- More reflective surfaces (glass buildings)
- Stronger specular highlights

#### Creative District
- Slightly cooler color temperature
- Softer shadows (industrial buildings)
- More diffuse lighting
- Balanced contrast

#### Coastal
- Brighter overall (ocean reflection)
- Lower fog density (sea breeze)
- Stronger specular on water
- Warmer ambient (sand reflection)

### Performance Notes
- Use baked lighting for static geometry
- Real-time lighting only for dynamic objects
- Light probes for indirect lighting
- Occlusion culling for shadow casting
- Limit real-time shadow distance

## 2. Sunset Profile - Warm Golden Hour

### Visual Characteristics
- **Time**: 6:00 PM - 7:00 PM
- **Atmosphere**: Warm, dramatic, long shadows
- **Color Temperature**: 3500K (warm orange)
- **Intensity**: Lower direct light, higher ambient

### Sun Settings
- **Direction**: 10-20 degrees from horizon
- **Color**: Warm orange (RGB: 1.0, 0.8, 0.5)
- **Intensity**: 0.8 - 1.0
- **Shadow Type**: Long, soft shadows
- **Shadow Strength**: 0.5 - 0.6
- **Shadow Distance**: 100-150m

### Ambient Lighting
- **Sky Color**: Orange-pink (RGB: 0.8, 0.6, 0.5)
- **Equator Color**: Warm orange (RGB: 0.7, 0.5, 0.4)
- **Ground Color**: Deep orange (RGB: 0.5, 0.35, 0.25)
- **Ambient Intensity**: 1.2
- **Ambient Mode**: Baked

### Fog Settings
- **Fog Mode**: Exponential
- **Fog Color**: Warm orange (RGB: 0.8, 0.6, 0.4)
- **Fog Density**: 0.008 - 0.012
- **Fog Start**: 0m
- **Fog End**: 250m

### Material Guidelines

#### Reflective Surfaces
- **Metal**: Warm specular highlights, increased reflection
- **Glass**: Strong warm reflections, orange tint
- **Water**: Golden reflections, high specular
- **Wet Surfaces**: Very high specular, warm highlights

#### Non-Reflective Surfaces
- **Concrete**: Warm orange tint in shadows
- **Brick**: Enhanced red/orange tones
- **Wood**: Warm grain highlights
- **Fabric**: Subtle warm color shift

#### Emissive Materials
- **Neon Signs**: Low-medium intensity (becoming visible)
- **LED Displays**: Medium brightness (starting to activate)
- **Windows**: Warm interior glow

### District-Specific Adjustments

#### Mainland Streets
- Strong orange tint (urban haze)
- Long dramatic shadows
- Warm reflections on wet surfaces
- High contrast between light and shadow

#### Island Nightlife
- Cleaner sunset (less haze)
- Stronger reflections on glass
- Neon signs beginning to glow
- Dramatic lighting on modern architecture

#### Creative District
- Artistic sunset lighting
- Strong shadows on industrial buildings
- Warm tones on street art
- Balanced exposure

#### Coastal
- Spectacular sunset over water
- Strong golden reflections on ocean
- Palm tree silhouettes
- Warm sand tones

### Performance Notes
- Longer shadow distance (dramatic effect)
- Higher fog density (atmosphere)
- More dynamic lighting (sunset transition)
- Consider LOD for distant objects

## 3. Night Profile - Neon and Moonlight

### Visual Characteristics
- **Time**: 8:00 PM - 4:00 AM
- **Atmosphere**: Dark, vibrant, high contrast
- **Color Temperature**: 6500K (cool blue) for ambient
- **Intensity**: Low ambient, high local lighting

### Moon/Primary Light Settings
- **Direction**: 30-45 degrees from horizon
- **Color**: Cool blue-white (RGB: 0.7, 0.8, 1.0)
- **Intensity**: 0.3 - 0.5
- **Shadow Type**: Soft shadows
- **Shadow Strength**: 0.3 - 0.4
- **Shadow Distance**: 30-50m

### Ambient Lighting
- **Sky Color**: Deep blue (RGB: 0.1, 0.15, 0.3)
- **Equator Color**: Dark blue (RGB: 0.08, 0.1, 0.2)
- **Ground Color**: Very dark blue (RGB: 0.05, 0.05, 0.1)
- **Ambient Intensity**: 0.5 - 0.7
- **Ambient Mode**: Realtime

### Fog Settings
- **Fog Mode**: Exponential
- **Fog Color**: Dark blue (RGB: 0.1, 0.12, 0.18)
- **Fog Density**: 0.01 - 0.015
- **Fog Start**: 0m
- **Fog End**: 200m

### Street Lighting
- **Color**: Warm orange (RGB: 1.0, 0.8, 0.6) or cool white (RGB: 0.9, 0.95, 1.0)
- **Intensity**: 2.0 - 3.0
- **Range**: 15-25m
- **Shadow Type**: Soft shadows
- **Cookie**: Optional light cookie for pattern

### Material Guidelines

#### Reflective Surfaces
- **Metal**: High specular, cool reflections
- **Glass**: High reflection, interior glow visible
- **Water**: High reflection, moonlight sparkles
- **Wet Surfaces**: Very high specular, light source reflections

#### Non-Reflective Surfaces
- **Concrete**: Dark, low detail in shadows
- **Brick**: Dark, texture visible only near lights
- **Wood**: Dark, grain visible near lights
- **Fabric**: Very dark, no specular

#### Emissive Materials
- **Neon Signs**: High intensity, vibrant colors
- **LED Displays**: High brightness, animated content
- **Windows**: Interior glow, warm or cool depending on use
- **Street Lights**: Point light sources, warm or cool

### District-Specific Adjustments

#### Mainland Streets
- Warm orange street lighting
- Scattered neon signs (shops)
- Deep shadows between lights
- Wet street reflections after rain

#### Island Nightlife
- Abundant neon and LED lighting
- Cool blue ambient with warm accents
- High contrast lighting
- Strong reflections on modern surfaces

#### Creative District
- Warm white industrial lighting
- Artistic accent lighting
- Moderate contrast
- Interesting shadow patterns

#### Coastal
- Cool moonlight ambient
- Warm tiki torch or lantern lighting
- Water reflections (moonlight)
- Palm tree silhouettes

### Special Lighting Conditions

#### Wet Streets (After Rain)
- **Reflections**: Increased on all surfaces
- **Specular**: Very high on asphalt
- **Light Sources**: Reflected in puddles
- **Atmosphere**: Slightly brighter due to reflections
- **Fog**: Reduced density (cleared by rain)

#### Neon Lighting (Island Nightlife)
- **Colors**: Vibrant pinks, blues, purples, greens
- **Intensity**: High emissive values
- **Bloom**: Post-processing bloom effect
- **Reflections**: Strong on wet surfaces
- **Contrast**: High against dark background

#### Moonlight (Coastal)
- **Color**: Cool blue-white
- **Intensity**: Low, subtle
- **Shadows**: Soft, long
- **Water**: Sparkle effects
- **Atmosphere**: Clear, low fog

### Performance Notes
- Real-time lighting for dynamic elements
- Baked lighting for static geometry
- Light probes for character lighting
- Limit real-time light count
- Use emissive materials instead of lights where possible
- Optimize shadow cascades
- Consider light culling

## Lighting Transition System

### Day to Sunset
- **Duration**: 30-60 seconds
- **Sun Position**: Lower gradually
- **Color Temperature**: 5500K → 3500K
- **Ambient Intensity**: 1.0 → 1.2
- **Fog Density**: 0.005 → 0.01
- **Shadow Length**: Increase gradually

### Sunset to Night
- **Duration**: 30-60 seconds
- **Sun Position**: Below horizon
- **Color Temperature**: 3500K → 6500K
- **Ambient Intensity**: 1.2 → 0.5
- **Fog Density**: 0.01 → 0.015
- **Street Lights**: Fade in gradually
- **Neon Signs**: Fade in gradually

### Night to Day
- **Duration**: 30-60 seconds
- **Sun Position**: Rise gradually
- **Color Temperature**: 6500K → 5500K
- **Ambient Intensity**: 0.5 → 1.0
- **Fog Density**: 0.015 → 0.005
- **Street Lights**: Fade out gradually
- **Neon Signs**: Fade out gradually

## Post-Processing Effects

### Day Profile
- **Bloom**: Minimal or disabled
- **Color Grading**: Neutral, slight warmth
- **Exposure**: Standard
- **Contrast**: Medium
- **Saturation**: Normal

### Sunset Profile
- **Bloom**: Low-medium (sun glow)
- **Color Grading**: Warm orange tint
- **Exposure**: Slightly increased
- **Contrast**: High
- **Saturation**: Increased

### Night Profile
- **Bloom**: High (neon glow)
- **Color Grading**: Cool blue tint with warm accents
- **Exposure**: Decreased
- **Contrast**: Very high
- **Saturation**: Normal to increased (neon colors)

## Implementation Notes

### Unity Settings
- Use Lighting Settings assets for each profile
- Create Scene Lighting profiles
- Use Light Probes for character lighting
- Configure Reflection Probes for accurate reflections
- Set up Realtime Global Illumination if performance allows

### Mobile Optimization
- Limit realtime light count
- Use baked lighting where possible
- Reduce shadow map resolution
- Limit reflection probe count
- Use simplified fog
- Consider disabling some post-processing effects on low-end devices

### Testing Checklist
- [ ] Day profile looks correct in all districts
- [ ] Sunset transitions smoothly
- [ ] Night profile has good contrast
- [ ] Wet street reflections work correctly
- [ ] Neon signs are visible at night
- [ ] Moonlight effects work on coast
- [ ] Performance is acceptable on target devices
- [ ] Transitions between profiles are smooth
