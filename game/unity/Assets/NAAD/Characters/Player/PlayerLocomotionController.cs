using System;
using UnityEngine;
using UnityEngine.AI;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// Authoritative client-side locomotion controller for the player character.
    /// <para>
    /// Movement is driven by <see cref="PlayerInputBridge"/> and executed through a
    /// <see cref="NavMeshAgent"/>. The Animator is driven by normalized speed and
    /// direction parameters. All movement is client-predicted; the server validates
    /// via the <c>TRAVEL</c> command and may correct position if desynchronized.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public sealed class PlayerLocomotionController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private PlayerLocomotionConfig _config;

        [Header("References")]
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private NavMeshAgent _agent;

        [Header("Runtime State")]
        [SerializeField]
        [ReadOnly]
        private Vector3 _currentMoveDirection = Vector3.zero;

        [SerializeField]
        [ReadOnly]
        private float _currentSpeed = 0f;

        [SerializeField]
        [ReadOnly]
        private bool _isRunning = false;

        private PlayerInputBridge _inputBridge;
        private int _animSpeedHash;
        private int _animDirectionHash;
        private int _animIsRunningHash;
        private int _animIsMovingHash;

        /// <summary>Current normalized movement input magnitude (0-1).</summary>
        public float InputMagnitude { get; private set; }

        /// <summary>Current movement direction in world space.</summary>
        public Vector3 MoveDirection => _currentMoveDirection;

        /// <summary>Whether the agent has reached its destination.</summary>
        public bool IsAtDestination => _agent != null && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;

        /// <summary>Whether the character is currently running.</summary>
        public bool IsRunning => _isRunning;

        private void Awake()
        {
            ValidateReferences();
            CacheAnimatorHashes();
            ConfigureAgent();
        }

        private void Start()
        {
            _inputBridge = GetComponent<PlayerInputBridge>();
            if (_inputBridge == null)
            {
                _inputBridge = gameObject.AddComponent<PlayerInputBridge>();
                Debug.LogWarning("[PlayerLocomotionController] No PlayerInputBridge found; auto-added one.");
            }

            _inputBridge.OnMoveInput += HandleMoveInput;
        }

        private void OnDestroy()
        {
            if (_inputBridge != null)
            {
                _inputBridge.OnMoveInput -= HandleMoveInput;
            }
        }

        private void Update()
        {
            if (_agent == null || _config == null) return;

            UpdateSpeeds();
            UpdateRotation();
            UpdateAnimator();
        }

        /// <summary>
        /// Programmatically move the player to a world-space position.
        /// <para>
        /// This sets the NavMeshAgent destination. The server authoritative
        /// TRAVEL command should be issued separately by the gameplay layer.
        /// </para>
        /// </summary>
        /// <param name="destination">Target world-space position.</param>
        public void MoveTo(Vector3 destination)
        {
            if (_agent == null)
            {
                Debug.LogError("[PlayerLocomotionController] NavMeshAgent is null. Cannot move.");
                return;
            }

            if (!NavMesh.SamplePosition(destination, out var hit, 2.0f, _config.NavMeshAreaMask))
            {
                Debug.LogWarning($"[PlayerLocomotionController] Destination {destination} is not on NavMesh. Snapping to nearest.");
                destination = hit.position;
            }

            _agent.destination = destination;
            _agent.isStopped = false;
        }

        /// <summary>
        /// Immediately stop all movement.
        /// </summary>
        public void Stop()
        {
            if (_agent == null) return;
            _agent.isStopped = true;
            _agent.ResetPath();
            InputMagnitude = 0f;
            _currentMoveDirection = Vector3.zero;
        }

        /// <summary>
        /// Teleport the player to a position (use for scene transitions or spawn).
        /// </summary>
        /// <param name="position">Target world-space position.</param>
        public void Warp(Vector3 position)
        {
            if (_agent == null) return;
            _agent.Warp(position);
        }

        private void ValidateReferences()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_config == null)
            {
                _config = Resources.Load<PlayerLocomotionConfig>("PlayerLocomotionConfig");
                if (_config == null)
                {
                    Debug.LogWarning("[PlayerLocomotionController] No PlayerLocomotionConfig assigned. Using defaults.");
                }
            }
        }

        private void CacheAnimatorHashes()
        {
            _animSpeedHash = Animator.StringToHash("Speed");
            _animDirectionHash = Animator.StringToHash("Direction");
            _animIsRunningHash = Animator.StringToHash("IsRunning");
            _animIsMovingHash = Animator.StringToHash("IsMoving");
        }

        private void ConfigureAgent()
        {
            if (_agent == null) return;
            _agent.updateRotation = false; // We handle rotation manually for smooth turning
            _agent.updatePosition = true;
            _agent.stoppingDistance = _config.StoppingDistance;
            _agent.areaMask = _config.NavMeshAreaMask;
        }

        private void HandleMoveInput(Vector2 input, bool isRunPressed)
        {
            // Apply deadzone
            if (input.magnitude < _config.JoystickDeadzone)
            {
                input = Vector2.zero;
            }
            else
            {
                // Clamp to circle
                input = Vector2.ClampMagnitude(input, 1f);
            }

            InputMagnitude = input.magnitude;

            // Convert screen-space input to world-space direction relative to camera
            var cameraForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            var cameraRight = Camera.main != null ? Camera.main.transform.right : Vector3.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            _currentMoveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

            // Determine target speed based on input magnitude and run modifier
            var targetSpeed = 0f;
            _isRunning = isRunPressed && InputMagnitude >= _config.RunThreshold;

            if (InputMagnitude >= _config.RunThreshold)
            {
                targetSpeed = _isRunning ? _config.RunSpeed : _config.WalkSpeed;
            }
            else if (InputMagnitude >= _config.WalkThreshold)
            {
                targetSpeed = _config.WalkSpeed;
            }

            // Smooth speed interpolation
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _config.RotationSpeed * Time.deltaTime);

            // Set NavMeshAgent destination when there is meaningful input
            if (_agent != null && InputMagnitude >= _config.WalkThreshold && !_agent.pathPending)
            {
                var targetPosition = transform.position + _currentMoveDirection * (_currentSpeed * 2f);
                if (NavMesh.SamplePosition(targetPosition, out var hit, 2.0f, _config.NavMeshAreaMask))
                {
                    _agent.destination = hit.position;
                    _agent.speed = _currentSpeed > 0f ? _currentSpeed : _config.WalkSpeed;
                    _agent.isStopped = false;
                }
            }
            else if (_agent != null && InputMagnitude < _config.WalkThreshold)
            {
                _agent.isStopped = true;
            }
        }

        private void UpdateSpeeds()
        {
            if (_agent == null) return;
            _agent.speed = _currentSpeed > 0f ? _currentSpeed : _config.WalkSpeed;
        }

        private void UpdateRotation()
        {
            if (_agent == null || _currentMoveDirection.sqrMagnitude < 0.001f) return;

            var targetRotation = Quaternion.LookRotation(_currentMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _config.RotationSpeed * Time.deltaTime);
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            _animator.SetFloat(_animSpeedHash, _currentSpeed, 0.1f, Time.deltaTime);
            _animator.SetFloat(_animDirectionHash, InputMagnitude, 0.1f, Time.deltaTime);
            _animator.SetBool(_animIsRunningHash, _isRunning);
            _animator.SetBool(_animIsMovingHash, InputMagnitude >= _config.WalkThreshold);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_agent == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, _agent.radius);

            if (!Application.isPlaying) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _currentMoveDirection * 2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_agent.destination, 0.25f);
        }
#endif
    }

    /// <summary>
    /// Attribute to make a field read-only in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : PropertyAttribute { }