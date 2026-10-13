using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NAAD.Core.Bootstrap;
using NAAD.Gameplay.FirstNight;

namespace NAAD.Editor
{
    public static class NAADBuild
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        public static void Verify()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
                throw new Exception("NAAD Gate 22: no build scenes configured.");

            if (!scenes[0].enabled || scenes[0].path != BootstrapScenePath)
                throw new Exception("NAAD Gate 22: Bootstrap scene must be enabled at build index 0.");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath) == null)
                throw new Exception("NAAD Gate 22: Bootstrap scene asset could not be loaded.");

            // Open the actual scene so validation checks serialized scene wiring,
            // not merely the existence of a .unity file in the project.
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new Exception("NAAD Gate 22: Bootstrap scene failed to open.");

            var roots = scene.GetRootGameObjects();
            ValidateNoMissingScripts(roots);

            var appRoot = FindComponent<NAADApplicationRoot>(roots);
            if (appRoot == null)
                throw new Exception("NAAD Gate 22: NAADApplicationRoot is missing from Bootstrap.");

            if (appRoot.Config == null)
                throw new Exception("NAAD Gate 22: SupabaseConfig asset is not assigned to NAADApplicationRoot.");

            if (!appRoot.Config.IsConfigured)
                throw new Exception("NAAD Gate 22: SupabaseConfig requires a project URL and anon key.");

            var bootstrap = FindComponent<NAADBootstrap>(roots);
            if (bootstrap == null)
                throw new Exception("NAAD Gate 22: NAADBootstrap is missing from Bootstrap.");

            var controller = FindComponent<FirstNightController>(roots);
            if (controller == null)
                throw new Exception("NAAD Gate 22: FirstNightController is missing from Bootstrap.");

            var hud = FindComponent<FirstNightHud>(roots);
            if (hud == null)
                throw new Exception("NAAD Gate 22: FirstNightHud is missing from Bootstrap.");

            var hudSerialized = new SerializedObject(hud);
            var controllerProperty = hudSerialized.FindProperty("controller");
            if (controllerProperty == null || controllerProperty.objectReferenceValue != controller)
                throw new Exception("NAAD Gate 22: FirstNightHud.controller must reference the scene's FirstNightController.");

            var mainCamera = FindComponent<Camera>(roots);
            if (mainCamera == null || !mainCamera.CompareTag("MainCamera"))
                throw new Exception("NAAD Gate 22: a Camera tagged MainCamera is required.");

            Debug.Log(
                $"[NAAD][Gate22] Verification passed. Scene={scene.path}; " +
                $"Root={appRoot.name}; Bootstrap={bootstrap.name}; " +
                $"FirstNight={controller.name}; HUD={hud.name}; MainCamera={mainCamera.name}; " +
                $"BuildScenes={scenes.Length}.");
        }

        public static void BuildAndroid()
        {
            Verify();
            const string output = "Builds/Android/NAAD.apk";
            var report = BuildPipeline.BuildPlayer(
                EditorBuildSettings.scenes,
                output,
                BuildTarget.Android,
                BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception(
                    $"NAAD Android build failed: {report.summary.result} " +
                    $"({report.summary.totalErrors} errors)");

            Debug.Log($"[NAAD][Gate22] Android build succeeded: {output}");
        }

        private static T FindComponent<T>(GameObject[] roots) where T : Component
        {
            foreach (var root in roots)
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }

        private static void ValidateNoMissingScripts(GameObject[] roots)
        {
            foreach (var root in roots)
            {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    var components = transform.gameObject.GetComponents<Component>();
                    foreach (var component in components)
                    {
                        if (component == null)
                            throw new Exception(
                                $"NAAD Gate 22: missing script reference on GameObject " +
                                $"'{transform.gameObject.name}' in Bootstrap.");
                    }
                }
            }
        }
    }
}
