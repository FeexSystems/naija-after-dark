using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NAAD.Gameplay.ContentExpansion;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for content expansion opportunities screen.
    /// Displays registered locations, careers, and available opportunities.
    /// </summary>
    public class ContentExpansionUIController : BaseScreenController
    {
        [Header("Location Content")]
        [SerializeField] private GameObject locationItemPrefab;
        [SerializeField] private Transform locationContentPanel;
        
        [Header("Career Content")]
        [SerializeField] private GameObject careerItemPrefab;
        [SerializeField] private Transform careerContentPanel;
        
        [Header("Opportunity Content")]
        [SerializeField] private GameObject opportunityItemPrefab;
        [SerializeField] private Transform opportunityContentPanel;

        private void OnEnable()
        {
            InitializeContent();
        }
        
        private void InitializeContent()
        {
            DisplayLocations();
            DisplayCareers();
            DisplayOpportunities();
        }
        
        private void DisplayLocations()
        {
            if (locationContentPanel == null || locationItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in locationContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            // Get registered locations from ContentExpansionService
            var locations = ContentExpansionService.Locations;
            
            foreach (var loc in locations)
            {
                var item = Instantiate(locationItemPrefab, locationContentPanel);
                var itemText = item.GetComponentInChildren<Text>();
                if (itemText != null)
                {
                    itemText.text = "" + loc.Value.Name + "\n" +
                                   "Type: " + loc.Value.Type + "\n" +
                                   "Description: " + loc.Value.Description + "\n" +
                                   "Cost to Enter: " + loc.Value.CostToEnter.ToString("N0") + " NGN\n" +
                                   "Interaction Range: " + loc.Value.InteractionRange.ToString("F1") + "f";
                }
            }
        }
        
        private void DisplayCareers()
        {
            if (careerContentPanel == null || careerItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in careerContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            // Get career IDs and related locations
            var careerIds = ContentExpansionService.CareerIds;
            
            foreach (var careerId in careerIds.Keys)
            {
                var relatedLocations = careerIds[careerId];
                var careerData = Resources.Load<CareerData>("Careers/" + careerId);
                
                var item = Instantiate(careerItemPrefab, careerContentPanel);
                var itemText = item.GetComponentInChildren<Text>();
                if (itemText != null)
                {
                    string careerInfo = careerData != null
                        ? "" + careerData.Label + "\n" +
                          "Description: " + careerData.Description + "\n" +
                          "Salary: " + careerData.DefaultSalaryNgn.ToString("N0") + " NGN\n" +
                          "Related Locations: " + string.Join(", ", careerData.RelatedLocations)
                        : "" + careerId + "\nRelated Locations: " + string.Join(", ", relatedLocations);

                    itemText.text = careerInfo;
                }
            }
        }
        
        private void DisplayOpportunities()
        {
            if (opportunityContentPanel == null || opportunityItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in opportunityContentPanel)
            {
                Destroy(child.gameObject);
            }
            
            // Get all opportunity IDs
            var opportunityIds = new List<string>(ContentExpansionService.Opportunities.Keys);
            
            foreach (var oppId in opportunityIds)
            {
                var opportunity = ContentExpansionService.Opportunities[oppId];
                var item = Instantiate(opportunityItemPrefab, opportunityContentPanel);
                var itemText = item.GetComponentInChildren<Text>();
                if (itemText != null)
                {
                    string oppInfo = "" + oppId + "\n" +
                                    "Type: " + opportunity + "\n" +
                                    "Available: " + ContentExpansionService.HasOpportunity(oppId);

                    itemText.text = oppInfo;
                }
            }
        }
    }
}