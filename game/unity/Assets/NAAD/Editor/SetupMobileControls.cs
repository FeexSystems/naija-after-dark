using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Characters.Player;

namespace NAAD.Editor
{
    public class SetupMobileControls
    {
        [MenuItem("NAAD/Setup Mobile Controls/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartment();
            SetupCityStreet();
            SetupSuyaSpot();
            SetupNightclub();
            SetupBeach();
            
            Debug.Log("All scenes have been set up with MobileTouchControls.");
        }
        
        [MenuItem("NAAD/Setup Mobile Controls/Apartment")]
        public static void SetupApartment()
        {
            SetupScene("Assets/NAAD/Scenes/Apartment.unity");
        }
        
        [MenuItem("NAAD/Setup Mobile Controls/CityStreet")]
        public static void SetupCityStreet()
        {
            SetupScene("Assets/NAAD/Scenes/CityStreet.unity");
        }
        
        [MenuItem("NAAD/Setup Mobile Controls/SuyaSpot")]
        public static void SetupSuyaSpot()
        {
            SetupScene("Assets/NAAD/Scenes/SuyaSpot.unity");
        }
        
        [MenuItem("NAAD/Setup Mobile Controls/Nightclub")]
        public static void SetupNightclub()
        {
            SetupScene("Assets/NAAD/Scenes/Nightclub.unity");
        }
        
        [MenuItem("NAAD/Setup Mobile Controls/Beach")]
        public static void SetupBeach()
        {
            SetupScene("Assets/NAAD/Scenes/Beach.unity");
        }
        
        private static void SetupScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var mobileControls = GameObject.Find("MobileControls");
            if (mobileControls != null)
            {
                var controls = mobileControls.GetComponent<MobileTouchControls>();
                if (controls == null)
                {
                    mobileControls.AddComponent<MobileTouchControls>();
                    Debug.Log($"Added MobileTouchControls to {scenePath}");
                }
            }
            
            EditorSceneManager.SaveScene(scene);
        }
    }
}
