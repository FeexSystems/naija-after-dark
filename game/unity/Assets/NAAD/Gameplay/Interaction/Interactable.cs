using System;
using UnityEngine;

namespace NAAD.Gameplay.Interaction
{
    /// <summary>
    /// The kind of interaction an object offers. Maps to the interaction types the
    /// First Night flow needs (talk to NPCs, use fixtures, buy from vendors).
    /// </summary>
    public enum InteractionType
    {
        Talk,
        Use,
        Buy
    }

    /// <summary>
    /// Marks a world object the player can interact with.
    ///
    /// Attach alongside a Collider (set to trigger for volume-based ranges) or let
    /// InteractionSystem discover it by distance. The prompt text is the single
    /// source of truth for what the HUD displays; there is no separate prompt
    /// component so prompts cannot drift from the interactable they describe.
    /// </summary>
    public sealed class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private InteractionType interactionType = InteractionType.Talk;
        [SerializeField] private string prompt = "Interact";
        [SerializeField] private float interactionRange = 2.5f;

        [Header("Behaviour")]
        [Tooltip("Player must be looking roughly at the object to interact (angle in degrees). 0 disables the check.")]
        [SerializeField, Range(0f, 180f)] private float facingConeDegrees = 0f;

        [Tooltip("When false the object reports as unavailable and cannot be interacted with.")]
        [SerializeField] private bool isEnabled = true;

        /// <summary>Raised when the player performs the interaction.</summary>
        public event Action<InteractionType> Interacted;

        public InteractionType Type => interactionType;
        public string Prompt => isEnabled ? prompt : string.Empty;
        public float Range => interactionRange;
        public bool IsEnabled => isEnabled;

        /// <summary>Content-side identifier, e.g. NightDistrictIds.Tunde for NPCs.</summary>
        [SerializeField] private string interactableId = string.Empty;
        public string InteractableId => interactableId;

        private void Reset()
        {
            // Sensible default when the component is added in the editor.
            prompt = interactionType switch
            {
                InteractionType.Talk => "Talk",
                InteractionType.Buy => "Buy",
                _ => "Use"
            };
        }

        /// <summary>
        /// True when the point is inside range. Distance is measured from this
        /// object's origin, so vendors and NPCs anchored at their counter/body work
        /// without extra configuration.
        /// </summary>
        public bool IsInRange(Vector3 point)
        {
            if (!isEnabled) return false;
            var distance = Vector3.Distance(transform.position, point);
            return distance <= interactionRange;
        }

        /// <summary>
        /// Optional facing check: the direction to the interactable must lie within
        /// facingConeDegrees of the look direction. Always true when the cone is 0.
        /// </summary>
        public bool IsInFacingCone(Vector3 lookOrigin, Vector3 lookDirection)
        {
            if (facingConeDegrees <= 0f) return true;
            var toTarget = transform.position - lookOrigin;
            if (toTarget.sqrMagnitude < 0.0001f) return true;
            var angle = Vector3.Angle(lookDirection.normalized, toTarget.normalized);
            return angle <= facingConeDegrees;
        }

        /// <summary>
        /// Perform the interaction. Returns false when the interactable is disabled;
        /// subscribers (e.g. the dialogue or vending flow) decide what the
        /// interaction actually does — this component only identifies and gates it.
        /// </summary>
        public bool Interact()
        {
            if (!isEnabled) return false;
            Interacted?.Invoke(interactionType);
            return true;
        }

        /// <summary>Editor/testing helper to toggle availability.</summary>
        public void SetEnabled(bool value)
        {
            isEnabled = value;
        }
    }
}
