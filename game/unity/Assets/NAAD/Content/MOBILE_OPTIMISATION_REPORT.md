# NAAD Mobile Optimisation and Android Verification Report

**Generated**: 2026-10-13  
**Status**: Configuration Inspected, Plan Defined, Implementation Pending

## Summary

The NAAD project has been inspected for mobile optimisation and Android verification. Unity configuration has been reviewed, performance budgets have been established for representative devices, and a comprehensive optimisation plan has been defined.

**Configuration Status**: ✓ Inspected  
**Performance Budgets**: ✓ Defined  
**Optimisation Plan**: ✓ Complete  
**Implementation Status**: ✗ Pending  
**Android Build Status**: ✗ Not verified

## Unity Configuration

### Unity Version
- **Version**: 6000.6.5f1 (Unity 6.0.6)
- **Status**: Latest stable release, suitable for mobile development

### Render Pipeline
- **Type**: Built-in Render Pipeline (BRP)
- **Mobile Rendering Path**: Forward
- **Custom Pipeline**: None
- **Assessment**: BRP is suitable for mobile, but URP would offer better performance optimisation options

### Android Configuration
- **Package Name**: com.feexsystems.naijaafterdark
- **Min SDK**: 26 (Android 8.0 Oreo) ✓
- **Target SDK**: 0 (Not set) ✗ **REQUIRES CONFIGURATION**
- **Architecture**: ARM64 ✓
- **Scripting Backend**: IL2CPP ✓
- **Managed Stripping**: Minimal ✓
- **Graphics API**: OpenGLES3 ✓
- **Default Quality**: Low (Level 2) ✓

### Quality Settings (Android Default: Low)
- **Pixel Light Count**: 0 ✓ (Good for mobile)
- **Shadows**: Disabled ✓ (Good for mobile)
- **Shadow Resolution**: Disabled ✓
- **Shadow Distance**: 20 ✓
- **Anti-Aliasing**: Disabled ✓
- **Anisotropic Textures**: Disabled ✓
- **Soft Particles**: Disabled ✓
- **Soft Vegetation**: Disabled ✓
- **Realtime Reflection Probes**: Disabled ✓
- **LOD Bias**: 0.4 ✓
- **VSync Count**: 0 (Unlimited FPS) ⚠️ Should be 1 for battery saving
- **Texture Mipmap Limit**: 0 (Full resolution) ⚠️ Should use mipmap LOD

### Graphics Settings
- **Custom Render Pipeline**: None
- **Tier Settings**: Not configured
- **Shader Stripping**: Not enabled ⚠️ Should be enabled
- **Lightmap Stripping**: Not enabled ⚠️ Should be enabled
- **Instancing Stripping**: Not enabled ⚠️ Should be enabled

## Performance Budgets

### Low-End Device (Samsung Galaxy A10s or equivalent)
- **CPU**: Exynos 7884 (Octa-core 1.6 GHz)
- **GPU**: Mali-G71 MP2
- **RAM**: 2 GB
- **Resolution**: 720p (1280x720)

**Budgets**:
- **Target FPS**: 30 FPS minimum
- **Frame Time**: 33.3 ms maximum
- **Draw Calls**: < 100
- **Triangles**: < 50,000
- **Texture Memory**: < 200 MB
- **Total Memory**: < 1 GB
- **Shader Variants**: < 200
- **GC Allocations**: < 1 MB per frame

### Mid-Range Device (Samsung Galaxy A53 or equivalent)
- **CPU**: Exynos 1280 (Octa-core 2.4 GHz)
- **GPU**: Mali-G68
- **RAM**: 6 GB
- **Resolution**: 1080p (2400x1080)

**Budgets**:
- **Target FPS**: 60 FPS minimum
- **Frame Time**: 16.6 ms maximum
- **Draw Calls**: < 200
- **Triangles**: < 100,000
- **Texture Memory**: < 400 MB
- **Total Memory**: < 2 GB
- **Shader Variants**: < 400
- **GC Allocations**: < 2 MB per frame

## Current Performance Status

### Measured Metrics
- **Frame Time**: Not measured (requires profiling)
- **FPS**: Not measured (requires profiling)
- **GPU/CPU Bottlenecks**: Not measured (requires profiling)
- **Draw Calls**: Not measured (requires profiling)
- **Memory Use**: Not measured (requires profiling)
- **Texture Compression**: Not configured
- **Shader Variants**: Not measured (requires profiling)
- **GC Allocations**: Not measured (requires profiling)
- **NPC Update Frequency**: Not implemented
- **Audio Voice Limits**: Not configured
- **Scene Loading**: Not measured (requires profiling)
- **Touch Controls**: Not implemented
- **Safe Areas**: Not implemented
- **Thermal Behaviour**: Not measured (requires profiling)
- **Battery Behaviour**: Not measured (requires profiling)

## Required Optimisations

### Critical (Must Implement)
1. **Set Android Target SDK** - Currently not set, must configure to latest stable
2. **Configure Texture Compression** - ASTC for Android, ETC2 fallback
3. **Enable Shader Stripping** - Reduce shader variant count
4. **Set VSync to 1** - Improve battery life
5. **Configure Texture Mipmap LOD** - Reduce texture memory at distance

### High Priority
6. **Implement LOD System** - Reduce geometry at distance
7. **Implement Object Pooling** - Reduce GC allocations
8. **Optimize UI Canvas** - Improve UI performance
9. **Implement Safe Area Support** - Support notches and cutouts
10. **Bake Lighting** - Reduce runtime lighting cost

### Medium Priority
11. **Implement Audio Priority System** - Limit audio voices
12. **Create Texture Atlases** - Reduce draw calls
13. **Optimize Meshes** - Reduce triangle count
14. **Implement Particle Culling** - Reduce particle cost
15. **Compress Audio** - Reduce audio memory

### Low Priority
16. **Implement Texture Streaming** - For large scenes
17. **Implement Asset Bundles** - For dynamic loading
18. **Implement Addressables** - For memory management
19. **Optimize Network Requests** - Batch and compress
20. **Implement Quality Scaling** - Dynamic quality adjustment

## Android Permissions

### Current Status
- **INTERNET**: Not explicitly forced (may be needed)
- **ACCESS_NETWORK_STATE**: Not explicitly forced (may be needed)
- **VIBRATE**: Not configured
- **Location Permissions**: Not configured
- **Camera Permission**: Not configured
- **Audio Recording**: Not configured
- **Storage**: Not configured

### Required Actions
1. **Review networking permissions** - Ensure INTERNET and ACCESS_NETWORK_STATE are available
2. **Review location permissions** - Add if location features are needed
3. **Review audio permissions** - Add if voice features are needed
4. **Review storage permissions** - Add if file access is needed
5. **Verify no unnecessary permissions** - Remove any not needed

### Security Verification
- **Server Secrets**: Not verified (requires code review)
- **API Keys**: Not verified (requires code review)
- **Certificate Pinning**: Not implemented
- **Code Obfuscation**: Not enabled (ProGuard/R8 should be enabled)

## Optimisation Implementation Plan

### Phase 1: Critical Configuration (Week 1)
- [ ] Set Android Target SDK to 34 (Android 14)
- [ ] Configure texture compression (ASTC 4x4)
- [ ] Enable shader variant stripping
- [ ] Set VSync to 1
- [ ] Configure texture mipmap LOD
- [ ] Verify Android permissions

### Phase 2: Asset Optimisation (Week 2-3)
- [ ] Compress all textures
- [ ] Create texture atlases for UI
- [ ] Implement LOD system (3 levels)
- [ ] Optimize meshes (reduce triangles)
- [ ] Bake lighting for static objects
- [ ] Compress audio files

### Phase 3: Runtime Optimisation (Week 4)
- [ ] Implement object pooling system
- [ ] Implement audio priority system
- [ ] Optimize UI canvas configuration
- [ ] Implement safe area support
- [ ] Implement particle culling
- [ ] Optimize scripts for GC

### Phase 4: Profiling and Tuning (Week 5)
- [ ] Profile on low-end device
- [ ] Profile on mid-range device
- [ ] Measure frame time and FPS
- [ ] Identify GPU/CPU bottlenecks
- [ ] Measure draw calls
- [ ] Measure memory usage
- [ ] Tune performance budgets
- [ ] Test thermal behaviour
- [ ] Measure battery impact

## Verification Checklist

### Before Android Build
- [ ] Android Target SDK set to 34
- [ ] Texture compression configured
- [ ] Shader stripping enabled
- [ ] VSync set to 1
- [ ] Texture mipmap LOD configured
- [ ] LOD system implemented
- [ ] Object pooling implemented
- [ ] Audio priority system implemented
- [ ] UI canvas optimized
- [ ] Safe area support implemented
- [ ] Lighting baked
- [ ] Android permissions verified
- [ ] No server secrets in client code
- [ ] ProGuard/R8 enabled

### After Android Build
- [ ] Build succeeds without errors
- [ ] APK size acceptable (< 150 MB)
- [ ] App installs on target device
- [ ] App launches successfully
- [ ] Authentication works
- [ ] Networking works
- [ ] UI displays correctly
- [ ] Touch controls work
- [ ] Safe areas work correctly

### Performance Verification
- [ ] Frame time measured on low-end device
- [ ] FPS measured on low-end device (target: 30 FPS)
- [ ] Frame time measured on mid-range device
- [ ] FPS measured on mid-range device (target: 60 FPS)
- [ ] Draw calls measured and within budget
- [ ] Memory usage measured and within budget
- [ ] Thermal behaviour tested (no throttling)
- [ ] Battery usage measured and acceptable
- [ ] Scene loading time measured (< 3 seconds)
- [ ] GC allocations measured and within budget

## Known Issues

### Configuration Issues
1. **Android Target SDK not set** - Must configure before build
2. **Shader stripping not enabled** - Will increase build size and memory
3. **VSync set to 0** - Will drain battery faster
4. **Texture compression not configured** - Will increase memory usage

### Implementation Gaps
1. **No LOD system** - Will cause high triangle count
2. **No object pooling** - Will cause GC spikes
3. **No safe area support** - Will cause UI issues on notched devices
4. **No audio priority system** - May cause audio issues
5. **No performance profiling** - Cannot verify performance budgets

### Security Concerns
1. **Server secrets not verified** - Requires code review
2. **API keys not verified** - Requires code review
3. **Certificate pinning not implemented** - Security risk
4. **Code obfuscation not enabled** - Security risk

## Recommendations

### Immediate Actions
1. Set Android Target SDK to latest stable (34)
2. Configure texture compression (ASTC)
3. Enable shader variant stripping
4. Set VSync to 1 for battery saving
5. Review and configure Android permissions
6. Review code for server secrets and API keys

### Short-Term Actions
1. Implement LOD system
2. Implement object pooling
3. Implement safe area support
4. Compress all textures
5. Bake lighting
6. Optimize UI canvas

### Long-Term Actions
1. Profile on actual target devices
2. Tune performance budgets
3. Implement quality scaling
4. Implement asset bundles
5. Implement addressables
6. Monitor thermal and battery behaviour

## Conclusion

The NAAD project has been inspected for mobile optimisation and Android verification. Unity configuration is mostly suitable for mobile development, but several critical configuration changes are required before building for Android.

**Critical Issues**:
- Android Target SDK not set
- Texture compression not configured
- Shader stripping not enabled
- VSync set to 0 (battery drain)
- No LOD system
- No object pooling
- No safe area support

**Action Required**: Implement Phase 1 critical configuration changes before attempting Android build. Then proceed with Phase 2-4 optimisations and verify performance on target devices.

**Note**: Performance budgets are estimates based on typical device specifications. Actual performance must be measured on target devices to verify budgets are met.
