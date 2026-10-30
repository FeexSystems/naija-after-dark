using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using NAAD.Animation;

namespace NAAD.Editor
{
    /// <summary>
    /// Promotes the committed player source model into the strict Gate C layout
    /// and creates the deterministic controller, upper-body mask, and prefab
    /// wiring. Animation clips remain external inputs and are never fabricated.
    /// </summary>
    public static class ConfigureGateCAssets
    {
        private const string SourceRigPath = "Assets/NAAD/Content/Models/STD/CHR_Player_STD_v001.fbx";
        private const string RigPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx";
        private const string PrefabPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.prefab";
        private const string ControllerPath = "Assets/NAAD/Animation/Controllers/PlayerController.controller";
        private const string AvatarPath = "Assets/NAAD/Animation/Avatars/PlayerAvatar.asset";
        private const string MaskPath = "Assets/NAAD/Animation/Avatars/UpperBodyMask.mask";

        [MenuItem("NAAD/Gate C/Configure Player Rig Wiring", priority = 100)]
        public static void Configure()
        {
            EnsureFolders();

            if (!File.Exists(SourceRigPath))
                throw new InvalidOperationException("Gate C source rig is missing: " + SourceRigPath);

            PromoteSourceRig();
            CreatePlayerAvatar();
            var mask = CreateUpperBodyMask();
            var controller = CreateController(mask);
            CreatePlayerPrefab(controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[NAAD][GateC] Player rig wiring configured. Animation clips remain pending external assets.");
        }

        public static void ConfigureAndVerify()
        {
            Configure();
            VerifyAnimationAssets.RunVerification();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/NAAD/Animation");
            EnsureFolder("Assets/NAAD/Animation/Characters");
            EnsureFolder("Assets/NAAD/Animation/Characters/Player");
            EnsureFolder("Assets/NAAD/Animation/Controllers");
            EnsureFolder("Assets/NAAD/Animation/Avatars");
            EnsureFolder("Assets/NAAD/Animation/Animations");
            EnsureFolder("Assets/NAAD/Animation/Animations/Locomotion");
            EnsureFolder("Assets/NAAD/Animation/Animations/Phone");
            EnsureFolder("Assets/NAAD/Animation/Animations/Social");
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            var name = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Invalid Unity folder path: " + folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void PromoteSourceRig()
        {
            var importer = AssetImporter.GetAtPath(RigPath) as ModelImporter;
            if (importer == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(RigPath) != null)
                    AssetDatabase.DeleteAsset(RigPath);

                File.Copy(SourceRigPath, RigPath, true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                importer = AssetImporter.GetAtPath(RigPath) as ModelImporter;
            }

            if (importer == null)
                throw new InvalidOperationException("Promoted player rig is not a ModelImporter: " + RigPath);

            // Keep Unity's complete imported skeleton description. Replacing it with
            // an empty array can make an otherwise correctly mapped Mixamo rig fail
            // Avatar validation because the importer no longer has the bind-pose
            // skeleton data needed to build the humanoid Avatar.
            var importedSkeleton = importer.humanDescription.skeleton;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.autoGenerateAvatarMappingIfUnspecified = true;
            var humanDescription = BuildPlayerHumanDescription();
            var hierarchySkeleton = BuildSkeletonFromImportedModel();
            if (hierarchySkeleton.Length > 0)
                humanDescription.skeleton = hierarchySkeleton;
            else if (importedSkeleton != null && importedSkeleton.Length > 0)
                humanDescription.skeleton = importedSkeleton;
            var diagnosticModel = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
            if (diagnosticModel != null)
            {
                var diagnosticAvatar = AvatarBuilder.BuildHumanAvatar(diagnosticModel, humanDescription);
                Debug.Log($"[NAAD][GateC] AvatarBuilder diagnostic: valid={diagnosticAvatar != null && diagnosticAvatar.isValid}, human={diagnosticAvatar != null && diagnosticAvatar.isHuman}, humanBones={humanDescription.human?.Length ?? 0}, skeletonBones={humanDescription.skeleton?.Length ?? 0}");
                if (diagnosticAvatar == null || !diagnosticAvatar.isValid || !diagnosticAvatar.isHuman)
                    Debug.LogError("[NAAD][GateC] AvatarBuilder rejected the explicit human description before FBX reimport.");
            }
            importer.humanDescription = humanDescription;
            importer.importAnimation = true;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(RigPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log($"[NAAD][GateC] Humanoid import configured: animationType={importer.animationType}, avatarSetup={importer.avatarSetup}, skeletonBones={humanDescription.skeleton?.Length ?? 0}, autoMapping={importer.autoGenerateAvatarMappingIfUnspecified}");
        }

        private static SkeletonBone[] BuildSkeletonFromImportedModel()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
            if (model == null)
                return new SkeletonBone[0];

            var bones = new List<SkeletonBone>();
            AppendSkeletonBone(model.transform, bones);
            return bones.ToArray();
        }

        private static void AppendSkeletonBone(Transform transform, List<SkeletonBone> bones)
        {
            bones.Add(new SkeletonBone
            {
                name = transform.name,
                position = transform.localPosition,
                rotation = transform.localRotation,
                scale = transform.localScale
            });

            for (var index = 0; index < transform.childCount; index++)
                AppendSkeletonBone(transform.GetChild(index), bones);
        }

        private static HumanDescription BuildPlayerHumanDescription()
        {
            return new HumanDescription
            {
                human = new[]
                {
                    HumanBone("Hips", "mixamorigHips"),
                    HumanBone("Spine", "mixamorigSpine"),
                    HumanBone("Chest", "mixamorigSpine1"),
                    HumanBone("UpperChest", "mixamorigSpine2"),
                    HumanBone("Neck", "mixamorigNeck"),
                    HumanBone("Head", "mixamorigHead"),
                    HumanBone("LeftShoulder", "mixamorigLeftShoulder"),
                    HumanBone("LeftUpperArm", "mixamorigLeftArm"),
                    HumanBone("LeftLowerArm", "mixamorigLeftForeArm"),
                    HumanBone("LeftHand", "mixamorigLeftHand"),
                    HumanBone("RightShoulder", "mixamorigRightShoulder"),
                    HumanBone("RightUpperArm", "mixamorigRightArm"),
                    HumanBone("RightLowerArm", "mixamorigRightForeArm"),
                    HumanBone("RightHand", "mixamorigRightHand"),
                    HumanBone("LeftUpperLeg", "mixamorigLeftUpLeg"),
                    HumanBone("LeftLowerLeg", "mixamorigLeftLeg"),
                    HumanBone("LeftFoot", "mixamorigLeftFoot"),
                    HumanBone("LeftToes", "mixamorigLeftToeBase"),
                    HumanBone("RightUpperLeg", "mixamorigRightUpLeg"),
                    HumanBone("RightLowerLeg", "mixamorigRightLeg"),
                    HumanBone("RightFoot", "mixamorigRightFoot"),
                    HumanBone("RightToes", "mixamorigRightToeBase")
                },
                skeleton = null,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0.0f,
                hasTranslationDoF = false
            };
        }

        private static HumanBone HumanBone(string humanName, string boneName)
        {
            return new HumanBone
            {
                humanName = humanName,
                boneName = boneName,
                limit = new HumanLimit { useDefaultValues = true }
            };
        }

        private static void CreatePlayerAvatar()
        {
            // Reuse a previously validated independent avatar asset.
            var existingAsset = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            if (existingAsset != null && existingAsset.isValid && existingAsset.isHuman)
            {
                Debug.Log("[NAAD][GateC] Reusing existing PlayerAvatar.asset (valid humanoid).");
                return;
            }

            // Prefer an Avatar sub-asset generated by ModelImporter (CreateFromThisModel).
            var avatar = FindValidHumanoidAvatarOnRig();
            string stage = "ModelImporter.subAsset";

            // Headless Linux CI frequently imports the FBX without embedding an Avatar
            // sub-asset even when AvatarBuilder accepts the same hierarchy in-memory.
            // Fall back to explicit AvatarBuilder and persist PlayerAvatar.asset.
            if (avatar == null)
            {
                stage = "AvatarBuilder.BuildHumanAvatar";
                avatar = BuildHumanoidAvatarFromHierarchy();
            }

            if (avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                var imported = AssetDatabase.LoadAllAssetsAtPath(RigPath)
                    .OfType<Avatar>()
                    .Where(candidate => candidate != null)
                    .Select(candidate => $"{candidate.name}(valid={candidate.isValid},human={candidate.isHuman})");
                Debug.LogError(
                    "[NAAD][GateC] Avatar validation failed."
                    + " asset=" + RigPath
                    + " stage=" + stage
                    + " diagnostic=no valid humanoid Avatar"
                    + " importedSubAssets=[" + string.Join(", ", imported) + "]");
                throw new InvalidOperationException(
                    "Gate C Avatar failure at stage " + stage + " for asset " + RigPath);
            }

            if (AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath) != null)
                AssetDatabase.DeleteAsset(AvatarPath);

            // Persist a standalone Avatar so verification and the prefab do not depend
            // on FBX sub-asset generation succeeding in every environment.
            var savedAvatar = UnityEngine.Object.Instantiate(avatar);
            savedAvatar.name = "PlayerAvatar";
            AssetDatabase.CreateAsset(savedAvatar, AvatarPath);
            AssetDatabase.SaveAssets();

            var reloaded = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
            if (reloaded == null || !reloaded.isValid || !reloaded.isHuman)
                throw new InvalidOperationException(
                    "Gate C Avatar failure at stage PlayerAvatar.asset.save for asset " + AvatarPath);

            Debug.Log(
                "[NAAD][GateC] PlayerAvatar.asset saved from stage=" + stage
                + " valid=" + reloaded.isValid + " human=" + reloaded.isHuman);
        }

        private static Avatar FindValidHumanoidAvatarOnRig()
        {
            return AssetDatabase.LoadAllAssetsAtPath(RigPath)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate != null && candidate.isValid && candidate.isHuman);
        }

        private static Avatar BuildHumanoidAvatarFromHierarchy()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
            if (model == null)
            {
                Debug.LogError("[NAAD][GateC] Cannot BuildHumanAvatar — model missing: " + RigPath);
                return null;
            }

            var description = BuildPlayerHumanDescription();
            var hierarchySkeleton = BuildSkeletonFromImportedModel();
            if (hierarchySkeleton.Length > 0)
                description.skeleton = hierarchySkeleton;
            else
            {
                var importer = AssetImporter.GetAtPath(RigPath) as ModelImporter;
                if (importer != null && importer.humanDescription.skeleton != null
                    && importer.humanDescription.skeleton.Length > 0)
                    description.skeleton = importer.humanDescription.skeleton;
            }

            var built = AvatarBuilder.BuildHumanAvatar(model, description);
            if (built == null)
            {
                Debug.LogError("[NAAD][GateC] AvatarBuilder.BuildHumanAvatar returned null for " + RigPath);
                return null;
            }

            Debug.Log(
                "[NAAD][GateC] AvatarBuilder fallback: valid=" + built.isValid
                + " human=" + built.isHuman
                + " humanBones=" + (description.human != null ? description.human.Length : 0)
                + " skeletonBones=" + (description.skeleton != null ? description.skeleton.Length : 0));
            return built;
        }

        private static AvatarMask CreateUpperBodyMask()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
            if (existing != null) return existing;

            var mask = new AvatarMask();
            mask.name = "UpperBodyMask";
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Root, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
            AssetDatabase.CreateAsset(mask, MaskPath);
            return mask;
        }

        private static AnimatorController CreateController(AvatarMask upperBodyMask)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            EnsureParameter(controller, "LocomotionSpeed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "LocomotionState", AnimatorControllerParameterType.Int);
            EnsureParameter(controller, "SocialState", AnimatorControllerParameterType.Int);
            EnsureParameter(controller, "PhoneState", AnimatorControllerParameterType.Int);
            EnsureParameter(controller, "SeatingState", AnimatorControllerParameterType.Int);
            EnsureParameter(controller, "Trigger", AnimatorControllerParameterType.Trigger);

            var baseLayer = controller.layers[0];
            EnsureState(baseLayer.stateMachine, "Idle");
            EnsureState(baseLayer.stateMachine, "Walk");
            EnsureState(baseLayer.stateMachine, "Run");
            baseLayer.stateMachine.defaultState = FindState(baseLayer.stateMachine, "Idle");
            controller.layers = ReplaceLayer(controller.layers, 0, baseLayer);

            var social = EnsureLayer(controller, "Social", upperBodyMask);
            EnsureState(social.stateMachine, "None");
            EnsureState(social.stateMachine, "Greeting");
            social.stateMachine.defaultState = FindState(social.stateMachine, "None");

            var phone = EnsureLayer(controller, "Phone", upperBodyMask);
            EnsureState(phone.stateMachine, "None");
            EnsureState(phone.stateMachine, "Retrieve");
            EnsureState(phone.stateMachine, "Hold");
            EnsureState(phone.stateMachine, "Browse");
            EnsureState(phone.stateMachine, "PutAway");
            phone.stateMachine.defaultState = FindState(phone.stateMachine, "None");

            var seating = EnsureLayer(controller, "Seating", null);
            EnsureState(seating.stateMachine, "None");
            EnsureState(seating.stateMachine, "Sit");
            EnsureState(seating.stateMachine, "IdleSeated");
            EnsureState(seating.stateMachine, "Stand");
            seating.stateMachine.defaultState = FindState(seating.stateMachine, "None");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (var parameter in controller.parameters)
                if (parameter.name == name) return;
            controller.AddParameter(name, type);
        }

        private static AnimatorControllerLayer EnsureLayer(AnimatorController controller, string name, AvatarMask mask)
        {
            foreach (var layer in controller.layers)
                if (layer.name == name)
                {
                    layer.avatarMask = mask;
                    layer.blendingMode = AnimatorLayerBlendingMode.Override;
                    controller.layers = ReplaceLayer(controller.layers, Array.IndexOf(controller.layers, layer), layer);
                    return layer;
                }

            controller.AddLayer(name);
            foreach (var layer in controller.layers)
                if (layer.name == name)
                {
                    layer.avatarMask = mask;
                    layer.blendingMode = AnimatorLayerBlendingMode.Override;
                    controller.layers = ReplaceLayer(controller.layers, Array.IndexOf(controller.layers, layer), layer);
                    return layer;
                }

            throw new InvalidOperationException("Could not create Animator layer: " + name);
        }

        private static AnimatorControllerLayer[] ReplaceLayer(AnimatorControllerLayer[] layers, int index, AnimatorControllerLayer layer)
        {
            if (index >= 0 && index < layers.Length) layers[index] = layer;
            return layers;
        }

        private static AnimatorState EnsureState(AnimatorStateMachine machine, string name)
        {
            var existing = FindState(machine, name);
            return existing ?? machine.AddState(name);
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            foreach (var state in machine.states)
                if (state.state.name == name) return state.state;
            return null;
        }

        private static void CreatePlayerPrefab(AnimatorController controller)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
            if (source == null)
                throw new InvalidOperationException("Promoted player rig could not be loaded: " + RigPath);

            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate player rig: " + RigPath);

            try
            {
                instance.name = "PlayerRig";
                var animator = instance.GetComponentInChildren<Animator>(true) ?? instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                var humanoidAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath);
                if (humanoidAvatar != null && humanoidAvatar.isValid && humanoidAvatar.isHuman)
                    animator.avatar = humanoidAvatar;
                else
                    throw new InvalidOperationException(
                        "Gate C prefab wiring requires a valid PlayerAvatar.asset at " + AvatarPath);

                if (instance.GetComponent<CharacterController>() == null)
                    instance.AddComponent<CharacterController>();
                if (instance.GetComponent<CharacterAnimationController>() == null)
                    instance.AddComponent<CharacterAnimationController>();
                if (instance.GetComponent<AnimationStateManager>() == null)
                    instance.AddComponent<AnimationStateManager>();
                if (instance.GetComponent<PlayerLocomotionController>() == null)
                    instance.AddComponent<PlayerLocomotionController>();

                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
