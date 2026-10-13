# NAAD Mobile Optimisation Plan

**Generated**: 2026-10-13  
**Status**: Configuration Inspected, Plan Defined

## Unity Configuration Summary

### Unity Version
- **Version**: 6000.6.5f1 (Unity 6.0.6)
- **Status**: Latest stable release

### Render Pipeline
- **Type**: Built-in Render Pipeline (BRP)
- **Mobile Rendering Path**: Forward
- **Custom Pipeline**: None

### Android Configuration
- **Package Name**: com.feexsystems.naijaafterdark
- **Min SDK**: 26 (Android 8.0 Oreo)
- **Target SDK**: 0 (Not set - REQUIRES CONFIGURATION)
- **Architecture**: ARM64
- **Scripting Backend**: IL2CPP
- **Managed Stripping**: Minimal
- **Graphics API**: OpenGLES3
- **Default Quality**: Low (Level 2)

### Quality Settings (Android Default: Low)
- **Pixel Light Count**: 0
- **Shadows**: Disabled
- **Shadow Resolution**: Disabled
- **Shadow Distance**: 20
- **Anti-Aliasing**: Disabled
- **Anisotropic Textures**: Disabled
- **Soft Particles**: Disabled
- **Soft Vegetation**: Disabled
- **Realtime Reflection Probes**: Disabled
- **LOD Bias**: 0.4
- **VSync Count**: 0 (Unlimited FPS)
- **Texture Mipmap Limit**: 0 (Full resolution)

## Performance Budgets

### Target Devices

#### Low-End Device (Representative)
- **Device**: Samsung Galaxy A10s or equivalent
- **CPU**: Exynos 7884 (Octa-core 1.6 GHz)
- **GPU**: Mali-G71 MP2
- **RAM**: 2 GB
- **Resolution**: 720p (1280x720)
- **Android Version**: 8.0+

**Performance Budgets**:
- **Target FPS**: 30 FPS minimum
- **Frame Time**: 33.3 ms maximum
- **Draw Calls**: < 100
- **Triangles**: < 50,000
- **Texture Memory**: < 200 MB
- **Total Memory**: < 1 GB
- **Shader Variants**: < 200
- **GC Allocations**: < 1 MB per frame

#### Mid-Range Device (Representative)
- **Device**: Samsung Galaxy A53 or equivalent
- **CPU**: Exynos 1280 (Octa-core 2.4 GHz)
- **GPU**: Mali-G68
- **RAM**: 6 GB
- **Resolution**: 1080p (2400x1080)
- **Android Version**: 12+

**Performance Budgets**:
- **Target FPS**: 60 FPS minimum
- **Frame Time**: 16.6 ms maximum
- **Draw Calls**: < 200
- **Triangles**: < 100,000
- **Texture Memory**: < 400 MB
- **Total Memory**: < 2 GB
- **Shader Variants**: < 400
- **GC Allocations**: < 2 MB per frame

## Inspection Checklist

### Frame Time and FPS
- **Current Status**: Not measured (requires profiling)
- **Target**: 30 FPS (low-end), 60 FPS (mid-range)
- **Measurement Tool**: Unity Profiler, Android Profiler

### GPU/CPU Bottlenecks
- **Current Status**: Not measured (requires profiling)
- **Target**: Balanced GPU/CPU usage
- **Measurement Tool**: Unity Profiler, RenderDoc

### Draw Calls
- **Current Status**: Not measured (requires profiling)
- **Target**: < 100 (low-end), < 200 (mid-range)
- **Optimization**: Batching, instancing

### Memory Use
- **Current Status**: Not measured (requires profiling)
- **Target**: < 1 GB (low-end), < 2 GB (mid-range)
- **Measurement Tool**: Unity Profiler, Android Memory Profiler

### Texture Compression
- **Current Status**: Not configured
- **Target**: ASTC for Android, ETC2 fallback
- **Action**: Configure texture compression settings

### Shader Variants
- **Current Status**: Not measured (requires profiling)
- **Target**: < 200 (low-end), < 400 (mid-range)
- **Optimization**: Shader stripping, SRP Batcher

### GC Allocations
- **Current Status**: Not measured (requires profiling)
- **Target**: < 1 MB/frame (low-end), < 2 MB/frame (mid-range)
- **Optimization**: Object pooling,减少 allocations

### NPC and Crowd Update Frequency
- **Current Status**: Not implemented
- **Target**: Update at 10-15 Hz for distant NPCs
- **Optimization**: LOD system, culling

### Audio Voice Limits
- **Current Status**: Not configured
- **Target**: Max 16 simultaneous voices
- **Optimization**: Audio priority system

### Scene Loading
- **Current Status**: Not measured (requires profiling)
- **Target**: < 3 seconds for main scenes
- **Optimization**: Async loading, asset bundles

### Touch Controls and Safe Areas
- **Current Status**: Not implemented
- **Target**: 44dp minimum touch targets, safe area support
- **Optimization**: Unity Safe Area package

### Thermal and Battery Behaviour
- **Current Status**: Not measured (requires profiling)
- **Target**: No thermal throttling, efficient battery use
- **Optimization**: Frame rate limiting, quality scaling

## Optimisation Strategy

### 1. Texture Optimisation

#### Compression
- **Format**: ASTC 4x4 for Android (primary), ETC2 (fallback)
- **Max Resolution**: 1024x1024 for large assets, 512x512 for medium, 256x256 for small
- **Mipmaps**: Enable for all textures
- **Texture Streaming**: Consider for large scenes

#### Implementation
- Set texture compression in import settings
- Use texture atlases for UI and small props
- Compress normal maps to BC5/ASTC
- Use RGB channels for packed textures

### 2. Shader Optimisation

#### Shader Variants
- **Stripping**: Enable shader variant stripping
- **Keywords**: Minimize shader keywords
- **SRP Batcher**: Not applicable (Built-in pipeline)
- **Mobile Shaders**: Use mobile-optimized shaders

#### Implementation
- Use Unity built-in mobile shaders
- Minimize custom shader complexity
- Avoid expensive operations in shaders
- Use texture compression for shader properties

### 3. Geometry Optimisation

#### LOD System
- **LOD Levels**: 3 levels (High, Medium, Low)
- **LOD Transitions**: Smooth cross-fade
- **LOD Bias**: Adjust based on quality settings

#### Mesh Optimisation
- **Triangle Count**: Target < 50k triangles per scene
- **Vertex Count**: Target < 100k vertices per scene
- **Mesh Compression**: Enable for static meshes
- **Optimize Mesh Data**: Enable

### 4. Lighting Optimisation

#### Baked Lighting
- **Lightmaps**: Bake static lighting
- **Lightmap Resolution**: 1 texel per unit
- **Lightmap Compression**: Enable
- **Realtime Lights**: Minimize to essential lights

#### Shadow Optimisation
- **Shadows**: Disabled on Low quality (current setting is correct)
- **Shadow Distance**: Keep at 20 on Low quality
- **Shadow Cascades**: 1 cascade on Low quality

### 5. Particle System Optimisation

#### Particle Count
- **Max Particles**: Limit per system
- **Particle Budget**: < 1000 active particles
- **Culling**: Enable particle culling

#### Implementation
- Use particle pooling
- Limit particle lifetime
- Use simple particle shaders
- Disable particles on Low quality

### 6. Audio Optimisation

#### Voice Limits
- **Max Voices**: 16 simultaneous
- **Priority System**: Implement audio priority
- **Compression**: Use compressed audio formats

#### Implementation
- Use Ogg Vorbis or MP3 for music
- Use compressed formats for SFX
- Implement audio pooling
- Mute distant audio sources

### 7. Script Optimisation

#### GC Allocations
- **Object Pooling**: Pool frequently used objects
- **StringBuilder**: Use for string concatenation
- **Avoid LINQ**: In hot paths
- **Cache References**: Cache frequently accessed components

#### Update Frequency
- **Fixed Timestep**: 50 Hz (20ms)
- **Maximum Allowed Timestep**: 100ms
- **NPC Updates**: Update distant NPCs at lower frequency

### 8. UI Optimisation

#### Canvas Optimisation
- **Canvas Scaler**: Configure for mobile
- **Pixel Perfect**: Disable
- **Reference Resolution**: 1080x1920 (portrait)
- **Match Mode**: Match Width or Height

#### UI Elements
- **Touch Targets**: 44dp minimum
- **Safe Areas**: Use Unity Safe Area package
- **UI Batching**: Enable
- **Atlas**: Use sprite atlas for UI

### 9. Network Optimisation

#### Data Compression
- **JSON**: Minify JSON payloads
- **Binary**: Consider binary serialization
- **Caching**: Cache frequently accessed data
- **Batching**: Batch network requests

#### Implementation
- Compress network payloads
- Implement request queuing
- Use HTTP/2 if available
- Implement offline mode

### 10. Memory Optimisation

#### Texture Memory
- **Unload Unused**: Unload unused textures
- **Asset Bundles**: Use for large assets
- **Addressables**: Consider for dynamic loading
- **Memory Budget**: Enforce memory limits

#### Implementation
- Use Resources.UnloadUnusedAssets
- Implement asset unloading on scene change
- Use texture compression
- Monitor memory usage

## Required Configuration Changes

### Android Settings
1. **Set Target SDK**: Set to latest stable (e.g., API 34)
2. **Enable Multidex**: Enable if needed
3. **Configure Graphics APIs**: Add Vulkan as option
4. **Set Scripting Backend**: IL2CPP (already set)
5. **Configure Architecture**: ARM64 (already set)

### Quality Settings
1. **Android Default**: Keep at Low (already set)
2. **VSync Count**: Set to 1 for battery saving
3. **Texture Quality**: Set to Full Resolution (use mipmap LOD)
4. **Anisotropic Textures**: Enable on Medium/High quality only

### Graphics Settings
1. **Tier Settings**: Configure for mobile
2. **Shader Stripping**: Enable
3. **Lightmap Stripping**: Enable
4. **Instancing Stripping**: Enable

### Player Settings
1. **Android Min SDK**: Keep at 26 (Android 8.0)
2. **Android Target SDK**: Set to 34 (Android 14)
3. **Android Install Location**: Auto (already set)
4. **Force Internet Permission**: Enable if needed
5. **Android Sustained Performance**: Consider enabling

## Implementation Priority

### Phase 1: Critical Configuration (Week 1)
1. Set Android Target SDK to latest stable
2. Configure texture compression (ASTC)
3. Enable shader variant stripping
4. Configure quality settings for mobile
5. Set VSync to 1 for battery saving

### Phase 2: Asset Optimisation (Week 2-3)
1. Compress all textures
2. Create texture atlases
3. Implement LOD system
4. Optimize meshes
5. Bake lighting

### Phase 3: Runtime Optimisation (Week 4)
1. Implement object pooling
2. Implement audio priority system
3. Optimize UI canvas
4. Implement safe area support
5. Optimize scripts for GC

### Phase 4: Profiling and Tuning (Week 5)
1. Profile on target devices
2. Measure frame time and FPS
3. Identify bottlenecks
4. Tune performance budgets
5. Verify thermal behaviour

## Android Permissions

### Required Permissions
- **INTERNET**: Required for networking
- **ACCESS_NETWORK_STATE**: Required for network checks
- **VIBRATE**: Optional for haptic feedback

### Optional Permissions
- **ACCESS_FINE_LOCATION**: If location features needed
- **ACCESS_COARSE_LOCATION**: If location features needed
- **CAMERA**: If camera features needed
- **RECORD_AUDIO**: If voice features needed
- **WRITE_EXTERNAL_STORAGE**: If file access needed

### Security
- **No Server Secrets**: Ensure no API keys in client code
- **Certificate Pinning**: Implement for network security
- **ProGuard/R8**: Enable code obfuscation

## Verification Checklist

Before marking mobile optimisation complete:
- [ ] Android Target SDK set to latest stable
- [ ] Texture compression configured (ASTC)
- [ ] Shader variant stripping enabled
- [ ] Quality settings configured for mobile
- [ ] LOD system implemented
- [ ] Object pooling implemented
- [ ] Audio priority system implemented
- [ ] UI canvas optimized
- [ ] Safe area support implemented
- [ ] Frame time measured on target device
- [ ] FPS measured on target device
- [ ] Draw calls measured and optimized
- [ ] Memory usage measured and optimized
- [ ] Thermal behaviour tested
- [ ] Battery usage measured
- [ ] Android permissions verified
- [ ] No server secrets in client code
- [ ] Build tested on target device
- [ ] Performance budgets met

## Next Steps

1. Configure Android Target SDK
2. Configure texture compression
3. Enable shader stripping
4. Implement LOD system
5. Implement object pooling
6. Profile on target devices
7. Tune performance budgets
8. Verify thermal and battery behaviour
9. Test on multiple devices
10. Document final performance metrics

## Notes

- Performance budgets are estimates based on typical device specifications
- Actual performance may vary based on device capabilities
- Profiling on actual target devices is required for accurate measurements
- Performance should be tested on both low-end and mid-range devices
- Thermal throttling should be monitored during extended play sessions
- Battery impact should be measured and optimized
