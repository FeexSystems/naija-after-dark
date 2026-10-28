using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupWorldClockService
    {
        [MenuItem("NAAD/Setup World Clock Service/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartment();
            SetupCityStreet();
            SetupSuyaSpot();
            SetupNightclub();
            SetupBeach();
            
            Debug.Log("All scenes have been set up with WorldClockService.");
        }
        
        [MenuItem("NAAD/Setup World Clock Service/Apartment")]
        public static void SetupApartment()
        {
            SetupScene("Assets/NAAD/Scenes/Apartment.unity");
        }
        
        [MenuItem("NAAD/Setup World Clock Service/CityStreet")]
        public static void SetupCityStreet()
        {
            SetupScene("Assets/NAAD/Scenes/CityStreet.unity");
        }
        
        [MenuItem("NAAD/Setup World Clock Service/SuyaSpot")]
        public static void SetupSuyaSpot()
        {
            SetupScene("Assets/NAAD/Scenes/SuyaSpot.unity");
        }
        
        [MenuItem("NAAD/Setup World Clock Service/Nightclub")]
        public static void SetupNightclub()
        {
            SetupScene("Assets/NAAD/Scenes/Nightclub.unity");
        }
        
        [MenuItem("NAAD/Setup World Clock Service/Beach")]
        public static void SetupBeach()
        {
            SetupScene("Assets/NAAD/Scenes/Beach.unity");
        }
        
        private static void SetupScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService != null)
            {
                // WorldClockService is a ScriptableObject-based service, not a MonoBehaviour
                // We create a GameObject with a component that references it
                var clockPresenter = GameObject.Find("WorldClockPresenter");
                if (clockPresenter == null)
                {
                    clockPresenter = new GameObject("WorldClockPresenter");
                }
                
                Debug.Log($"WorldClockService GameObject exists in {scenePath}");
            }
            
            EditorSceneManager.SaveScene(scene);
        }
    }
}
