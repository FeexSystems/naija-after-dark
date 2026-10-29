#if NAAD_LEGACY_EDITOR_TOOLS
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace NAAD.Editor
{
    public static partial class Editor
    {
        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static string _packageName = string.Empty;

        /// <summary>
        /// `-executeMethod NAAD.Editor.Editor.ListPackages`
        ///
        /// The package manager client is asynchronous. The documented sample used
        /// Thread.Sleep polling on the main thread, which can deadlock the editor's synchronisation
        /// context; this subscribes to the completion event and pumps the editor instead.
        /// </summary>
        public static void ListPackages()
        {
            Debug.Log("[NAAD][Verify] Listing installed packages...");
            _listRequest = Client.List(offlineMode: false, includeIndirectDependencies: true);
            EditorApplication.update += PollListPackages;
        }

        private static void PollListPackages()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            EditorApplication.update -= PollListPackages;

            if (_listRequest.Status == StatusCode.Success)
            {
                foreach (var package in _listRequest.Result)
                    Debug.Log($"[NAAD][Packages] {package.name} - {package.version}");
                Debug.Log($"[NAAD][Packages] {_listRequest.Result.Count()} package(s) installed.");
                _listRequest = null;
                ExitBatch(0);
                return;
            }

            Debug.LogError($"[NAAD][Packages] Failed to list packages: {_listRequest.Error?.message}");
            _listRequest = null;
            ExitBatch(1);
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.Editor.AddPackage`
        ///
        /// Unity passes custom `-package &lt;value&gt;` arguments into Environment.GetCommandLineArgs();
        /// the `public static string packageName` field in the documented sample is never populated
        /// by the CLI, so the argument is parsed from the command line instead. Falls back to the
        /// named field when invoked from a menu or another script.
        /// </summary>
        public static void AddPackage()
        {
            _packageName = ResolvePackageName();
            if (string.IsNullOrEmpty(_packageName))
            {
                Debug.LogError("[NAAD][Packages] No package specified. Pass `-package <name-or-git-url>`.");
                ExitBatch(1);
                return;
            }

            Debug.Log($"[NAAD][Packages] Adding package: {_packageName}");
            _addRequest = Client.Add(_packageName);
            EditorApplication.update += PollAddPackage;
        }

        private static void PollAddPackage()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
                return;

            EditorApplication.update -= PollAddPackage;

            if (_addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"[NAAD][Packages] Package added: {_addRequest.Result.name} {_addRequest.Result.version}");
                _addRequest = null;
                ExitBatch(0);
                return;
            }

            Debug.LogError($"[NAAD][Packages] Failed to add package '{_packageName}': {_addRequest.Error?.message}");
            _addRequest = null;
            ExitBatch(1);
        }

        private static string ResolvePackageName()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("-package", StringComparison.OrdinalIgnoreCase) ||
                    args[i].Equals("-packageName", StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return _packageName;
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.Editor.ReimportTextures`
        ///
        /// Forces a reimport of every Texture2D so platform compression settings are reapplied.
        /// </summary>
        public static void ReimportTextures()
        {
            Debug.Log("[NAAD][Assets] Reimporting textures...");

            var guids = AssetDatabase.FindAssets("t:Texture2D");
            var count = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[NAAD][Assets] Reimported {count} texture(s).");
            ExitBatch(0);
        }
    }
}
#endif
