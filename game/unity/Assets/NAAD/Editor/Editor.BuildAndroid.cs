using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NAAD.Editor
{
    public static partial class Editor
    {
        private const string ApkOutput = "Builds/Android/NAAD.apk";
        private const string DevelopmentApkOutput = "Builds/Android/NAAD-Dev.apk";
        private const string AabOutput = "Builds/Android/NAAD.aab";

        /// <summary>
        /// `-executeMethod NAAD.Editor.BuildAndroid`
        ///
        /// Release APK. Scenes come from Build Settings (Bootstrap is index 0, enforced by
        /// NAADBuild.Verify) rather than being hard-coded, so adding a scene cannot silently
        /// produce an inconsistent build. Exits non-zero on failure so CI fails the job.
        /// </summary>
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, ApkOutput, BuildOptions.CompressWithLz4HC, "Android APK");
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.BuildAndroidDevelopment`
        ///
        /// Development APK with scripting debugger and profiler attached.
        /// </summary>
        public static void BuildAndroidDevelopment()
        {
            Build(BuildTarget.Android, DevelopmentApkOutput,
                BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
                "Android development APK");
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.BuildAndroidAAB`
        ///
        /// Play Store app bundle. buildAppBundle is a global editor setting, so it is saved and
        /// restored around the build to keep a later APK build from being emitted as an AAB.
        /// </summary>
        public static void BuildAndroidAAB()
        {
            var previous = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle = true;
                Build(BuildTarget.Android, AabOutput, BuildOptions.CompressWithLz4HC, "Android AAB");
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previous;
            }
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.BuildAndroidWithProfiling`
        ///
        /// Profiled release build: the "profiling" axis is DeepProfilingSupport plus the
        /// connection options, not BuildOptions.Development on its own.
        /// </summary>
        public static void BuildAndroidWithProfiling()
        {
            var previousDeepProfiling = EditorUserBuildSettings.activeScriptCompilationDefines;
            var previousProfiling = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android);
            _ = previousDeepProfiling;
            _ = previousProfiling;

            var previousProfilerSetting = EditorUserBuildSettings.development;
            try
            {
                EditorUserBuildSettings.development = true;
                EditorUserBuildSettings.connectProfiler = true;
                Build(BuildTarget.Android, "Builds/Android/NAAD-Profiled.apk",
                    BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport,
                    "Android profiled APK");
            }
            finally
            {
                EditorUserBuildSettings.development = previousProfilerSetting;
            }
        }

        private static void Build(BuildTarget target, string output, BuildOptions options, string label)
        {
            Debug.Log($"[NAAD][Build] Building {label} -> {output}");

            NAADBuild.Verify();

            var directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var buildOptions = new BuildPlayerOptions
            {
                scenes = EnabledScenePaths(),
                locationPathName = output,
                target = target,
                options = options
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[NAAD][Build] {label} FAILED: {summary.result}, " +
                               $"{summary.totalErrors} error(s), {summary.totalWarnings} warning(s).");
                ExitBatch(1);
                return;
            }

            Debug.Log($"[NAAD][Build] {label} succeeded in {summary.totalTime.TotalSeconds:F1}s " +
                      $"({summary.totalSize / 1024 / 1024} MB): {output}");
            ExitBatch(0);
        }

        private static string[] EnabledScenePaths()
        {
            var scenes = EditorBuildSettings.scenes;
            var paths = new System.Collections.Generic.List<string>(scenes.Length);
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                    paths.Add(scene.path);
            }

            return paths.ToArray();
        }
    }
}
