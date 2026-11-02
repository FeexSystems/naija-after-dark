using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace NAAD.Editor
{
    public static class CompleteGateC
    {
        [MenuItem("NAAD/Complete Gate C Setup")]
        public static void Run()
        {
            var rigPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx";
            var prefabPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.prefab";
            var controllerPath = "Assets/NAAD/Animation/Controllers/PlayerController.controller";
            var maskPath = "Assets/NAAD/Animation/Avatars/UpperBodyMask.mask";

            // Step 1: Create PlayerRig prefab from FBX
            var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (rigPrefab == null)
            {
                var fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(rigPath);
                if (fbxObject == null)
                {
                    Debug.LogError($"Failed to load {rigPath}");
                    EditorApplication.Exit(1);
                    return;
                }

                rigPrefab = PrefabUtility.SaveAsPrefabAsset(fbxObject, prefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[NAAD][GateC] Created prefab at {prefabPath}");
            }
            else
            {
                Debug.Log($"[NAAD][GateC] Prefab already exists at {prefabPath}");
            }

            // Step 2: Add upper body layer to Animator Controller
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);

            if (controller == null)
            {
                Debug.LogError("PlayerController not found");
                EditorApplication.Exit(1);
                return;
            }

            if (mask == null)
            {
                Debug.LogError("UpperBodyMask not found");
                EditorApplication.Exit(1);
                return;
            }

            var hasUpperBodyLayer = false;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == "Upper Body")
                {
                    hasUpperBodyLayer = true;
                    break;
                }
            }

            if (!hasUpperBodyLayer)
            {
                controller.AddLayer("Upper Body");
                var layers = controller.layers;
                layers[1].avatarMask = mask;
                layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
                layers[1].defaultWeight = 1f;
                AssetDatabase.SaveAssets();
                Debug.Log("[NAAD][GateC] Added Upper Body layer with mask to PlayerController");
            }
            else
            {
                Debug.Log("[NAAD][GateC] Upper Body layer already exists in PlayerController");
            }

            Debug.Log("[NAAD][GateC] Gate C batch setup complete. Manual steps remaining:");
            Debug.Log("1. Configure PlayerRig.fbx as Humanoid in Inspector");
            Debug.Log("2. Extract and save Avatar as PlayerAvatar.asset");
            Debug.Log("3. Add components to PlayerRig prefab (Animator, CharacterAnimationController, AnimationStateManager, CharacterController, PlayerLocomotionController)");
            Debug.Log("4. Configure Animator to use PlayerController and PlayerAvatar");
            Debug.Log("5. Run NAAD > Verify Animation Assets");
            EditorApplication.Exit(0);
        }
    }
}
