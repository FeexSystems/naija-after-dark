using System;
using System.Collections.Generic;
using UnityEngine;

namespace NAAD.Gameplay.ContentExpansion
{
    /// <summary>
    /// Types of extra opportunities available through content expansion (Gate 17).
    /// </summary>
    public enum ExtraOpportunity
    {
        None,
        ExtraLocation,
        ExtraCareer,
        SpecialEvent
    }

    /// <summary>
    /// Service for managing content expansion (Gate 17).
    /// Exposes extra locations, careers, and their associated command/rpc interfaces.
    /// </summary>
    public static class ContentExpansionService
    {
        private static readonly Dictionary<string, ContentExpansionLocations> _locationsTable =
            new Dictionary<string, ContentExpansionLocations>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, List<string>> _careerIds = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, ExtraOpportunity> _opportunities =
            new Dictionary<string, ExtraOpportunity>(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyDictionary<string, ContentExpansionLocations> Locations =
            new ReadOnlyDictionary<string, ContentExpansionLocations>(_locationsTable);

        public static IReadOnlyDictionary<string, List<string>> CareerIds =
            new ReadOnlyDictionary<string, List<string>>(_careerIds);

        public static IReadOnlyDictionary<string, ExtraOpportunity> Opportunities =
            new ReadOnlyDictionary<string, ExtraOpportunity>(_opportunities);

        public static void RegisterLocations(params ContentExpansionLocations[] locations)
        {
            foreach (var location in locations)
            {
                if (!_locationsTable.TryGetValue(location.name, out _))
                {
                    _locationsTable.Add(location.name, location);
                }
            }
        }

        public static void RegisterCareer(string careerId, string[] relatedLocationIds)
        {
            if (string.IsNullOrEmpty(careerId)) return;

            if (!_careerIds.TryGetValue(careerId, out var list))
            {
                list = new List<string>();
                _careerIds.Add(careerId, list);
            }

            foreach (var locId in relatedLocationIds)
            {
                if (!list.Contains(locId))
                    list.Add(locId);
            }
        }

        public static void RegisterOpportunity(string id, ExtraOpportunity opp)
        {
            if (string.IsNullOrEmpty(id)) return;
            _opportunities[id] = opp;
        }

        public static bool TryGetLocation(string id, out ContentExpansionLocations value)
        {
            return _locationsTable.TryGetValue(id, out value);
        }

        public static bool TryGetCareer(string id, out List<string> relatedLocations)
        {
            return _careerIds.TryGetValue(id, out relatedLocations);
        }

        public static bool TryGetOpportunity(string id, out ExtraOpportunity value)
        {
            return _opportunities.TryGetValue(id, out value);
        }

        public static bool HasOpportunity(string id) => _opportunities.ContainsKey(id);
    }
}
