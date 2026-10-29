using System;
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

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.SaveAndReimport();
        }

        private static void CreatePlayerAvatar()
        {
            if (AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath) != null)
                return;

            var avatar = AssetDatabase.LoadAllAssetsAtPath(RigPath)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate != null && candidate.isValid && candidate.isHuman);
            if (avatar == null)
                throw new InvalidOperationException("The promoted player rig does not contain a valid humanoid Avatar: " + RigPath);

            var savedAvatar = UnityEngine.Object.Instantiate(avatar);
            savedAvatar.name = "PlayerAvatar";
            AssetDatabase.CreateAsset(savedAvatar, AvatarPath);
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
