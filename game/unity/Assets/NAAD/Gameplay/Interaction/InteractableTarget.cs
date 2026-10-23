using UnityEngine;

namespace NAAD.Gameplay.Interaction
{
    /// <summary>
    /// Lightweight editor-time interactable marker used by scene builders
    /// (e.g. NaadCityStreetBuilder). Unlike <see cref="Interactable"/>, its
    /// properties are settable so builders can configure type, prompt and
    /// content ID immediately after AddComponent.
    ///
    /// At runtime, InteractionSystem discovers these by component type and
    /// reads the configured values.
    /// </summary>
    public sealed class InteractableTarget : MonoBehaviour
    {
        public InteractionType Type { get; set; } = InteractionType.Talk;
        public string Prompt { get; set; } = "Interact";
        public string ContentId { get; set; } = string.Empty;
        public float Radius { get; set; } = 2.5f;
    }
}