using UnityEditor;
using UnityEngine;
using System.IO;

namespace NAAD.Editor
{
    public static class VerifyUI
    {
        public static void Execute()
        {
            Debug.Log("Verifying UI components...");

            bool allChecksPassed = true;

            // Check UI controllers
            string[] uiControllers = new string[]
            {
                "Assets/NAAD/UI/DialogueBubble.cs",
                "Assets/NAAD/UI/ConversationUI.cs",
                "Assets/NAAD/UI/DialogueUIController.cs",
                "Assets/NAAD/UI/MainHudController.cs",
                "Assets/NAAD/UI/BootstrapScreenController.cs"
            };

            foreach (string controller in uiControllers)
            {
                string fullPath = Path.Combine(Application.dataPath, controller.Substring("Assets/".Length));
                if (!File.Exists(fullPath))
                {
                    Debug.LogError($"UI controller missing: {controller}");
                    allChecksPassed = false;
                }
                else
                {
                    Debug.Log($"✓ UI controller exists: {controller}");
                }
            }

            // Check UI prefabs
            string[] uiPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/NAAD/UI" });
            Debug.Log($"Found {uiPrefabs.Length} UI prefabs");

            // Check data contracts
            string dataContractsPath = "Assets/NAAD/UI/DataContracts.cs";
            string fullContractsPath = Path.Combine(Application.dataPath, dataContractsPath.Substring("Assets/".Length));
            if (!File.Exists(fullContractsPath))
            {
                Debug.LogError($"Data contracts missing: {dataContractsPath}");
                allChecksPassed = false;
            }
            else
            {
                Debug.Log($"✓ Data contracts exist: {dataContractsPath}");
            }

            // Generate report
            string reportPath = "Logs/ui-verification-report.json";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            string report = $@"{{
  ""timestamp"": ""{System.DateTime.UtcNow:O}"",
  ""uiControllers"": {uiControllers.Length},
  ""uiPrefabs"": {uiPrefabs.Length},
  ""allChecksPassed"": {allChecksPassed.ToString().ToLower()}
}}";

            File.WriteAllText(reportPath, report);
            Debug.Log($"UI verification report written to: {reportPath}");

            if (allChecksPassed)
            {
                Debug.Log("✓ UI verification PASSED");
            }
            else
            {
                Debug.LogError("✗ UI verification FAILED");
            }

            EditorApplication.Exit(allChecksPassed ? 0 : 1);
        }
    }
}
