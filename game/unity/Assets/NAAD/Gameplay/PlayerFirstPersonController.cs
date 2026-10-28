using System;
using NAAD.Animation;
using NAAD.Gameplay.Interaction;
using UnityEngine;

namespace NAAD.Gameplay
{
    /// <summary>
    /// Third-person player controller: wires locomotion, animation, and interaction
    /// for the First Night vertical slice (Gate D). Matches the structure of the
    /// existing FirstNightController so it can be composed with the same game logic.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerLocomotionController))]
    [RequireComponent(typeof(InteractionSystem))]
    [RequireComponent(typeof(CharacterAnimationController))]
    [RequireComponent(typeof(AnimationStateManager))]
    public class PlayerFirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float runSpeed = 4.5f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float rotationSharpness = 12f;
        [SerializeField] private float animationThreshold = 0.08f;

        [Header("Formations")]
        [Tooltip("Number of players in a group; unused in single-player but provided for future multiplayer.")]
        [SerializeField] private int playerCount = 1;

        private PlayerLocomotionController _locomotion;
        private InteractionSystem _interaction;
        private CharacterAnimationController _animationController;
        private AnimationStateManager _animationStateManager;
        private CharacterController _characterController;
        private Camera _mainCamera;

        public Vector2 MoveInput => _locomotion != null ? _locomotion.MoveInput : Vector2.zero;
        public bool IsMoving => _locomotion != null ? _locomotion.IsMoving : false;
        public Interactable? CurrentInteractable => _interaction != null ? _interaction.CurrentTarget : null;
        public string CurrentPrompt => _interaction != null ? _interaction.CurrentPrompt : string.Empty;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _locomotion = GetComponent<PlayerLocomotionController>();
            _interaction = GetComponent<InteractionSystem>();
            _animationController = GetComponent<CharacterAnimationController>();
            _animationStateManager = GetComponent<AnimationStateManager>();

            _mainCamera = Camera.main ?? GameObject.FindObjectOfType<Camera>();
            _locomotion.cameraTransform = _mainCamera?.transform;

            // Best-effort wiring so the controller is self-sufficient.
            if (_locomotion.animationStateManager == null && _animationStateManager != null)
                _locomotion.animationStateManager = _animationStateManager;

            _animationStateManager ??= GetComponentInChildren<AnimationStateManager>(true);

            if (_mainCamera == null)
                Debug.LogWarning("[NAAD][Player] No main camera found; movement and interaction may be limited.", this);
        }

        private void Start()
        {
            _locomotion.SetMoveInput(Vector2.zero);
            _locomotion.SetRunHeld(false);
        }

        private void Update()
        {
            UpdateLocomotionInput();
            UpdateInteractionInput();
        }

        #region Input wiring

        private void UpdateLocomotionInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                _locomotion?.StopMovement();

            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift))
                _locomotion?.SetRunHeld(true);
            else if (UnityEngine.Input.GetKeyUp(KeyCode.LeftShift))
                _locomotion?.SetRunHeld(false);
        }

        private void UpdateInteractionInput()
        {
            if (_interaction == null) return;

            // Press E to interact. Unity input is handled in InteractionSystem.Update(),
            // but we expose a public API so automated tests and future UI buttons can call it.
            if (UnityEngine.Input.GetKeyDown(KeyCode.E) && _interaction.CurrentTarget != null)
                _interaction.TryInteract();
        }

        #endregion

        #region Public API (mirrors existing gameplay services)

        /// <summary>
        /// Tell the controller where the player is traveling to. The authoritative
        /// location service still owns persistence; this is the presentation layer only.
        /// </summary>
        public void SetDestination(string target)
        {
            // Presentation only: location changes are authoritative via ICommandService.
        }

        /// <summary>
        /// Invoke the interaction on the currently focused target.
        /// </summary>
        public void PerformInteraction()
        {
            _interaction?.TryInteract();
        }

        #endregion
    }
}
