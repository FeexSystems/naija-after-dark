using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Menu wrapper for Android build.
    /// Run from Unity Editor menu: NAAD > Build > Build Android APK
    /// </summary>
    public static class BuildAndroidMenu
    {
        [MenuItem("NAAD/Build/Build Android APK")]
        public static void BuildAndroidAPK()
        {
            Debug.Log("[NAAD][Build] Starting Android APK build via menu...");
            Editor.BuildAndroid();
        }

        [MenuItem("NAAD/Build/Build Android AAB")]
        public static void BuildAndroidAAB()
        {
            Debug.Log("[NAAD][Build] Starting Android AAB build via menu...");
            Editor.BuildAndroidAAB();
        }

        [MenuItem("NAAD/Build/Build Android Development APK")]
        public static void BuildAndroidDevAPK()
        {
            Debug.Log("[NAAD][Build] Starting Android Development APK build via menu...");
            Editor.BuildAndroidDevelopment();
        }
    }
}