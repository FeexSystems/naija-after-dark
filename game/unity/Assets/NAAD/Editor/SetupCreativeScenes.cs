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
        
        // SC-1: Occupied Nightclub (T3/T4) - Room presence integration, crowd visualization
        [MenuItem("NAAD/Setup Creative Scenes/SC-1 Occupied Nightclub")]
        public static void SetupOccupiedNightclub()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            // Add CrowdManager for room presence
            var crowdManager = new GameObject("CrowdManager");
            crowdManager.AddComponent<CrowdManager>();
            
            // Add NPC crowd members
            for (int i = 0; i < 5; i++)
            {
                var npc = new GameObject($"NightclubNPC_{i}");
                npc.transform.position = new Vector3(
                    Random.Range(-5f, 5f),
                    1f,
                    Random.Range(-5f, 5f)
                );
                npc.AddComponent<NPCController>();
                npc.AddComponent<Interactable>();
            }
            
            // Add dance floor crowd visualization
            var danceFloorCrowd = new GameObject("DanceFloorCrowd");
            danceFloorCrowd.transform.position = new Vector3(0, 0, 0);
            
            for (int i = 0; i < 8; i++)
            {
                var dancer = new GameObject($"Dancer_{i}");
                dancer.transform.SetParent(danceFloorCrowd.transform);
                dancer.transform.position = new Vector3(
                    Random.Range(-3f, 3f),
                    1f,
                    Random.Range(-3f, 3f)
                );
                dancer.AddComponent<NPCController>();
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-1: Occupied Nightclub with crowd system");
        }
        
        // SC-4: Opportunities (T4) - Client-side opportunity display and acceptance
        [MenuItem("NAAD/Setup Creative Scenes/SC-4 Opportunities")]
        public static void SetupOpportunities()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            // Add OpportunityManager
            var opportunityManager = new GameObject("OpportunityManager");
            opportunityManager.AddComponent<OpportunityManager>();
            
            // Add opportunity board/kiosk
            var opportunityBoard = new GameObject("OpportunityBoard");
            opportunityBoard.transform.position = new Vector3(0, 1.5f, 0);
            opportunityBoard.AddComponent<Interactable>();
            opportunityBoard.GetComponent<Interactable>().interactionLabel = "Check Opportunities";
            
            // Add visual indicator
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.transform.SetParent(opportunityBoard.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(1f, 2f, 0.1f);
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0f, 0.8f, 1f, 0.7f); // Cyan transparent
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-4: Opportunities with opportunity board");
        }
        
        // SC-5: Weather on Street (T1/T2) - Visual weather layer in CityStreet
        [MenuItem("NAAD/Setup Creative Scenes/SC-5 Weather on Street")]
        public static void SetupWeatherOnStreet()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            // Add WeatherSystem
            var weatherSystem = new GameObject("WeatherSystem");
            weatherSystem.AddComponent<WeatherSystem>();
            
            // Add rain particle system
            var rainSystem = new GameObject("RainSystem");
            rainSystem.transform.SetParent(weatherSystem.transform);
            rainSystem.transform.position = new Vector3(0, 20, 0);
            var rainParticles = rainSystem.AddComponent<ParticleSystem>();
            
            // Configure rain particles
            var main = rainParticles.main;
            main.startLifetime = 2f;
            main.startSpeed = 10f;
            main.startSize = 0.1f;
            main.gravityModifier = 1f;
            main.maxParticles = 1000;
            main.emissionRate = 100;
            
            var emission = rainParticles.emission;
            emission.rateOverTime = 100;
            
            var shape = rainParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(50, 0, 50);
            
            // Add fog for atmosphere
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.6f);
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-5: Weather on Street with rain system");
        }
        
        // SC-6: Beach by Period (T1/T4) - Time-based beach state, event visibility
        [MenuItem("NAAD/Setup Creative Scenes/SC-6 Beach by Period")]
        public static void SetupBeachByPeriod()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            // Find WorldClockService
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            // Add BeachEventManager
            var beachEventManager = new GameObject("BeachEventManager");
            var eventManager = beachEventManager.AddComponent<BeachEventManager>();
            eventManager.worldClockService = worldClockService.GetComponent<WorldClockService>();
            
            // Add time-dependent beach objects
            var dayBeachObjects = new GameObject("DayBeachObjects");
            dayBeachObjects.SetActive(true);
            
            var nightBeachObjects = new GameObject("NightBeachObjects");
            nightBeachObjects.SetActive(false);
            
            // Add beach bonfire (night only)
            var bonfire = new GameObject("Bonfire");
            bonfire.transform.SetParent(nightBeachObjects.transform);
            bonfire.transform.position = new Vector3(0, 0.5f, 10);
            var bonfireLight = bonfire.AddComponent<Light>();
            bonfireLight.type = LightType.Point;
            bonfireLight.intensity = 3f;
            bonfireLight.range = 10f;
            bonfireLight.color = new Color(1f, 0.5f, 0.2f); // Orange fire
            
            // Add beach party area (night only)
            var partyArea = new GameObject("BeachPartyArea");
            partyArea.transform.SetParent(nightBeachObjects.transform);
            partyArea.transform.position = new Vector3(5, 0, 8);
            partyArea.AddComponent<Interactable>();
            partyArea.GetComponent<Interactable>().interactionLabel = "Join Beach Party";
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-6: Beach by Period with time-dependent objects");
        }
        
        // SC-7: Apartment Aftermath (T5) - Messages and receipts display
        [MenuItem("NAAD/Setup Creative Scenes/SC-7 Apartment Aftermath")]
        public static void SetupApartmentAftermath()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            // Add AftermathManager
            var aftermathManager = new GameObject("AftermathManager");
            aftermathManager.AddComponent<AftermathManager>();
            
            // Add message display on phone/tablet
            var messageDisplay = new GameObject("MessageDisplay");
            messageDisplay.transform.position = new Vector3(-1, 1.2f, 1);
            messageDisplay.AddComponent<Interactable>();
            messageDisplay.GetComponent<Interactable>().interactionLabel = "Check Messages";
            
            // Add receipt pile
            var receiptPile = new GameObject("ReceiptPile");
            receiptPile.transform.position = new Vector3(2, 0.9f, 1);
            receiptPile.AddComponent<Interactable>();
            receiptPile.GetComponent<Interactable>().interactionLabel = "Review Receipts";
            
            // Add visual indicators
            var phoneIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            phoneIndicator.transform.SetParent(messageDisplay.transform);
            phoneIndicator.transform.localPosition = Vector3.zero;
            phoneIndicator.transform.localScale = new Vector3(0.3f, 0.5f, 0.1f);
            var phoneRenderer = phoneIndicator.GetComponent<Renderer>();
            if (phoneRenderer != null)
            {
                phoneRenderer.material.color = new Color(0.2f, 0.8f, 1f, 0.7f); // Cyan
            }
            
            var receiptIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            receiptIndicator.transform.SetParent(receiptPile.transform);
            receiptIndicator.transform.localPosition = Vector3.zero;
            receiptIndicator.transform.localScale = new Vector3(0.5f, 0.01f, 0.3f);
            var receiptRenderer = receiptIndicator.GetComponent<Renderer>();
            if (receiptRenderer != null)
            {
                receiptRenderer.material.color = new Color(1f, 1f, 0.8f, 0.7f); // Yellow
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup SC-7: Apartment Aftermath with messages and receipts");
        }
    }
    
    // Placeholder components (add to runtime codebase if not exists)
    public class CrowdManager : MonoBehaviour
    {
        // Manages room presence and crowd density
    }
    
    public class NPCController : MonoBehaviour
    {
        // Basic NPC behavior controller
    }
    
    public class OpportunityManager : MonoBehaviour
    {
        // Manages career and job opportunities
    }
    
    public class WeatherSystem : MonoBehaviour
    {
        // Manages weather effects (rain, fog, etc.)
    }
    
    public class BeachEventManager : MonoBehaviour
    {
        public WorldClockService worldClockService;
        
        void Update()
        {
            // Toggle day/night beach objects based on time
        }
    }
    
    public class AftermathManager : MonoBehaviour
    {
        // Manages aftermath messages and receipts
    }
}
