using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Gameplay.Interaction;

namespace NAAD.Editor
{
    public class SetupRemainingCreative
    {
        [MenuItem("NAAD/Setup Remaining Creative/All Features")]
        public static void SetupAllFeatures()
        {
            SetupBeachParty();
            SetupOpportunityBoard();
            SetupApartmentMessages();
            
            Debug.Log("All remaining creative features have been set up.");
        }
        
        [MenuItem("NAAD/Setup Remaining Creative/Beach Party")]
        public static void SetupBeachParty()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var nightBeachObjects = GameObject.Find("NightBeachObjects");
            if (nightBeachObjects != null)
            {
                var partyArea = GameObject.Find("BeachPartyArea");
                if (partyArea == null)
                {
                    partyArea = new GameObject("BeachPartyArea");
                    partyArea.transform.SetParent(nightBeachObjects.transform);
                    partyArea.transform.position = new Vector3(5, 0, 8);
                    
                    var interactable = partyArea.GetComponent<Interactable>();
                    if (interactable == null)
                    {
                        partyArea.AddComponent<Interactable>();
                    }
                    
                    // Add visual indicator
                    var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    indicator.transform.SetParent(partyArea.transform);
                    indicator.transform.localPosition = Vector3.zero;
                    indicator.transform.localScale = new Vector3(3f, 0.1f, 3f);
                    var renderer = indicator.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = new Color(1f, 0.3f, 0.5f, 0.7f);
                    }
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup beach party area");
        }
        
        [MenuItem("NAAD/Setup Remaining Creative/Opportunity Board")]
        public static void SetupOpportunityBoard()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var opportunityBoard = GameObject.Find("OpportunityBoard");
            if (opportunityBoard != null)
            {
                var interactable = opportunityBoard.GetComponent<Interactable>();
                if (interactable == null)
                {
                    opportunityBoard.AddComponent<Interactable>();
                }
                
                // Add visual indicator
                var indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                indicator.transform.SetParent(opportunityBoard.transform);
                indicator.transform.localPosition = Vector3.zero;
                indicator.transform.localScale = new Vector3(1f, 2f, 0.1f);
                var renderer = indicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0f, 0.8f, 1f, 0.7f);
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup opportunity board");
        }
        
        [MenuItem("NAAD/Setup Remaining Creative/Apartment Messages")]
        public static void SetupApartmentMessages()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            var messageDisplay = GameObject.Find("MessageDisplay");
            if (messageDisplay != null)
            {
                var interactable = messageDisplay.GetComponent<Interactable>();
                if (interactable == null)
                {
                    messageDisplay.AddComponent<Interactable>();
                }
            }
            
            var receiptPile = GameObject.Find("ReceiptPile");
            if (receiptPile != null)
            {
                var interactable = receiptPile.GetComponent<Interactable>();
                if (interactable == null)
                {
                    receiptPile.AddComponent<Interactable>();
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup apartment messages and receipts");
        }
    }
}
