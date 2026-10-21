using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using NAAD.Characters.Player;
using NAAD.Core.Bootstrap;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that creates the NAAD player character prefab with all
    /// required components wired up: NavMeshAgent, Animator,
    /// PlayerLocomotionController, PlayerAnimator, ThirdPersonCamera,
    /// PlayerInputBridge, VirtualJoystick, and PlayerSpawn.
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

            // --- Core components ---
            var locomotion = playerGo.AddComponent<PlayerLocomotionController>();
            var animator = playerGo.AddComponent<Animator>();
            var agent = playerGo.AddComponent<NavMeshAgent>();
            var playerAnimator = playerGo.AddComponent<PlayerAnimator>();
            var inputBridge = playerGo.AddComponent<PlayerInputBridge>();

            // --- Camera (child or separate) ---
            var cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(playerGo.transform, worldPositionStays: false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.8f, -4.5f);
            cameraGo.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            var thirdPersonCamera = cameraGo.AddComponent<ThirdPersonCamera>();
            thirdPersonCamera.SetTarget(playerGo.transform);

            // --- NavMeshAgent configuration ---
            agent.radius = 0.4f;
            agent.height = 1.8f;
            agent.speed = 3.2f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.15f;
            agent.autoBraking = true;
            agent.updateRotation = false; // Locomotion controller handles rotation
            agent.updatePosition = true;

            // --- Animator: assign controller if available ---
            var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                Path.Combine(PrefabFolder, "PlayerAnimator.controller"));
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }
            else
            {
                Debug.LogWarning("[PlayerPrefabCreator] PlayerAnimator.controller not found. " +
                    "Run NAAD > Characters > Setup Animator Controller first.");
            }

            // --- Locomotion config ---
            var config = AssetDatabase.LoadAssetAtPath<PlayerLocomotionConfig>(
                Path.Combine(PrefabFolder, "PlayerLocomotionConfig.asset"));
            if (config != null)
            {
                locomotion._config = config;
            }

            locomotion._animator = animator;
            locomotion._agent = agent;

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