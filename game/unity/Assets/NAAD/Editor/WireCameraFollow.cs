using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Characters.Player;

namespace NAAD.Editor
{
    public class WireCameraFollow
    {
        [MenuItem("NAAD/Wire Camera Follow/All Scenes")]
        public static void WireAllScenes()
        {
            WireApartment();
            WireCityStreet();
            WireSuyaSpot();
            WireNightclub();
            WireBeach();
            
            Debug.Log("All scenes have camera follow targets wired.");
        }
        
        [MenuItem("NAAD/Wire Camera Follow/Apartment")]
        public static void WireApartment()
        {
            WireScene("Assets/NAAD/Scenes/Apartment.unity");
        }
        
        [MenuItem("NAAD/Wire Camera Follow/CityStreet")]
        public static void WireCityStreet()
        {
            WireScene("Assets/NAAD/Scenes/CityStreet.unity");
        }
        
        [MenuItem("NAAD/Wire Camera Follow/SuyaSpot")]
        public static void WireSuyaSpot()
        {
            WireScene("Assets/NAAD/Scenes/SuyaSpot.unity");
        }
        
        [MenuItem("NAAD/Wire Camera Follow/Nightclub")]
        public static void WireNightclub()
        {
            WireScene("Assets/NAAD/Scenes/Nightclub.unity");
        }
        
        [MenuItem("NAAD/Wire Camera Follow/Beach")]
        public static void WireBeach()
        {
            WireScene("Assets/NAAD/Scenes/Beach.unity");
        }
        
        private static void WireScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var player = GameObject.Find("Player");
            var cameraObj = GameObject.Find("ThirdPersonCamera");
            
            if (player != null && cameraObj != null)
            {
                var cameraController = cameraObj.GetComponent<ThirdPersonCamera>();
                if (cameraController != null)
                {
                    // Use SerializedObject to set the target
                    var serializedObject = new SerializedObject(cameraController);
                    var targetProperty = serializedObject.FindProperty("_target");
                    if (targetProperty != null)
                    {
                        targetProperty.objectReferenceValue = player.transform;
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log($"Wired camera target to Player in {scenePath}");
                    }
                }
            }
            
            EditorSceneManager.SaveScene(scene);
        }
    }
}
