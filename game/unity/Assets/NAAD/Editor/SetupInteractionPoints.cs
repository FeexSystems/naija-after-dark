using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupInteractionPoints
    {
        [MenuItem("NAAD/Setup Interaction Points/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartmentInteractionPoints();
            SetupCityStreetInteractionPoints();
            SetupSuyaSpotInteractionPoints();
            SetupNightclubInteractionPoints();
            SetupBeachInteractionPoints();
            
            Debug.Log("All scenes have been set up with interaction point GameObjects.");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Apartment")]
        public static void SetupApartmentInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            CreateInteractionPoint("Door_Apartment_To_Street", new Vector3(2, 1, 0));
            CreateInteractionPoint("Bed", new Vector3(0, 0.5f, 2));
            CreateInteractionPoint("Phone", new Vector3(-1, 1.2f, 1));
            CreateInteractionPoint("Fridge", new Vector3(3, 1, 2));
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Apartment scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/CityStreet")]
        public static void SetupCityStreetInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            CreateInteractionPoint("Door_Street_To_Apartment", new Vector3(-5, 1, 0));
            CreateInteractionPoint("Door_Street_To_SuyaSpot", new Vector3(5, 1, 0));
            CreateInteractionPoint("Door_Street_To_Nightclub", new Vector3(10, 1, 0));
            CreateInteractionPoint("Taxi_Stand", new Vector3(0, 1, 5));
            CreateInteractionPoint("Street_Vendor", new Vector3(-3, 1, 3));
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for CityStreet scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/SuyaSpot")]
        public static void SetupSuyaSpotInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            CreateInteractionPoint("Door_SuyaSpot_To_Street", new Vector3(0, 1, -5));
            CreateInteractionPoint("Suya_Stand", new Vector3(2, 1, 0));
            CreateInteractionPoint("Table_1", new Vector3(-2, 0.8f, 2));
            CreateInteractionPoint("Table_2", new Vector3(2, 0.8f, 2));
            CreateInteractionPoint("Beverage_Stand", new Vector3(-3, 1, 0));
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for SuyaSpot scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Nightclub")]
        public static void SetupNightclubInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            CreateInteractionPoint("Door_Nightclub_To_Street", new Vector3(0, 1, -8));
            CreateInteractionPoint("Bar", new Vector3(5, 1, 0));
            CreateInteractionPoint("DJ_Booth", new Vector3(0, 1, 5));
            CreateInteractionPoint("Dance_Floor_Center", new Vector3(0, 1, 0));
            CreateInteractionPoint("VIP_Area", new Vector3(-5, 1, 3));
            CreateInteractionPoint("Restroom", new Vector3(8, 1, -2));
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Nightclub scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Beach")]
        public static void SetupBeachInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            CreateInteractionPoint("Beach_Vendor", new Vector3(0, 1, 10));
            CreateInteractionPoint("Beach_Chair_1", new Vector3(-5, 0.5f, 5));
            CreateInteractionPoint("Beach_Chair_2", new Vector3(5, 0.5f, 5));
            CreateInteractionPoint("Water_Edge", new Vector3(0, 0.5f, 15));
            CreateInteractionPoint("Beach_Volleyball", new Vector3(10, 1, 0));
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Beach scene");
        }
        
        private static void CreateInteractionPoint(string name, Vector3 position)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            
            // Add a visual indicator (sphere)
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.SetParent(obj.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = Vector3.one * 0.5f;
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange transparent
            }
        }
    }
}
