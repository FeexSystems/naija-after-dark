using UnityEngine;

namespace NAAD.Animation
{
    /// <summary>
    /// Script-driven, camera-relative player locomotion.
    ///
    /// Input is injected through SetMoveInput so mobile virtual joysticks, gamepad,
    /// keyboard adapters, and automated tests can share one movement implementation.
    /// This component owns local presentation/traversal only. Server-authoritative
    /// location, travel costs, wallet state, and command validation remain in the
    /// existing gameplay/service layer.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerLocomotionController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 2.0f;
        [SerializeField, Min(0f)] private float _runSpeed = 4.5f;
        [SerializeField, Min(0f)] private float _rotationSharpness = 12f;
        [SerializeField, Min(0f)] private float _gravity = 20f;
        [SerializeField] private Transform _cameraTransform;

        [Header("Animation")]
        [SerializeField] private AnimationStateManager _animationStateManager;
        [SerializeField, Range(0.01f, 1f)] private float _movementAnimationThreshold = 0.08f;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private bool _runHeld;
        private float _verticalVelocity;
        private bool _wasMoving;
        private bool _wasRunning;

        /// <summary>Current normalized movement input, useful for HUD/tests.</summary>
        public Vector2 MoveInput => _moveInput;

        /// <summary>Whether the controller is currently applying horizontal movement.</summary>
        public bool IsMoving => _moveInput.sqrMagnitude >= _movementAnimationThreshold * _movementAnimationThreshold;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_animationStateManager == null)
                _animationStateManager = GetComponent<AnimationStateManager>();

            if (_animationStateManager == null)
                _animationStateManager = GetComponentInChildren<AnimationStateManager>(true);

            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (_animationStateManager == null)
            {
                Debug.LogWarning(
                    "[NAAD][Locomotion] AnimationStateManager is not assigned. " +
                    "Movement will work, but animation commands will be skipped.", this);
            }
        }

        private void Update()
        {
            ApplyMovement(Time.deltaTime);
            UpdateAnimationState();
        }

        /// <summary>
        /// Supply a movement vector from a mobile joystick, gamepad, keyboard adapter,
        /// or test. Values are clamped to the unit circle.
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>Hold to run; release to walk. Intended for a mobile HUD button too.</summary>
        public void SetRunHeld(bool isHeld)
        {
            _runHeld = isHeld;
        }

        /// <summary>Immediately clear movement intent, for UI cancellation or scene exit.</summary>
        public void StopMovement()
        {
            _moveInput = Vector2.zero;
            _runHeld = false;
            UpdateAnimationState();
        }

        private void ApplyMovement(float deltaTime)
        {
            if (_characterController == null || deltaTime <= 0f)
                return;

            var inputMagnitude = _moveInput.magnitude;
            var isMoving = inputMagnitude >= _movementAnimationThreshold;

            Vector3 moveDirection = Vector3.zero;
            if (isMoving)
            {
                var forward = _cameraTransform != null
                    ? Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized
                    : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                var right = _cameraTransform != null
                    ? Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized
                    : Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

                moveDirection = Vector3.ClampMagnitude(
                    right * _moveInput.x + forward * _moveInput.y, 1f);

                if (moveDirection.sqrMagnitude > 0.0001f)
                {
                    var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    var blend = 1f - Mathf.Exp(-_rotationSharpness * deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blend);
                }
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity -= _gravity * deltaTime;

            var speed = _runHeld ? _runSpeed : _walkSpeed;
            var velocity = moveDirection * speed + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * deltaTime);
        }

        private void UpdateAnimationState()
        {
            if (_animationStateManager == null)
                return;

            var moving = IsMoving;
            var running = moving && _runHeld;

            if (!moving)
            {
                if (_wasMoving)
                    _animationStateManager.Stop();
                else if (_animationStateManager.GetCurrentState() != AnimationCommandType.LocomotionIdle)
                    _animationStateManager.SetIdle();
            }
            else if (running)
            {
                if (!_wasMoving || !_wasRunning)
                    _animationStateManager.SetRun(_runSpeed / Mathf.Max(_walkSpeed, 0.01f));
            }
            else if (!_wasMoving || _wasRunning)
            {
                _animationStateManager.SetWalk(1f);
            }

            _wasMoving = moving;
            _wasRunning = running;
        }
    }
}
