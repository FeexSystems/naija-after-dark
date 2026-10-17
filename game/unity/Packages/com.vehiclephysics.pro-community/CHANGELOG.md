# Changelog
All notable changes to this package will be documented in this file.
[Full Changelog](https://vehiclephysics.com/about/changelog/)

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.7.1] - 2026-02-04

*First UPM release of \<Vehicle Physics Pro - Community Edition\>. The package is now fully compatible with Unity's Package Manager*.

## [1.7.0] - 2025-08-13

*Vehicle Physics Pro engine updated to latest SDK v9.8.
[Release Notes - SDK v9.8](https://vehiclephysics.com/about/release-notes-sdk-v9.8)*

### Added

- Compatibility with new Input System.

### Changed

- Vehicle Physics Pro no longer requires the old Input Manager to be enabled. All the input functionality works the same regardless the active input method.
- VeraMono font replaced with DejaVuSansMono.
- Minor improvements and fixes.

## [1.6.0] - 2025-06-20

*Vehicle Physics Pro engine updated to latest SDK v9.7.
[Release Notes - SDK v9.7](https://vehiclephysics.com/about/release-notes-sdk-v9.7)*

### Added

- Compatibility with Unity 6 API. When importing, allow the automatic script updater to modify the package files.
- New Brake Sensitivity feature in the Gearbox, for adjusting downshifts according to the brake pressure applied. Check the Sport Coupe.

### Changed

- Minimum Unity version supported: 2021.3.

### Fixed

- Fixed visual steering wheel not rotating.
- Sport Coupe prefab modified because stupid Unity 6 doesn't import overrides in nested prefabs correctly (IN-100900, UUM-104751).

## [1.5.0] - 2025-01-17

*Vehicle Physics Pro engine updated to latest SDK v9.6*

### Added

- New Engine Power Cap setting for realistic power curves in combustion engines.

### Changed

- Up to 50% more performance.
- Minimum Unity version supported: 2019.4.
- Various fixes and improvements.

## [1.4.0] - 2024-10-01

*Vehicle Physics Pro engine updated to latest SDK v9.5.
[Release Notes and Upgrade Guide - SDK v9.5](https://vehiclephysics.com/about/release-notes-sdk-v9.5)*

### Added

- New contact point model
- Engine: New starting engine sound effect (component VPAudioExtras).
- Camera Orbit mode now supports Target Relative rotation and panning.

### Changed

- Improved wheel integration model
- Improved rolling friction model
- Wheel radius can now change dynamically.
- Brake-assist driving aids improved (ABS, ASR, ESC).
- Gearbox: Relaxed speed requirement to engage/disengage park mode.
- TireFriction: Default values now represent standard street tires.
- VPRollingFriction: Now applies rolling friction within the wheel model instead of applying external forces.
- VPTelemetry: Improved fuel consumption display.
- Camera Orbit mode: By default, right mouse button is now required to rotate the view.

### Fixed

- Transmission: Fixed misbehavior in locking/unlocking detachable differentials.
- Engine: Fixed behavior when RPM is negative.
- Engine: Friction alone can now keep the car stopped.
- VPAudioExtras: Fixed issue where pitch was not being applied.

## [1.3.0] - 2022-01-04

### Added

- New Limited Drive modes (D1-D5) in automatic gearbox. These modes work like the regular D mode but using up to the specified gear only (i.e. up to 3nd gear in D3).
- New Aerodynamic Drag Models added: cubic, mixed. Allows matching the simulated aerodynamic profile with actual data.
- Vehicles now support Floating Origin and soft reposition with preservation of the physics state.
- New Hard Reposition feature for translating/respawning vehicles while restarting their physics state.
- New Combined Retarder Brake option in the brakes section to apply retarder brake automatically from the brake pedal.
- New VPAudioExtras component for additional sound effects, starting with engine starting sound.
- New Limit Reverse Speed option in Speed Control section.
- New Inertia Tensor Matrix feature allowing to configure inertia using an actual inertia tensor matrix.
- New Throttle Sensitivity setting in the gearbox for automatic gear shifts based on throttle pressure.

### Changed

- VPTelemetry.showData renamed to VPTelemetry.showTelemetry.

### Fixed

- Removed errors on VR API deprecation.
- Fixed wheel suspension issues caused by Unity/PhysX faked sprung mass model.

## [1.2.0] - 2020-04-01

## [1.1.0] - 2019-11-22

## [1.0.0] - 2019-09-09