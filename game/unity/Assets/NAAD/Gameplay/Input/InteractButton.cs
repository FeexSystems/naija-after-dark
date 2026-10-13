using NAAD.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NAAD.Gameplay.Input
{
    /// <summary>
    /// Mobile interact button. Forwards taps to the InteractionSystem through its
    /// public TryInteract() seam — the same entry point the editor E-key uses — so
    /// desktop and device share one interaction path and prompts stay consistent.
    ///
    /// Sized per the UI component library minimum 44dp touch target; the default
    /// size is expressed in canvas units and should sit inside a SafeAreaFitter
    /// panel so the notch never covers it.
    /// </summary>
    public sealed class InteractButton : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Interaction system driven by this button. Resolved from the scene when unassigned.")]
        [SerializeField] private InteractionSystem? interactionSystem;

        private void Awake()
        {
            if (interactionSystem == null)
                interactionSystem = FindFirstObjectByType<InteractionSystem>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TryInteract();
        }

        /// <summary>Public for binding from other UI (e.g. a UI Toolkit button).</summary>
        public void TryInteract()
        {
            interactionSystem?.TryInteract();
        }
    }
}
