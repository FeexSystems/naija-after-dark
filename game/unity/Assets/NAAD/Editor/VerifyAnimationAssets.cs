using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Headless-capable Gate C contract verifier. It checks real imported assets and
    /// Animator wiring; a file-existence-only pass is deliberately insufficient.
    /// </summary>
    public static class VerifyAnimationAssets
    {
        private const string Root = "Assets/NAAD/Animation/";
        private const string RigPath = Root + "Characters/Player/PlayerRig.fbx";
        private const string PrefabPath = Root + "Characters/Player/PlayerRig.prefab";
        private const string ControllerPath = Root + "Controllers/PlayerController.controller";
        private const string AvatarPath = Root + "Avatars/PlayerAvatar.asset";
        private const string MaskPath = Root + "Avatars/UpperBodyMask.mask";
        private const string ReportRelativePath = "Logs/animation-verification-report.json";

        private static readonly string[] RequiredClipPaths =
        {
            Root + "Animations/Locomotion/Idle.anim",
            Root + "Animations/Locomotion/Walk.anim",
            Root + "Animations/Locomotion/Run.anim",
            Root + "Animations/Phone/Retrieve.anim",
            Root + "Animations/Phone/Hold.anim",
            Root + "Animations/Phone/Browse.anim",
            Root + "Animations/Phone/PutAway.anim",
            Root + "Animations/Social/Greeting.anim"
        };

        [Serializable]
        private sealed class VerificationReport
        {
            public bool success;
            public int passed;
            public int failed;
            public string[] failures;
            public string generatedUtc;
        }

        [MenuItem("NAAD/Verify Animation Assets")]
        public static void RunVerification()
        {
            var failures = new List<string>();
            var passed = 0;

            Check("PlayerRig FBX exists", AssetDatabase.LoadAssetAtPath<GameObject>(RigPath) != null, failures, ref passed);
            Check("PlayerRig prefab exists", AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null, failures, ref passed);
            Check("PlayerController exists", AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null, failures, ref passed);
            Check("PlayerAvatar exists", AssetDatabase.LoadAssetAtPath<Avatar>(AvatarPath) != null, failures, ref passed);
            Check("UpperBodyMask exists", AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath) != null, failures, ref passed);

            var importer = AssetImporter.GetAtPath(RigPath) as ModelImporter;
            var humanoidImport = importer != null && importer.animationType == ModelImporterAnimationType.Human;
            Check("PlayerRig is imported as Humanoid", humanoidImport, failures, ref passed);

            var importedAvatar = AssetDatabase.LoadAllAssetsAtPath(RigPath)
                .OfType<Avatar>()
                .FirstOrDefault(avatar => avatar != null && avatar.isValid && avatar.isHuman);
            Check("PlayerRig contains a valid humanoid Avatar", importedAvatar != null, failures, ref passed);

            foreach (var clipPath in RequiredClipPaths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                Check("AnimationClip is valid: " + clipPath, clip != null && clip.length > 0f, failures, ref passed);
            }

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            var requiredParameters = new Dictionary<string, AnimatorControllerParameterType>
            {
                { "LocomotionSpeed", AnimatorControllerParameterType.Float },
                { "LocomotionState", AnimatorControllerParameterType.Int },
                { "SocialState", AnimatorControllerParameterType.Int },
                { "PhoneState", AnimatorControllerParameterType.Int },
                { "SeatingState", AnimatorControllerParameterType.Int },
                { "Trigger", AnimatorControllerParameterType.Trigger }
            };

            if (controller != null)
            {
                var parameters = controller.parameters;
                foreach (var expected in requiredParameters)
                {
                    var actual = parameters.FirstOrDefault(parameter => parameter.name == expected.Key);
                    Check(
                        "Animator parameter " + expected.Key + " has type " + expected.Value,
                        actual != null && actual.type == expected.Value,
                        failures,
                        ref passed);
                }

                var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
                var hasMaskedOverlay = mask != null && controller.layers.Any(
                    layer => layer.avatarMask == mask && layer.name != "Base Layer");
                Check("Animator has a non-base layer using UpperBodyMask", hasMaskedOverlay, failures, ref passed);

                var controllerClips = controller.animationClips;
                foreach (var clipPath in RequiredClipPaths)
                {
                    var requiredClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    Check(
                        "PlayerController references required clip: " + clipPath,
                        requiredClip != null && controllerClips.Contains(requiredClip),
                        failures,
                        ref passed);
                }
            }
            else
            {
                Check("Animator parameter contract can be inspected", false, failures, ref passed);
                Check("Animator overlay layer can be inspected", false, failures, ref passed);
            }

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var animator = playerPrefab != null ? playerPrefab.GetComponentInChildren<Animator>(true) : null;
            Check("PlayerRig prefab contains an Animator", animator != null, failures, ref passed);
            Check(
                "PlayerRig prefab Animator references PlayerController",
                animator != null && animator.runtimeAnimatorController == controller && controller != null,
                failures,
                ref passed);
            Check(
                "PlayerRig prefab Animator uses a valid humanoid Avatar",
                animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                failures,
                ref passed);
            Check(
                "PlayerRig prefab contains CharacterAnimationController",
                playerPrefab != null && playerPrefab.GetComponentInChildren<NAAD.Animation.CharacterAnimationController>(true) != null,
                failures,
                ref passed);
            Check(
                "PlayerRig prefab contains AnimationStateManager",
                playerPrefab != null && playerPrefab.GetComponentInChildren<NAAD.Animation.AnimationStateManager>(true) != null,
                failures,
                ref passed);
            Check(
                "PlayerRig prefab contains CharacterController for script-driven movement",
                playerPrefab != null && playerPrefab.GetComponentInChildren<CharacterController>(true) != null,
                failures,
                ref passed);
            Check(
                "PlayerRig prefab contains PlayerLocomotionController",
                playerPrefab != null && playerPrefab.GetComponentInChildren<NAAD.Animation.PlayerLocomotionController>(true) != null,
                failures,
                ref passed);

            var report = new VerificationReport
            {
                success = failures.Count == 0,
                passed = passed,
                failed = failures.Count,
                failures = failures.ToArray(),
                generatedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            WriteReport(report);

            if (report.success)
                Debug.Log($"[NAAD][GateC] PASS: {passed} animation/rig contract checks.");
            else
                Debug.LogError($"[NAAD][GateC] FAIL: {report.failed} of {passed + report.failed} checks failed. See {ReportRelativePath}.");

            if (Application.isBatchMode)
                EditorApplication.Exit(report.success ? 0 : 1);
        }

        private static void Check(string name, bool condition, List<string> failures, ref int passed)
        {
            if (condition)
            {
                passed++;
                Debug.Log("[NAAD][GateC] PASS: " + name);
                return;
            }

            failures.Add(name);
            Debug.LogError("[NAAD][GateC] FAIL: " + name);
        }

        private static void WriteReport(VerificationReport report)
        {
            try
            {
                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot))
                    throw new InvalidOperationException("Unable to resolve Unity project root.");

                var reportPath = Path.Combine(projectRoot, ReportRelativePath);
                var reportDirectory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(reportDirectory))
                    Directory.CreateDirectory(reportDirectory);

                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Debug.Log("[NAAD][GateC] Wrote report: " + ReportRelativePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[NAAD][GateC] Could not write report: " + exception.Message);
            }
        }
    }
}
