using System.IO;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static partial class Editor
    {
        /// <summary>
        /// `-executeMethod NAAD.Editor.VerifyProject`
        ///
        /// Verifies the project skeleton the runtime actually depends on: the bootstrap scene is in
        /// Build Settings at index 0, the core service/bootstrap scripts exist, and the
        /// SupabaseConfig asset is present. Reports to Logs/ProjectVerification.log via -logFile and
        /// to Logs/verification-report.json.
        /// </summary>
        public static void VerifyProject()
        {
            Debug.Log("[NAAD][Verify] Verifying project structure...");
            var report = new VerificationReport();

            VerifyBuildScenes(report);
            VerifyCoreScripts(report);
            VerifyConfiguration(report);

            Finish("VerifyProject", report);
        }

        private static void VerifyBuildScenes(VerificationReport report)
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Fail(report, "No scenes configured in Build Settings.");
                return;
            }

            Pass(report, $"{scenes.Length} scene(s) configured in Build Settings.");

            const string bootstrapPath = "Assets/Scenes/Bootstrap.unity";
            if (scenes[0].path.Equals(bootstrapPath, System.StringComparison.OrdinalIgnoreCase)
                && scenes[0].enabled)
            {
                Pass(report, $"Bootstrap scene enabled at build index 0 ({bootstrapPath}).");
            }
            else
            {
                Fail(report, $"Bootstrap scene must be enabled at build index 0; found '{scenes[0].path}' (enabled={scenes[0].enabled}).");
            }

            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!scene.enabled) continue;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                    Fail(report, $"Build scene at index {i} could not be loaded: {scene.path}");
            }
        }

        private static void VerifyCoreScripts(VerificationReport report)
        {
            string[] requiredScripts =
            {
                "Assets/NAAD/Core/Bootstrap/NAADApplicationRoot.cs",
                "Assets/NAAD/Core/Bootstrap/NAADBootstrap.cs",
                "Assets/NAAD/Core/Bootstrap/SceneLoader.cs",
                "Assets/NAAD/Core/Logging/INAADLogger.cs",
                "Assets/NAAD/Core/State/GameStateManager.cs",
                "Assets/NAAD/Networking/Interfaces/IAuthService.cs",
                "Assets/NAAD/Networking/Interfaces/ICommandService.cs",
                "Assets/NAAD/Networking/Interfaces/IPhoneService.cs",
                "Assets/NAAD/Networking/Interfaces/IPlayerService.cs",
                "Assets/NAAD/Networking/Interfaces/IWorldStateService.cs",
                "Assets/NAAD/UI/BaseScreenController.cs",
                "Assets/NAAD/Gameplay/FirstNight/FirstNightController.cs"
            };

            foreach (var path in requiredScripts)
            {
                if (File.Exists(path))
                    Pass(report, $"Found core script: {path}");
                else
                    Fail(report, $"Missing core script: {path}");
            }

            string[] requiredFolders =
            {
                "Assets/NAAD/Core",
                "Assets/NAAD/Networking",
                "Assets/NAAD/UI",
                "Assets/NAAD/Gameplay",
                "Assets/NAAD/Content",
                "Assets/NAAD/Editor",
                "Assets/Scenes"
            };

            foreach (var folder in requiredFolders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                    Pass(report, $"Found folder: {folder}");
                else
                    Fail(report, $"Missing folder: {folder}");
            }
        }

        private static void VerifyConfiguration(VerificationReport report)
        {
            const string configPath = "Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<NAAD.Networking.Config.SupabaseConfig>(configPath);
            if (config == null)
            {
                Fail(report, $"SupabaseConfig asset missing at {configPath}.");
                return;
            }

            Pass(report, "SupabaseConfig asset present.");

            if (config.IsConfigured)
                Pass(report, "SupabaseConfig is configured (live services will be composed).");
            else
                Debug.LogWarning("[NAAD][Verify] SupabaseConfig is NOT configured — the app will fall back to stub services. This is expected before credentials are provisioned.");
        }
    }
}
