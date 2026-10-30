using UnityEngine;

namespace NAAD.Gameplay.Interaction
{
    public enum InteractionKind
    {
        None = 0,
        Talk = 1,
        Travel = 2,
        Spend = 3,
        AttendEvent = 4,
        Examine = 5
    }

    /// <summary>
    /// Gate D — world interaction point. Presentation only until PlayerInteractionController
    /// issues an authoritative command via ICommandService.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = "anchor";
        [SerializeField] private InteractionKind kind = InteractionKind.Examine;
        [SerializeField] private string prompt = "Interact";
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private string targetLocationId;
        [SerializeField] private string sku;
        [SerializeField] private string npcId;
        [SerializeField] private string eventId;

        public string AnchorId => anchorId;
        public InteractionKind Kind => kind;
        public string Prompt => prompt;
        public float Radius => radius;
        public string TargetLocationId => targetLocationId;
        public string Sku => sku;
        public string NpcId => npcId;
        public string EventId => eventId;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
