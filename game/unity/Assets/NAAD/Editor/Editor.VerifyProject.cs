using UnityEditor;
using UnityEngine;
using System.IO;

namespace NAAD.Editor
{
    public static class VerifyProject
    {
        public static void Execute()
        {
            Debug.Log("Verifying NAAD project structure...");

            bool allChecksPassed = true;

            // Check required folders
            string[] requiredFolders = new string[]
            {
                "Assets/NAAD",
                "Assets/NAAD/AI",
                "Assets/NAAD/Audio",
                "Assets/NAAD/Content",
                "Assets/NAAD/Environment",
                "Assets/NAAD/Gameplay",
                "Assets/NAAD/Networking",
                "Assets/NAAD/NPCs",
                "Assets/NAAD/UI"
            };

            foreach (string folder in requiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Debug.LogError($"Required folder missing: {folder}");
                    allChecksPassed = false;
                }
                else
                {
                    Debug.Log($"✓ Folder exists: {folder}");
                }
            }

            // Check required scripts
            string[] requiredScripts = new string[]
            {
                "Assets/NAAD/Gameplay/DialogueManager.cs",
                "Assets/NAAD/UI/DialogueBubble.cs",
                "Assets/NAAD/NPCs/TundeNPC.cs",
                "Assets/NAAD/NPCs/BehaviorStateMachine.cs",
                "Assets/NAAD/Networking/SupabaseConfig.cs"
            };

            foreach (string script in requiredScripts)
            {
                if (!File.Exists(Path.Combine(Application.dataPath, script.Substring("Assets/".Length))))
                {
                    Debug.LogError($"Required script missing: {script}");
                    allChecksPassed = false;
                }
                else
                {
                    Debug.Log($"✓ Script exists: {script}");
                }
            }

            // Check configuration files
            string[] requiredConfigs = new string[]
            {
                "Assets/NAAD/UNITY_CLI_COMMANDS.md",
                "game/unity/mcp-manifest.json"
            };

            foreach (string config in requiredConfigs)
            {
                string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", config));
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"Configuration file not found: {config}");
                }
                else
                {
                    Debug.Log($"✓ Config exists: {config}");
                }
            }

            // Generate report
            string reportPath = "Logs/verification-report.json";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            string report = $@"{{
  ""timestamp"": ""{System.DateTime.UtcNow:O}"",
  ""project"": ""NAIJA AFTER DARK"",
  ""unityVersion"": ""{Application.unityVersion}"",
  ""allChecksPassed"": {allChecksPassed.ToString().ToLower()},
  ""checks"": {{
    ""requiredFolders"": {requiredFolders.Length},
    ""requiredScripts"": {requiredScripts.Length}
  }}
}}";

            File.WriteAllText(reportPath, report);
            Debug.Log($"Verification report written to: {reportPath}");

            if (allChecksPassed)
            {
                Debug.Log("✓ Project verification PASSED");
            }
            else
            {
                Debug.LogError("✗ Project verification FAILED");
            }

            EditorApplication.Exit(allChecksPassed ? 0 : 1);
        }
    }
}
