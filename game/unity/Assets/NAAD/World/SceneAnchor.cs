using UnityEngine;

namespace NAAD.World
{
    /// <summary>
    /// Stable scene marker for spawn, travel, and interaction points.
    /// Presentation-only — authoritative travel still goes through command services.
    /// </summary>
    public sealed class SceneAnchor : MonoBehaviour
    {
        public enum AnchorKind
        {
            PlayerSpawn = 0,
            ExitTravel = 1,
            Interaction = 2,
            CameraRig = 3,
            Landmark = 4
        }

        [SerializeField] private string contentId = "";
        [SerializeField] private AnchorKind kind = AnchorKind.Landmark;
        [SerializeField] private string displayName = "";
        [SerializeField] private string travelSceneName = "";

        public string ContentId => contentId;
        public AnchorKind Kind => kind;
        public string DisplayName => displayName;
        public string TravelSceneName => travelSceneName;

        public void Configure(string id, AnchorKind anchorKind, string name, string travelScene = "")
        {
            contentId = id;
            kind = anchorKind;
            displayName = name;
            travelSceneName = travelScene ?? "";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = kind switch
            {
                AnchorKind.PlayerSpawn => Color.green,
                AnchorKind.ExitTravel => Color.cyan,
                AnchorKind.Interaction => Color.yellow,
                AnchorKind.CameraRig => Color.magenta,
                _ => Color.white
            };
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.6f);
        }
#endif
    }
}
