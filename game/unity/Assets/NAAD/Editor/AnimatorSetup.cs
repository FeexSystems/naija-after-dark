using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using NAAD.Characters.Player;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that creates the NAAD player Animator Controller asset with
    /// all required parameters, layers, and the base locomotion state machine.
    /// <para>
    /// Invoke via the menu: <b>NAAD &gt; Characters &gt; Setup Animator Controller</b>.
    /// </para>
    /// <para>
    /// After creation, assign animation clips to each state using actual animation
    /// assets (from Mixamo, Asset Store, or custom production). See
    /// <c>ANIMATION_MANIFEST.md</c> for the complete animation ID list.
    /// </para>
    /// </summary>
    public static class AnimatorSetup
    {
        private const string Folder = "Assets/NAAD/Characters/Player";
        private const string AssetName = "PlayerAnimator.controller";

        [MenuItem("NAAD/Characters/Setup Animator Controller", priority = 201)]
        public static void CreateAnimatorController()
        {
            if (!EnsureFolderExists()) return;

            var path = Path.Combine(Folder, AssetName);
            if (File.Exists(path))
            {
                if (!EditorUtility.DisplayDialog("Animator Controller Exists",
                    $"{path} already exists. Overwrite?", "Overwrite", "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(path);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // --- Parameters ---
            AddParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            AddParameter(controller, "Direction", AnimatorControllerParameterType.Float);
            AddParameter(controller, "IsRunning", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "SocialTrigger", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "PhoneTrigger", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "IsSeated", AnimatorControllerParameterType.Bool);

            // --- Layers ---
            // Layer 0: Base Locomotion (already exists as the default layer)
            ConfigureLocomotionLayer(controller.layers[0]);

            // Layer 1: Social overlay
            var socialLayer = controller.AddLayer("Social");
            ConfigureOverlayLayer(socialLayer);

            // Layer 2: Phone overlay
            var phoneLayer = controller.AddLayer("Phone");
            ConfigureOverlayLayer(phoneLayer);

            // Layer 3: Seating override
            var seatingLayer = controller.AddLayer("Seating");
            ConfigureSeatingLayer(seatingLayer);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AnimatorSetup] Animator Controller created: {path}");
        }

        private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            controller.AddParameter(name, type);
        }

        private static void ConfigureLocomotionLayer(AnimatorControllerLayer layer)
        {
            var sm = layer.stateMachine;

            var idle = sm.AddState("Idle");
            var walk = sm.AddState("Walk");
            var run = sm.AddState("Run");

            // Motion fields are left null here because animation clips must be
            // acquired externally (Mixamo, Asset Store, custom production).
            // Assign clips in the Animator window after creation.

            sm.defaultState = idle;

            // Transitions: Idle <-> Walk based on Speed parameter
            AddTransition(idle, walk, "Speed > 0.25");
            AddTransition(walk, idle, "Speed <= 0.25");

            // Transitions: Walk <-> Run based on IsRunning parameter
            AddTransition(walk, run, "IsRunning");
            AddTransition(run, walk, "!IsRunning");

            Debug.Log("[AnimatorSetup] Base locomotion layer configured (Idle, Walk, Run states).");
        }

        private static void ConfigureOverlayLayer(AnimatorControllerLayer layer)
        {
            var sm = layer.stateMachine;
            var empty = sm.AddState("Empty");
            sm.defaultState = empty;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
        }

        private static void ConfigureSeatingLayer(AnimatorControllerLayer layer)
        {
            var sm = layer.stateMachine;
            var seated = sm.AddState("Seated");
            var standing = sm.AddState("Standing");
            sm.defaultState = standing;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;

            AddTransition(standing, seated, "IsSeated");
            AddTransition(seated, standing, "!IsSeated");
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, string condition)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.25f;
            transition.offset = 0f;
            transition.interruptionSource = InterruptionSource.None;

            if (condition.Contains(">"))
            {
                var parts = condition.Split('>');
                var param = parts[0].Trim();
                var value = float.Parse(parts[1].Trim());
                transition.AddCondition(AnimatorConditionMode.Greater, value, param);
            }
            else if (condition.Contains("<="))
            {
                var parts = condition.Split('<');
                var param = parts[0].Trim().Trim('=');
                var value = float.Parse(parts[1].Trim());
                transition.AddCondition(AnimatorConditionMode.Less, value, param);
            }
            else if (condition.StartsWith("!"))
            {
                var param = condition.Substring(1).Trim();
                transition.AddCondition(AnimatorConditionMode.IfNot, 0f, param);
            }
            else
            {
                transition.AddCondition(AnimatorConditionMode.If, 0f, condition);
            }
        }

        private static bool EnsureFolderExists()
        {
            if (AssetDatabase.IsValidFolder(Folder)) return true;

            var parent = Path.GetDirectoryName(Folder);
            AssetDatabase.CreateFolder(parent, "Player");
            Debug.Log($"[AnimatorSetup] Created folder: {Folder}");
            return true;
        }
    }
}