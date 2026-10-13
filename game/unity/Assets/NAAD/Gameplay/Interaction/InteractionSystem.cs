using System.Collections.Generic;
using NAAD.UI;
using UnityEngine;

namespace NAAD.Gameplay.Interaction
{
    /// <summary>
    /// Player-side interaction detection.
    /// Attach to the player character. Each frame it finds the best available
    /// Interactable in range (closest, facing-cone aware), shows its prompt on the
    /// HUD, and forwards the interact input to it. Discovery uses OverlapSphere so
    /// it works with both trigger and solid colliders, and does not require the
    /// interactable to have a rigidbody.
    ///
    /// UI feedback goes through MainHudController.ShowInteractionPrompt /
    /// HideInteractionPrompt — the same contract MainHudController already exposes,
    /// so prompts cannot drift from the HUD. If no HUD is present (headless tests),
    /// the prompt is still tracked and readable via CurrentPrompt.
    /// </summary>
    public sealed class InteractionSystem : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional HUD controller; prompt is shown here when assigned.")]
        [SerializeField] private MainHudController? hud;

        [Header("Discovery")]
        [Tooltip("Radius scanned for interactables. Interactables with a larger range than this are still found if their origin is within this radius.")]
        [SerializeField] private float scanRadius = 6f;

        [Tooltip("Layer mask containing interactable colliders. Defaults to everything.")]
        [SerializeField] private LayerMask interactableMask = ~0;

        [Tooltip("Tag of the transform this system treats as the look/eye origin. Falls back to the component's own transform.")]
        [SerializeField] private string cameraTag = "MainCamera";

        [Header("Input")]
        [Tooltip("Key that performs the interaction in the editor. Touch/joystick wiring lands with the Android input pass.")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        public Interactable? CurrentTarget { get; private set; }
        public string CurrentPrompt { get; private set; } = string.Empty;

        /// <summary>Raised when the focused interactable changes (or becomes null).</summary>
        public event System.Action<Interactable?> TargetChanged;

        private static readonly Collider[] Buffer = new Collider[32];
        private Interactable? _lastTarget;

        private void Update()
        {
            var focus = FindBestTarget();
            SetTarget(focus);

            if (CurrentTarget != null && Input.GetKeyDown(interactKey))
                TryInteract();
        }

        /// <summary>
        /// Closest enabled interactable within its own range and the facing cone.
        /// Returns null when nothing qualifies, so prompts never point at something
        /// the player cannot actually use.
        /// </summary>
        private Interactable? FindBestTarget()
        {
            var origin = transform.position;
            var lookOrigin = ResolveLookOrigin();
            var lookDirection = ResolveLookDirection();

            var count = Physics.OverlapSphereNonAlloc(origin, scanRadius, Buffer, interactableMask, QueryTriggerInteraction.Collide);
            Interactable? best = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < count; i++)
            {
                var candidate = Buffer[i].GetComponentInParent<Interactable>();
                if (candidate == null || !candidate.enabled) continue;
                if (!candidate.IsInRange(origin)) continue;
                if (!candidate.IsInFacingCone(lookOrigin, lookDirection)) continue;

                var distance = Vector3.Distance(origin, candidate.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private void SetTarget(Interactable? target)
        {
            if (ReferenceEquals(CurrentTarget, target))
                return;

            CurrentTarget = target;
            CurrentPrompt = target != null ? target.Prompt : string.Empty;
            TargetChanged?.Invoke(target);
            UpdateHud();
        }

        /// <summary>
        /// Perform the interaction on the current target. Public so UI buttons and
        /// future touch input can call it — the key handler is just one caller.
        /// </summary>
        public bool TryInteract()
        {
            var target = CurrentTarget;
            if (target == null || !target.Interact())
                return false;

            var log = NAAD.Core.Bootstrap.NAADApplicationRoot.Instance?.Logger;
            log?.Info("Interaction", $"Interacted with {target.name} ({target.Type})");
            return true;
        }

        private void UpdateHud()
        {
            if (hud == null) return;

            if (CurrentTarget != null && !string.IsNullOrEmpty(CurrentPrompt))
                hud.ShowInteractionPrompt(CurrentPrompt);
            else
                hud.HideInteractionPrompt();
        }

        private Vector3 ResolveLookOrigin()
        {
            var cam = GameObject.FindGameObjectWithTag(cameraTag);
            return cam != null ? cam.transform.position : transform.position + Vector3.up;
        }

        private Vector3 ResolveLookDirection()
        {
            var cam = GameObject.FindGameObjectWithTag(cameraTag);
            return cam != null ? cam.transform.forward : transform.forward;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.85f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
    }
}
