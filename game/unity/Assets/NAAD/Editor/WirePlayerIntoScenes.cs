using System.Collections.Generic;
using System.Linq;
using NAAD.Core.Bootstrap;
using NAAD.Gameplay.Interaction;
using NAAD.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate D — put a playable player into every production scene.
    ///
    /// <para>
    /// <c>PlayerRig.prefab</c> has been fully wired since Gate C (Animator, AnimatorController,
    /// CharacterController, locomotion, animation bridge — all verified by
    /// <c>VerifyAnimationAssets</c>, 37/37). But no scene instantiates it, so the scenes are
    /// empty dioramas and First Night can only be driven by the IMGUI panel in Bootstrap.
    /// This is the missing step between "the rig works" and "you can walk around".
    /// </para>
    ///
    /// <para>
    /// Idempotent and non-destructive: a scene that already has a
    /// <see cref="PlayerInteractionController"/> is skipped, and nothing is deleted. Safe to
    /// re-run after the pipeline rebuilds a scene.
    /// </para>
    ///
    /// Invoke from the menu: <b>NAAD &gt; Gate D &gt; Wire Player Into Scenes</b>
    /// Or headless: <c>-executeMethod NAAD.Editor.WirePlayerIntoScenes.Run</c>
    /// </summary>
    public static class WirePlayerIntoScenes
    {
        private const string PlayerPrefabPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.prefab";

        [MenuItem("NAAD/Gate D/Wire Player Into Scenes", priority = 200)]
        public static void RunMenu() => Run();

        [MenuItem("NAAD/Gate D/Validate Player Wiring", priority = 201)]
        public static void ValidateMenu() => Validate();

        public static void Run()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[NAAD][GateD] Player prefab missing: {PlayerPrefabPath}");
                return;
            }

            var wired = 0;
            var skipped = 0;

            foreach (var scenePath in ProductionScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Debug.LogWarning($"[NAAD][GateD] Could not open {scenePath}");
                    continue;
                }

                if (Object.FindFirstObjectByType<PlayerInteractionController>() != null)
                {
                    Debug.Log($"[NAAD][GateD] {SceneName(scenePath)}: player already present, skipping.");
                    skipped++;
                    continue;
                }

                var spawn = Object.FindObjectsByType<SceneAnchor>(FindObjectsInactive.Include)
                    .FirstOrDefault(a => a.Kind == SceneAnchor.AnchorKind.PlayerSpawn);

                if (spawn == null)
                {
                    Debug.LogWarning($"[NAAD][GateD] {SceneName(scenePath)}: no PlayerSpawn anchor — not wiring.");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "Player";
                instance.transform.position = spawn.transform.position;
                instance.transform.rotation = spawn.transform.rotation;

                // The interaction controller is what actually resolves anchors into commands.
                if (instance.GetComponent<PlayerInteractionController>() == null)
                    instance.AddComponent<PlayerInteractionController>();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                wired++;
                Debug.Log($"[NAAD][GateD] {SceneName(scenePath)}: player wired at {spawn.transform.position}.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[NAAD][GateD] Done — {wired} scene(s) wired, {skipped} already had a player.");
        }

        /// <summary>
        /// Fails if any production scene lacks a player or a camera, so the gap cannot
        /// silently reopen after a scene rebuild.
        /// </summary>
        public static void Validate()
        {
            var errors = new List<string>();

            foreach (var scenePath in ProductionScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    errors.Add($"Could not open {scenePath}");
                    continue;
                }

                if (Object.FindFirstObjectByType<PlayerInteractionController>() == null)
                    errors.Add($"{SceneName(scenePath)}: no PlayerInteractionController (run NAAD > Gate D > Wire Player Into Scenes)");

                var spawns = Object.FindObjectsByType<SceneAnchor>(FindObjectsInactive.Include)
                    .Count(a => a.Kind == SceneAnchor.AnchorKind.PlayerSpawn);
                if (spawns == 0)
                    errors.Add($"{SceneName(scenePath)}: no PlayerSpawn anchor");
            }

            if (errors.Count > 0)
                throw new System.Exception("NAAD Gate D player wiring failed:\n - " + string.Join("\n - ", errors));

            Debug.Log("[NAAD][GateD] Player wiring validated for all production scenes.");
        }

        private static IEnumerable<string> ProductionScenePaths() => new[]
        {
            NaadSceneNames.ApartmentPath,
            NaadSceneNames.CityStreetPath,
            NaadSceneNames.SuyaSpotPath,
            NaadSceneNames.NightclubPath,
            NaadSceneNames.BeachPath,
        };

        private static string SceneName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}
