using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate C Asset Setup Script
    /// Imports PlayerRig as Humanoid, extracts/renames animation clips,
    /// creates Animator Controller, UpperBodyMask, and PlayerRig prefab.
    /// </summary>
    public class SetupGateCAssets
    {
        private const string Root = "Assets/NAAD/Animation/";
        private const string RigSourcePath = Root + "Character/PlayerRig.fbx";
        private const string RigTargetPath = Root + "Characters/Player/PlayerRig.fbx";
        private const string PrefabPath = Root + "Characters/Player/PlayerRig.prefab";
        private const string ControllerPath = Root + "Controllers/PlayerController.controller";
        private const string MaskPath = Root + "Avatars/UpperBodyMask.mask";
        private const string AnimationsRoot = Root + "Animations/";

        // Source FBX clips - these will be referenced directly in the Animator Controller
        private static readonly string[] LocomotionClips = new string[]
        {
            AnimationsRoot + "Locomotion/Standing Idle.fbx",
            AnimationsRoot + "Locomotion/Standard Walk.fbx",
            AnimationsRoot + "Locomotion/Standard Running.fbx"
        };

        private static readonly string[] PhoneClips = new string[]
        {
            AnimationsRoot + "Phone/Female Answers A Phone While Walking.fbx",
            AnimationsRoot + "Phone/Female Standing Talking On Phone.fbx",
            AnimationsRoot + "Phone/Texting On A Smart Phone While Standing.fbx"
        };

        private static readonly string[] SocialClips = new string[]
        {
            AnimationsRoot + "Social/Aj Waving Gesture.fbx"
        };

        [MenuItem("NAAD/Gate C/Setup All Assets")]
        public static void SetupAll()
        {
            Debug.Log("[NAAD][GateC] Starting asset setup...");

            try
            {
                EnsureDirectoriesExist();
                ImportPlayerRig();
                ImportAnimations();
                CreateAnimatorController();
                CreateUpperBodyMask();
                CreatePlayerPrefab();

                Debug.Log("[NAAD][GateC] Asset setup complete!");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"[NAAD][GateC] Setup failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void EnsureDirectoriesExist()
        {
            CreateDirectory(Root + "Characters/Player/");
            CreateDirectory(Root + "Controllers/");
            CreateDirectory(Root + "Avatars/");
            CreateDirectory(AnimationsRoot + "Locomotion/");
            CreateDirectory(AnimationsRoot + "Phone/");
            CreateDirectory(AnimationsRoot + "Social/");
        }

        private static void CreateDirectory(string path)
        {
            var parent = Path.GetDirectoryName(path);
            if (!Directory.Exists(parent))
                Directory.CreateDirectory(parent);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void ImportPlayerRig()
        {
            Debug.Log("[NAAD][GateC] Importing PlayerRig as Humanoid...");

            if (!File.Exists(RigSourcePath))
            {
                Debug.LogError($"[NAAD][GateC] Source rig not found: {RigSourcePath}");
                return;
            }

            // Move rig to target location if needed
            if (RigSourcePath != RigTargetPath)
            {
                var targetDir = Path.GetDirectoryName(RigTargetPath);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                if (File.Exists(RigTargetPath))
                    File.Delete(RigTargetPath);

                File.Copy(RigSourcePath, RigTargetPath, true);
                File.Copy(RigSourcePath + ".meta", RigTargetPath + ".meta", true);
            }

            // Configure importer
            var importer = AssetImporter.GetAtPath(RigTargetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[NAAD][GateC] Failed to get ModelImporter for {RigTargetPath}");
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.humanoidAvatarCreationType = ModelImporterHumanoidAvatarCreationType.CreateFromOtherModel;
            importer.animationCompressionSettings = ModelImporterAnimationCompressionSettings.Off;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();

            Debug.Log("[NAAD][GateC] PlayerRig imported as Humanoid");
        }

        private static void ImportAnimations()
        {
            Debug.Log("[NAAD][GateC] Configuring animation FBX import settings...");

            var allClips = LocomotionClips.Concat(PhoneClips).Concat(SocialClips).ToArray();

            foreach (var clipPath in allClips)
            {
                if (!File.Exists(clipPath))
                {
                    Debug.LogWarning($"[NAAD][GateC] Animation FBX not found: {clipPath}");
                    continue;
                }

                try
                {
                    var importer = AssetImporter.GetAtPath(clipPath) as ModelImporter;
                    if (importer == null)
                    {
                        Debug.LogWarning($"[NAAD][GateC] Failed to get importer for: {clipPath}");
                        continue;
                    }

                    importer.importAnimation = true;
                    importer.animationCompressionSettings = ModelImporterAnimationCompressionSettings.Off;
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    importer.SaveAndReimport();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NAAD][GateC] Failed to configure {clipPath}: {e.Message}");
                }
            }

            AssetDatabase.Refresh();
        }

        private static void CreateAnimatorController()
        {
            Debug.Log("[NAAD][GateC] Creating Animator Controller...");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

            // Add required parameters
            controller.AddParameter("LocomotionSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("LocomotionState", AnimatorControllerParameterType.Int);
            controller.AddParameter("SocialState", AnimatorControllerParameterType.Int);
            controller.AddParameter("PhoneState", AnimatorControllerParameterType.Int);
            controller.AddParameter("SeatingState", AnimatorControllerParameterType.Int);
            controller.AddParameter("Trigger", AnimatorControllerParameterType.Trigger);

            // Create base layer with blend tree
            var baseLayer = controller.layers[0];
            var stateMachine = baseLayer.stateMachine;

            // Load clips from FBX files
            var idleClip = LoadClipFromFBX(LocomotionClips[0], "Idle");
            var walkClip = LoadClipFromFBX(LocomotionClips[1], "Walk");
            var runClip = LoadClipFromFBX(LocomotionClips[2], "Run");

            if (idleClip != null)
            {
                var idleState = stateMachine.AddState("Idle");
                idleState.motion = idleClip;
                stateMachine.defaultState = idleState;
            }

            if (walkClip != null)
            {
                var walkState = stateMachine.AddState("Walk");
                walkState.motion = walkClip;
            }

            if (runClip != null)
            {
                var runState = stateMachine.AddState("Run");
                runState.motion = runClip;
            }

            Debug.Log("[NAAD][GateC] Animator Controller created");
        }

        private static AnimationClip LoadClipFromFBX(string fbxPath, string clipName)
        {
            try
            {
                // Try to load the clip with the FBX|ClipName syntax
                var clipPath = fbxPath + "|" + clipName;
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

                if (clip == null)
                {
                    // Fallback: try loading all assets from the FBX and find the clip
                    var allObjects = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                    clip = allObjects.OfType<AnimationClip>().FirstOrDefault();
                }

                if (clip == null)
                {
                    Debug.LogWarning($"[NAAD][GateC] Failed to load clip '{clipName}' from {fbxPath}");
                }

                return clip;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NAAD][GateC] Error loading clip from {fbxPath}: {e.Message}");
                return null;
            }
        }

        private static void CreateUpperBodyMask()
        {
            Debug.Log("[NAAD][GateC] Creating UpperBodyMask...");

            var mask = new AvatarMask();

            // Create upper body mask (exclude legs/hips)
            mask.transformCount = 1;
            mask.SetTransformActive(0, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHand, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHand, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFoot, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFoot, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftShoulder, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightShoulder, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Spine, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Chest, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.UpperChest, true);

            AssetDatabase.CreateAsset(mask, MaskPath);
            Debug.Log("[NAAD][GateC] UpperBodyMask created");
        }

        private static void CreatePlayerPrefab()
        {
            Debug.Log("[NAAD][GateC] Creating PlayerRig prefab...");

            var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigTargetPath);
            if (rigPrefab == null)
            {
                Debug.LogError($"[NAAD][GateC] Failed to load rig at: {RigTargetPath}");
                return;
            }

            var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            prefabInstance.name = "PlayerRig";

            // Add Animator
            var animator = prefabInstance.GetComponent<Animator>();
            if (animator == null)
                animator = prefabInstance.AddComponent<Animator>();

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            animator.runtimeAnimatorController = controller;

            // Load avatar from the rig
            var avatar = AssetDatabase.LoadAllAssetsAtPath(RigTargetPath)[0] as Avatar;
            if (avatar != null && avatar.isValid)
                animator.avatar = avatar;

            animator.applyRootMotion = false;

            // Add CharacterAnimationController
            var animController = prefabInstance.AddComponent<NAAD.Animation.CharacterAnimationController>();

            // Add AnimationStateManager
            var stateManager = prefabInstance.AddComponent<NAAD.Animation.AnimationStateManager>();

            // Add CharacterController
            var charController = prefabInstance.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.radius = 0.3f;
            charController.center = new Vector3(0, 0.9f, 0);

            // Add PlayerLocomotionController
            var locomotion = prefabInstance.AddComponent<NAAD.Animation.PlayerLocomotionController>();

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabInstance, PrefabPath);
            GameObject.DestroyImmediate(prefabInstance);

            Debug.Log("[NAAD][GateC] PlayerRig prefab created");
        }
    }
}
