using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static class NAADBuild
    {
        public static void Verify()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
                throw new BuildFailedException("NAAD Gate 21: no build scenes configured.");
            if (!scenes[0].enabled || scenes[0].path != "Assets/Scenes/Bootstrap.unity")
                throw new BuildFailedException("NAAD Gate 21: Bootstrap scene must be enabled at build index 0.");
            if (AssetDatabase.LoadAssetAtPath<Object>("Assets/Scenes/Bootstrap.unity") == null)
                throw new BuildFailedException("NAAD Gate 21: Bootstrap.unity could not be loaded.");
            Debug.Log($"[NAAD][Gate21] Unity verification passed. Scenes={scenes.Length}, Bootstrap={scenes[0].path}");
        }

        public static void BuildAndroid()
        {
            Verify();
            var output = "Builds/Android/NAAD.apk";
            var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, output, BuildTarget.Android, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new BuildFailedException($"NAAD Android build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
            Debug.Log($"[NAAD][Gate21] Android build succeeded: {output}");
        }
    }
}