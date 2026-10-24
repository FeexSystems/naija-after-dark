using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using System.Threading;

namespace NAAD.Editor
{
    public static class PackageMethods
    {
        public static void ListPackages()
        {
            Debug.Log("Listing installed packages...");

            var listRequest = Client.List();

            while (!listRequest.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (listRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Found {listRequest.Result.Count} packages:");
                foreach (var package in listRequest.Result)
                {
                    Debug.Log($"  {package.name} - {package.version} ({package.source})");
                }
            }
            else
            {
                Debug.LogError($"Failed to list packages: {listRequest.Error.message}");
            }

            EditorApplication.Exit(listRequest.Status == StatusCode.Success ? 0 : 1);
        }

        public static string packageName = "";

        public static void AddPackage()
        {
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Package name not specified. Use -executeMethod with packageName parameter or set it in the script.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Adding package: {packageName}");

            var addRequest = Client.Add(packageName);

            while (!addRequest.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Package added: {addRequest.Result.name} v{addRequest.Result.version}");
            }
            else
            {
                Debug.LogError($"Failed to add package: {addRequest.Error.message}");
            }

            EditorApplication.Exit(addRequest.Status == StatusCode.Success ? 0 : 1);
        }

        public static void ReimportTextures()
        {
            Debug.Log("Reimporting textures...");

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");

            Debug.Log($"Found {textureGuids.Length} textures to reimport");

            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            Debug.Log($"Reimported {textureGuids.Length} textures");
            EditorApplication.Exit(0);
        }
    }
}
