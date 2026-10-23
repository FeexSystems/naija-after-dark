using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using NAAD.Core.Bootstrap;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that bakes NavMesh for every enabled scene in Build Settings.
    /// <para>
    /// Invoke via the menu: <b>NAAD &gt; Scenes &gt; Bake All NavMeshes</b>.
    /// Each scene is opened, the <see cref="NavMeshSurface"/> component is found,
    /// and <see cref="NavMeshSurface.BuildNavMesh()"/> is invoked. The scene is
    /// then saved with the baked NavMesh data.
    /// </para>
    /// </summary>
    public static class NavMeshSceneBaker
    {
        [MenuItem("NAAD/Scenes/Bake All NavMeshes", priority = 103)]
        public static void BakeAllNavMeshes()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("[NavMeshSceneBaker] No scenes in Build Settings. Run NAAD > Scenes > Register Build Settings first.");
                return;
            }

            var baked = 0;
            var failed = new List<string>();

            foreach (var scene in scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                if (!TryBakeScene(scene.path, out var error))
                {
                    failed.Add($"{scene.path}: {error}");
                    continue;
                }

                baked++;
            }

            Debug.Log($"[NavMeshSceneBaker] NavMesh baked for {baked} scene(s)." +
                      (failed.Count > 0 ? $" {failed.Count} failed." : string.Empty));

            foreach (var f in failed)
            {
                Debug.LogError($"[NavMeshSceneBaker] Failed: {f}");
            }
        }

        private static bool TryBakeScene(string scenePath, out string error)
        {
            error = null;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                error = "Scene asset not found.";
                return false;
            }

            Scene opened;
            try
            {
                opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                error = $"Failed to open scene: {ex.Message}";
                return false;
            }

            var navMeshSurfaces = Object.FindObjectsOfType<NavMeshSurface>();
            if (navMeshSurfaces.Length == 0)
            {
                error = "No NavMeshSurface found in scene.";
                return false;
            }

            foreach (var surface in navMeshSurfaces)
            {
                surface.BuildNavMesh();
            }

            EditorSceneManager.MarkSceneDirty(opened);
            try
            {
                EditorSceneManager.SaveScene(opened);
            }
            catch (System.Exception ex)
            {
                error = $"Failed to save scene: {ex.Message}";
                return false;
            }

            return true;
        }
    }
}