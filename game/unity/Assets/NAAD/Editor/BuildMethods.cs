using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace NAAD.Editor
{
    public static class BuildMethods
    {
        public static void BuildAndroid()
        {
            Debug.Log("Building Android APK...");

            string[] scenes = { "Assets/NAAD/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD.apk";

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {buildPath}");
                Debug.Log($"Build size: {report.summary.totalSize} bytes");
                Debug.Log($"Build time: {report.summary.totalTime.TotalMinutes:F2} minutes");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.totalErrors} errors");
            }

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildAndroidDevelopment()
        {
            Debug.Log("Building Android Development APK...");

            string[] scenes = { "Assets/NAAD/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD-Dev.apk";

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Development build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"Development build failed: {report.summary.totalErrors} errors");
            }

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildAndroidAAB()
        {
            Debug.Log("Building Android AAB...");

            string[] scenes = { "Assets/NAAD/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD.aab";

            EditorUserBuildSettings.buildAppBundle = true;

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"AAB build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"AAB build failed: {report.summary.totalErrors} errors");
            }

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static void BuildAndroidWithProfiling()
        {
            Debug.Log("Building Android APK with profiling...");

            string[] scenes = { "Assets/NAAD/Scenes/Bootstrap.unity" };
            string buildPath = "Builds/Android/NAAD-Profile.apk";

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Profiling build succeeded: {buildPath}");
            }
            else
            {
                Debug.LogError($"Profiling build failed: {report.summary.totalErrors} errors");
            }

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
