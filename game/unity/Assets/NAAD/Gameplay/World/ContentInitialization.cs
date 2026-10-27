using System.Collections.Generic;
using UnityEngine;

namespace NAAD.Gameplay.ContentExpansion
{
    /// <summary>
    /// Initializes sample content expansion data at startup (Gate 17).
    /// Registers sample locations and careers so they are available for UI and gameplay.
    /// </summary>
    public class ContentInitialization : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            // Register sample locations - these go into the ContentExpansionService locations table
            var locations = new ContentExpansionLocations[]
            {
                new ContentExpansionLocations
                {
                    Id = "rest_01",
                    Name = "Taste of Lagos Restaurant",
                    Description = "A popular Lagos restaurant serving jollof rice and grilled fish.",
                    Type = ContentExpansionLocations.LocationType.Restaurant,
                    InteractionRange = 3f,
                    CostToEnter = 2000
                },
                new ContentExpansionLocations
                {
                    Id = "lounge_01",
                    Name = "City Lounge",
                    Description = "Upscale lounge with views of the Lagos skyline.",
                    Type = ContentExpansionLocations.LocationType.Lounge,
                    InteractionRange = 2f,
                    CostToEnter = 3000
                },
                new ContentExpansionLocations
                {
                    Id = "hotel_01",
                    Name = "Eko Hotel & Suites",
                    Description = "Luxury hotel in the heart of the business district.",
                    Type = ContentExpansionLocations.LocationType.Hotel,
                    InteractionRange = 1f,
                    CostToEnter = 5000
                }
            };

            foreach (var loc in locations)
            {
                ContentExpansionService.RegisterLocations(loc);
            }

            // Register sample careers
            var careers = new CareerData[]
            {
                new CareerData
                {
                    Id = "tech_01",
                    Label = "Software Developer",
                    Description = "Developing software for fintech and startup companies.",
                    RelatedLocations = new[] { "rest_01", "lounge_01" },
                    DefaultSalaryNgn = 250000
                },
                new CareerData
                {
                    Id = "creative_01",
                    Label = "Creative Director",
                    Description = "Leading creative projects for advertising and media agencies.",
                    RelatedLocations = new[] { "lounge_01", "hotel_01" },
                    DefaultSalaryNgn = 300000
                },
                new CareerData
                {
                    Id = "hospitality_01",
                    Label = "Hotel Manager",
                    Description = "Managing hospitality operations for luxury hotels.",
                    RelatedLocations = new[] { "hotel_01" },
                    DefaultSalaryNgn = 180000
                }
            };

            foreach (var career in careers)
            {
                ContentExpansionService.RegisterCareer(career.Id, career.RelatedLocations);
            }

            // Register opportunities
            ContentExpansionService.RegisterOpportunity("tech_career", ExtraOpportunity.ExtraCareer);
            ContentExpansionService.RegisterOpportunity("city_lounge", ExtraOpportunity.ExtraLocation);
            ContentExpansionService.RegisterOpportunity("hotel_access", ExtraOpportunity.ExtraLocation);

            Debug.Log("[ContentInit] Gate 17 Content Expansion initialized with sample locations and careers.");
        }
    }
}