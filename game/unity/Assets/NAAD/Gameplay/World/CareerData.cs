using UnityEngine;

namespace NAAD.Gameplay.ContentExpansion
{
    /// <summary>
    /// Data model for a career field in the content expansion system (Gate 17).
    /// Used by both server-side content config and client UI.
    /// </summary>
    [CreateAssetMenu(fileName = "Careers", menuName = "NAAD/Content/Careers", order = 100)]
    public sealed class CareerData : ScriptableObject
    {
        public string Id;
        public string Label;
        public string Description;
        public string[] RelatedLocations;
        public int DefaultSalaryNgn;
        public Sprite Icon;
    }
}