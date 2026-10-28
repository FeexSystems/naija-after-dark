using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupCreativeScenes
    {
        [MenuItem("NAAD/Setup Creative Scenes/All Creative Features")]
        public static void SetupAllCreativeFeatures()
        {
            SetupOccupiedNightclub();
            SetupOpportunities();
            SetupWeatherOnStreet();
            SetupBeachByPeriod();
            SetupApartmentAftermath();
            
            Debug.Log("All creative scene features have been set up.");
        }
        
        [MenuItem("NAAD/Setup Creative Scenes/SC-1 Occupied Nightclub")]
        public static void SetupOccupiedNightclub()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var crowdManager = new GameObject("CrowdManager");
            var danceFloorCrowd = new GameObject("DanceFloorCrowd");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-1: Occupied Nightclub");
        }
        
        [MenuItem("NAAD/Setup Creative Scenes/SC-4 Opportunities")]
        public static void SetupOpportunities()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var opportunityManager = new GameObject("OpportunityManager");
            var opportunityBoard = new GameObject("OpportunityBoard");
            opportunityBoard.transform.position = new Vector3(0, 1.5f, 0);
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-4: Opportunities");
        }
        
        [MenuItem("NAAD/Setup Creative Scenes/SC-5 Weather on Street")]
        public static void SetupWeatherOnStreet()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var weatherSystem = new GameObject("WeatherSystem");
            var rainSystem = new GameObject("RainSystem");
            rainSystem.transform.SetParent(weatherSystem.transform);
            rainSystem.transform.position = new Vector3(0, 20, 0);
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-5: Weather on Street");
        }
        
        [MenuItem("NAAD/Setup Creative Scenes/SC-6 Beach by Period")]
        public static void SetupBeachByPeriod()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var beachEventManager = new GameObject("BeachEventManager");
            var dayBeachObjects = new GameObject("DayBeachObjects");
            var nightBeachObjects = new GameObject("NightBeachObjects");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-6: Beach by Period");
        }
        
        [MenuItem("NAAD/Setup Creative Scenes/SC-7 Apartment Aftermath")]
        public static void SetupApartmentAftermath()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            var aftermathManager = new GameObject("AftermathManager");
            var messageDisplay = new GameObject("MessageDisplay");
            messageDisplay.transform.position = new Vector3(-1, 1.2f, 1);
            var receiptPile = new GameObject("ReceiptPile");
            receiptPile.transform.position = new Vector3(2, 0.9f, 1);
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-7: Apartment Aftermath");
        }
    }
}
