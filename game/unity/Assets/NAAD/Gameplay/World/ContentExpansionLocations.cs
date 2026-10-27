using System;
using UnityEngine;

namespace NAAD.Gameplay.ContentExpansion
{
    /// <summary>
    /// Additional locations beyond the Night District core five.
    /// Part of Gate 17 - Content Expansion.
    /// </summary>
    [CreateAssetMenu(fileName = "ContentExpansionLocations", menuName = "NAAD/Content/ContentExpansionLocations", order = 100)]
    public sealed class ContentExpansionLocations : ScriptableObject
    {
        public LocationEntry[] ExtraLocations = new LocationEntry[0];

        [Serializable]
        public struct LocationEntry
        {
            public string Id;
            public string Name;
            public string Description;
            public LocationType Type;
            public float InteractionRange;
            public int CostToEnter;

            public enum LocationType
            {
                Restaurant,
                Lounge,
                Hotel,
                ATM,
                GasStation,
                Mall,
                Club
            }
        }
    }
}
