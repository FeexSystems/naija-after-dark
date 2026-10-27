using System.IO;
using UnityEditor;
using UnityEngine;
using NAAD.Animation;
using NAAD.Core.Bootstrap;
using NAAD.Gameplay.Interaction;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that creates the NAAD player character prefab for Gate D:
    /// PlayerLocomotionController + CharacterAnimationController + AnimationStateManager +
    /// InteractionSystem, all wired into a single actor. Matches the animation contract
    /// enforced by VerifyAnimationAssets and the interaction contract enforced by
    /// InteractionSystem. The controller is presentation-only; all authoritative state
    /// (location, relationships, money, dialogue) goes through ICommandService.
    /// <para>
    /// Invoke via the menu: <b>NAAD &gt; Characters &gt; Create Player Prefab</b>.
    /// </para>
    /// </summary>
    public static class PlayerPrefabCreator
    {
        private const string PrefabFolder = "Assets/NAAD/Characters/Player";
        private const string PrefabName = "Player.prefab";

        [MenuItem("NAAD/Characters/Create Player Prefab", priority = 200)]
        public static void CreatePlayerPrefab()
        {
            if (!EnsureFolderExists())
            {
                return;
            }

            var prefabPath = Path.Combine(PrefabFolder, PrefabName);
            if (File.Exists(prefabPath))
            {
                if (!EditorUtility.DisplayDialog("Player Prefab Exists",
                    $"{prefabPath} already exists. Overwrite?", "Overwrite", "Cancel"))
                {
                    return;
                }
            }

            var playerGo = BuildPlayerGameObject();

            // Add PlayerSpawn as a separate GameObject at the same position
            var spawnGo = new GameObject("PlayerSpawn");
            spawnGo.transform.position = playerGo.transform.position;
            spawnGo.AddComponent<PlayerSpawn>();

            // Create prefab asset (Unity 6 / modern API)
            var prefab = PrefabUtility.SaveAsPrefabAsset(playerGo, prefabPath);
            if (prefab != null)
            {
                Debug.Log($"[PlayerPrefabCreator] Player prefab created: {prefabPath}");
            }
            else
            {
                Debug.LogError("[PlayerPrefabCreator] Failed to create player prefab.");
            }

            // Clean up temporary scene objects
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(spawnGo);

            AssetDatabase.Refresh();
        }

        private static GameObject BuildPlayerGameObject()
        {
            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";

            // --- Core components (Gate D) ---
            // Components auto-discover each other in Awake():
            //   AnimationStateManager -> CharacterAnimationController (via GetComponentsInChildren)
            //   CharacterAnimationController -> Animator (via GetComponent<Animator>)
            var locomotion = playerGo.AddComponent<PlayerLocomotionController>();
            var animator = playerGo.AddComponent<Animator>();
            var characterController = playerGo.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            // --- Animation state machine ---
            var animStateManager = playerGo.AddComponent<AnimationStateManager>();
            var animController = playerGo.AddComponent<CharacterAnimationController>();

            // --- Interaction ---
            var interaction = playerGo.AddComponent<InteractionSystem>();

            // --- Wrapper ---
            var wrapper = playerGo.AddComponent<PlayerFirstPersonController>();
            wrapper.walkSpeed = 2.0f;
            wrapper.runSpeed = 4.5f;
            wrapper.gravity = 20f;
            wrapper.rotationSharpness = 12f;
            wrapper.animationThreshold = 0.08f;

            // --- Locomotion config (edit-time values) ---
            locomotion.walkSpeed = 2.0f;
            locomotion.runSpeed = 4.5f;
            locomotion.gravity = 20f;
            locomotion.rotationSharpness = 12f;

            // --- Camera ---
            var cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(playerGo.transform, worldPositionStays: false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.8f, -4.5f);
            cameraGo.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

            // Animation bridge and state manager wire themselves in Awake()
            // (CharacterAnimationController -> Animator via GetComponent)
            // (AnimationStateManager -> CharacterAnimationController via GetComponentsInChildren)

            return playerGo;
        }

        private static bool EnsureFolderExists()
        {
            if (AssetDatabase.IsValidFolder(PrefabFolder)) return true;

            var parent = Path.GetDirectoryName(PrefabFolder);
            AssetDatabase.CreateFolder(parent, "Player");
            Debug.Log($"[PlayerPrefabCreator] Created folder: {PrefabFolder}");
            return true;
        }
    }
}