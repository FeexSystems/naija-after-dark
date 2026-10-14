using UnityEngine;

namespace NAAD.World
{
    /// <summary>
    /// Marks NAAD-authored Environment placeholder prefabs (Gate B Phase 1).
    /// Carries the ENVIRONMENT_MANIFEST asset ID until vendor art replaces it.
    /// Runtime assembly so prefab YAML can reference it.
    /// </summary>
    public sealed class NaadPlaceholderTag : MonoBehaviour
    {
        [SerializeField] private string assetId = "";
        [TextArea] [SerializeField] private string description = "";

        public string AssetId { get => assetId; set => assetId = value; }
        public string Description { get => description; set => description = value; }
    }
}
