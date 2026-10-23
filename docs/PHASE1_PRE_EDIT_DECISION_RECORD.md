# Phase 1 Pre-Edit Decision Record

- Record ID: P1-001
- Date: 2026-10-22
- Repository: FeexSystems/naija-after-dark
- Baseline commit: 2c56797e644d74607955fb94a7543903ddb27bea
- Branch: chore/phase1-pre-edit-decision-record
- Status: Approved direction; implementation must remain isolated from main.

## Approved decisions

1. Retain Unity Editor 6000.7.0b4 as the project target. Investigate whether the exact GameCI runner/editor image and package set support this version before changing the target.
2. Restore .gitmodules metadata for the Blender asset-manager submodule only after verifying the upstream source and exact commit.
3. Classify Unity MCP packages and their dependencies before removal or isolation. Preserve them until a safe boundary is evidenced.
4. Do not merge or deploy. Do not discard local changes. No claim of a gate passing without fresh verification evidence.

## Evidence at baseline

- ProjectSettings/ProjectVersion.txt declares Unity 6000.7.0b4.
- .github/workflows/naad-unity.yml pins all five Unity build invocations to 6000.6.5f1.
- CI run https://github.com/FeexSystems/naija-after-dark/actions/runs/39794296448 failed during package resolution under Unity 6000.6.5f1. Reported unresolved dependencies include com.unity.inputsystem@6.7.0, com.unity.testtools@2.0.0, com.unity.xr.management@6.0.0, com.unity.modules.localizationruntime@1.0.0, com.unity.modules.smartstrings@1.0.0, com.unity.modules.spritemask@1.0.0, and com.rankupgames.unity-cursor-toolkit@1.0.0.
- The lockfile has seven manifest dependencies missing from lock entries and a Newtonsoft JSON direct-version mismatch (manifest 3.2.1 vs lock 3.2.2).
- tools/unity-cloud-asset-manager-for-blender is a Git submodule gitlink at 590374b3ec7af2f4c475c7b092756074081de0cc, but .gitmodules is absent. Upstream URL verified: https://github.com/Unity-Technologies/unity-cloud-asset-manager-for-blender.git. Upstream README marks the add-on deprecated and says it does not support recent Blender versions.
- game/unity/Assets/NAAD/Animation/VERIFICATION_REPORT.md says 0/37 assets are present while checkboxes for missing assets are marked [X].
- Local working-tree state and local Unity Editor execution are not available from this remote inspection. No local state is assumed clean.

## Phase 1 sequence

1. Preserve this record as the pre-edit baseline.
2. Restore verified submodule metadata in an isolated commit.
3. Test exact Unity 6000.7.0b4 CI support; inspect GameCI image/runner support and retain the beta target unless evidence requires a separate decision.
4. Resolve package IDs, registry availability, Unity-version compatibility, manifest/lock consistency, and MCP tooling status in evidence-backed small changes.
5. Run clean-checkout CI and review actual logs/artifacts. Do not proceed to player/animation gates until the Unity package/import/compile gate is green.

## Stop conditions

- Stop if Unity 6000.7.0b4 cannot be obtained or launched in the chosen CI path; report evidence and options instead of silently downgrading.
- Stop before removing a package unless code, scene, prefab, asset GUID, editor workflow, and tool usage searches support removal or safe isolation.
- Stop if the verified submodule source/commit changes or cannot be fetched.
- Stop before any merge, deployment, production mutation, or destructive cleanup.
