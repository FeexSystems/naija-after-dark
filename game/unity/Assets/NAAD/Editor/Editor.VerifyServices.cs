using UnityEditor;
using UnityEngine;
using System.IO;
using NAAD.Networking;

namespace NAAD.Editor
{
    public static class VerifyServices
    {
        public static void Execute()
        {
            Debug.Log("Verifying service configuration...");

            bool allChecksPassed = true;

            // Check SupabaseConfig
            string supabaseConfigPath = "Assets/NAAD/Networking/Config/SupabaseConfig.cs";
            string fullPath = Path.Combine(Application.dataPath, supabaseConfigPath.Substring("Assets/".Length));

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"SupabaseConfig missing: {supabaseConfigPath}");
                allChecksPassed = false;
            }
            else
            {
                Debug.Log($"✓ SupabaseConfig exists: {supabaseConfigPath}");

                // Check if SupabaseConfig has required fields (basic validation)
                string configContent = File.ReadAllText(fullPath);
                if (!configContent.Contains("SupabaseUrl") || !configContent.Contains("SupabaseKey"))
                {
                    Debug.LogWarning("SupabaseConfig may be missing required fields");
                }
            }

            // Check service interfaces
            string[] serviceInterfaces = new string[]
            {
                "Assets/NAAD/Networking/Interfaces/IDialogueService.cs",
                "Assets/NAAD/Networking/Supabase/SupabaseDialogueService.cs"
            };

            foreach (string service in serviceInterfaces)
            {
                string servicePath = Path.Combine(Application.dataPath, service.Substring("Assets/".Length));
                if (!File.Exists(servicePath))
                {
                    Debug.LogError($"Service missing: {service}");
                    allChecksPassed = false;
                }
                else
                {
                    Debug.Log($"✓ Service exists: {service}");
                }
            }

            // Note: Actual connectivity testing is read-only and requires credentials
            // This is a structural verification only
            Debug.Log("Note: Service connectivity testing requires valid credentials and is not performed in batch mode");

            // Generate report
            string reportPath = "Logs/service-verification-report.json";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            string report = $@"{{
  ""timestamp"": ""{System.DateTime.UtcNow:O}"",
  ""supabaseConfig"": {(File.Exists(fullPath) ? "present" : "missing")},
  ""serviceInterfaces"": {serviceInterfaces.Length},
  ""allChecksPassed"": {allChecksPassed.ToString().ToLower()}
}}";

            File.WriteAllText(reportPath, report);
            Debug.Log($"Service verification report written to: {reportPath}");

            if (allChecksPassed)
            {
                Debug.Log("✓ Service verification PASSED");
            }
            else
            {
                Debug.LogError("✗ Service verification FAILED");
            }

            EditorApplication.Exit(allChecksPassed ? 0 : 1);
        }
    }
}
