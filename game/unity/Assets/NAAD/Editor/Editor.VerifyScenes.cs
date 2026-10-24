using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;

namespace NAAD.Editor
{
    public static class VerifyScenes
    {
        public static void Execute()
        {
            Debug.Log("Verifying scenes...");

            bool allChecksPassed = true;

            // Get all scenes in Assets
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            Debug.Log($"Found {sceneGuids.Length} scenes in project");

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Checking scene: {scenePath}");

                // Load scene
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Check if scene has required components
                GameObject[] rootObjects = scene.GetRootGameObjects();
                bool hasMainCamera = false;
                bool hasLight = false;

                foreach (GameObject obj in rootObjects)
                {
                    if (obj.GetComponent<Camera>() != null)
                    {
                        hasMainCamera = true;
                    }
                    if (obj.GetComponent<Light>() != null)
                    {
                        hasLight = true;
                    }
                }

                if (!hasMainCamera)
                {
                    Debug.LogWarning($"Scene {scenePath} has no Camera");
                }
                if (!hasLight)
                {
                    Debug.LogWarning($"Scene {scenePath} has no Light");
                }

                Debug.Log($"✓ Scene {scenePath} verified");
            }

            // Check build settings
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            Debug.Log($"Build settings contains {buildScenes.Length} scenes");

            if (buildScenes.Length == 0)
            {
                Debug.LogError("No scenes in build settings");
                allChecksPassed = false;
            }

            // Generate report
            string reportPath = "Logs/scene-verification-report.json";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            string report = $@"{{
  ""timestamp"": ""{System.DateTime.UtcNow:O}"",
  ""totalScenes"": {sceneGuids.Length},
  ""buildScenes"": {buildScenes.Length},
  ""allChecksPassed"": {allChecksPassed.ToString().ToLower()}
}}";

            File.WriteAllText(reportPath, report);
            Debug.Log($"Scene verification report written to: {reportPath}");

            if (allChecksPassed)
            {
                Debug.Log("✓ Scene verification PASSED");
            }
            else
            {
                Debug.LogError("✗ Scene verification FAILED");
            }

            EditorApplication.Exit(allChecksPassed ? 0 : 1);
        }
    }
}
