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
            
            Debug.Log("All scenes have been set up with interaction points.");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Apartment")]
        public static void SetupApartmentInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            // Define interaction points for Apartment
            CreateInteractionPoint("Door_Apartment_To_Street", new Vector3(2, 1, 0), "Exit to Street");
            CreateInteractionPoint("Bed", new Vector3(0, 0.5f, 2), "Sleep");
            CreateInteractionPoint("Phone", new Vector3(-1, 1.2f, 1), "Check Messages");
            CreateInteractionPoint("Fridge", new Vector3(3, 1, 2), "Get Food");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Apartment scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/CityStreet")]
        public static void SetupCityStreetInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            // Define interaction points for CityStreet
            CreateInteractionPoint("Door_Street_To_Apartment", new Vector3(-5, 1, 0), "Enter Apartment");
            CreateInteractionPoint("Door_Street_To_SuyaSpot", new Vector3(5, 1, 0), "Enter Suya Spot");
            CreateInteractionPoint("Door_Street_To_Nightclub", new Vector3(10, 1, 0), "Enter Nightclub");
            CreateInteractionPoint("Taxi_Stand", new Vector3(0, 1, 5), "Call Taxi");
            CreateInteractionPoint("Street_Vendor", new Vector3(-3, 1, 3), "Talk to Vendor");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for CityStreet scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/SuyaSpot")]
        public static void SetupSuyaSpotInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            // Define interaction points for SuyaSpot
            CreateInteractionPoint("Door_SuyaSpot_To_Street", new Vector3(0, 1, -5), "Exit to Street");
            CreateInteractionPoint("Suya_Stand", new Vector3(2, 1, 0), "Order Suya");
            CreateInteractionPoint("Table_1", new Vector3(-2, 0.8f, 2), "Sit");
            CreateInteractionPoint("Table_2", new Vector3(2, 0.8f, 2), "Sit");
            CreateInteractionPoint("Beverage_Stand", new Vector3(-3, 1, 0), "Get Drink");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for SuyaSpot scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Nightclub")]
        public static void SetupNightclubInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            // Define interaction points for Nightclub
            CreateInteractionPoint("Door_Nightclub_To_Street", new Vector3(0, 1, -8), "Exit to Street");
            CreateInteractionPoint("Bar", new Vector3(5, 1, 0), "Order Drink");
            CreateInteractionPoint("DJ_Booth", new Vector3(0, 1, 5), "Request Song");
            CreateInteractionPoint("Dance_Floor_Center", new Vector3(0, 1, 0), "Dance");
            CreateInteractionPoint("VIP_Area", new Vector3(-5, 1, 3), "Enter VIP");
            CreateInteractionPoint("Restroom", new Vector3(8, 1, -2), "Use Restroom");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Nightclub scene");
        }
        
        [MenuItem("NAAD/Setup Interaction Points/Beach")]
        public static void SetupBeachInteractionPoints()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            // Define interaction points for Beach
            CreateInteractionPoint("Beach_Vendor", new Vector3(0, 1, 10), "Buy Refreshments");
            CreateInteractionPoint("Beach_Chair_1", new Vector3(-5, 0.5f, 5), "Relax");
            CreateInteractionPoint("Beach_Chair_2", new Vector3(5, 0.5f, 5), "Relax");
            CreateInteractionPoint("Water_Edge", new Vector3(0, 0.5f, 15), "Swim");
            CreateInteractionPoint("Beach_Volleyball", new Vector3(10, 1, 0), "Play Volleyball");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup interaction points for Beach scene");
        }
        
        private static void CreateInteractionPoint(string name, Vector3 position, string interactionLabel)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            
            var interactable = obj.AddComponent<Interactable>();
            interactable.interactionLabel = interactionLabel;
            interactable.interactionRadius = 2f;
            
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
