using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    public class SetupSceneTransitions
    {
        [MenuItem("NAAD/Setup Scene Transitions/Register All Scenes")]
        public static void RegisterAllScenes()
        {
            RegisterScene("Apartment", "Assets/NAAD/Scenes/Apartment.unity");
            RegisterScene("CityStreet", "Assets/NAAD/Scenes/CityStreet.unity");
            RegisterScene("SuyaSpot", "Assets/NAAD/Scenes/SuyaSpot.unity");
            RegisterScene("Nightclub", "Assets/NAAD/Scenes/Nightclub.unity");
            RegisterScene("Beach", "Assets/NAAD/Scenes/Beach.unity");
            
            Debug.Log("All scenes registered in Build Settings");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions/Add SceneLoaders to All Scenes")]
        public static void AddSceneLoadersToAllScenes()
        {
            AddSceneLoaderToScene("Assets/NAAD/Scenes/Apartment.unity");
            AddSceneLoaderToScene("Assets/NAAD/Scenes/CityStreet.unity");
            AddSceneLoaderToScene("Assets/NAAD/Scenes/SuyaSpot.unity");
            AddSceneLoaderToScene("Assets/NAAD/Scenes/Nightclub.unity");
            AddSceneLoaderToScene("Assets/NAAD/Scenes/Beach.unity");
            
            Debug.Log("Scene loaders added to all scenes");
        }
        
        private static void RegisterScene(string sceneName, string scenePath)
        {
            var scene = EditorBuildSettings.scenes;
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            
            if (sceneAsset == null)
            {
                Debug.LogError($"Scene not found: {scenePath}");
                return;
            }
            
            // Check if already registered
            foreach (var s in scene)
            {
                if (s.path == scenePath)
                {
                    Debug.Log($"Scene already registered: {sceneName}");
                    return;
                }
            }
            
            // Add to build settings
            var newScene = new EditorBuildSettingsScene(scenePath, true);
            var newScenes = new EditorBuildSettingsScene[scene.Length + 1];
            System.Array.Copy(scene, newScenes, scene.Length);
            newScenes[scene.Length] = newScene;
            EditorBuildSettings.scenes = newScenes;
            
            Debug.Log($"Registered scene: {sceneName}");
        }
        
        private static void AddSceneLoaderToScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Create SceneLoader GameObject
            var sceneLoader = new GameObject("SceneLoader");
            var loaderComponent = sceneLoader.AddComponent<SceneLoader>();
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Added SceneLoader to {scenePath}");
        }
    }
    
    // Simple SceneLoader component (add to runtime codebase if not exists)
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }
        
        private System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
