#if NAAD_LEGACY_EDITOR_TOOLS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using NAAD.Core.Bootstrap;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that creates the five NAAD MVP environment scenes with the
    /// required runtime setup (PlayerSpawn, NavMeshSurface, lighting, MainCamera).
    /// <para>
    /// Invoke via the menu: <b>NAAD &gt; Scenes &gt; Create All MVP Scenes</b>.
    /// Scenes are saved to <c>Assets/Scenes/</c> and registered in Build Settings
    /// by <see cref="BuildSettingsRegister"/>.
    /// </para>
    /// </summary>
    public static class SceneCreator
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("NAAD/Scenes/Create All MVP Scenes", priority = 100)]
        public static void CreateAllScenes()
        {
            if (!EnsureScenesFolderExists())
            {
                return;
            }

            foreach (var (name, path) in NaadSceneNames.AllScenes)
            {
                if (File.Exists(path))
                {
                    Debug.Log($"[SceneCreator] Scene already exists, skipping: {path}");
                    continue;
                }

                CreateScene(name, path);
            }

            AssetDatabase.Refresh();
            Debug.Log("[SceneCreator] All MVP scenes created. Open Build Settings to assign indices, or run NAAD > Scenes > Register Build Settings.");
        }

        private static bool EnsureScenesFolderExists()
        {
            if (AssetDatabase.IsValidFolder(ScenesFolder))
            {
                return true;
            }

            var parent = Path.GetDirectoryName(ScenesFolder);
            AssetDatabase.CreateFolder(parent, "Scenes");
            Debug.Log($"[SceneCreator] Created folder: {ScenesFolder}");
            return true;
        }

        private static void CreateScene(string sceneName, string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- MainCamera ---
            var cameraGo = new GameObject("MainCamera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cameraGo.transform.position = new Vector3(0f, 1.5f, -5f);
            cameraGo.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

            // --- Directional Light ---
            var lightGo = new GameObject("Directional Light");
            lightGo.transform.position = new Vector3(0f, 10f, 0f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;
            light.shadows = LightShadows.Soft;

            // --- PlayerSpawn ---
            var spawnGo = new GameObject("PlayerSpawn");
            spawnGo.AddComponent<PlayerSpawn>();
            spawnGo.transform.position = Vector3.zero;

            // --- NavMeshSurface ---
            var navMeshGo = new GameObject("NavMeshSurface");
            var navMeshSurface = navMeshGo.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.buildHeightMesh = true;

            // --- Simple floor geometry for NavMesh baking ---
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = Vector3.one * 20f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneCreator] Created scene: {scenePath}");
        }
    }
}
#endif
