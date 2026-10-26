using UnityEngine;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// Third-person camera that follows the player with smoothing, configurable
    /// offset, and optional collision-based radius protection.
    /// <para>
    /// The camera observes the player transform and does not drive movement,
    /// keeping the locomotion system fully decoupled from the camera.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform _target;

        [Header("Offset")]
        [SerializeField]
        [Tooltip("Camera offset relative to the target position.")]
        private Vector3 _offset = new Vector3(0f, 1.8f, -4.5f);

        [SerializeField]
        [Tooltip("Look-at offset from the target's pivot (e.g., head height).")]
        private Vector3 _lookAtOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Smoothing")]
        [SerializeField]
        [Range(0f, 50f)]
        [Tooltip("Position follow speed. Higher = snappier.")]
        private float _positionSmoothSpeed = 10f;

        [SerializeField]
        [Range(0f, 50f)]
        [Tooltip("Rotation follow speed. Higher = snappier.")]
        private float _rotationSmoothSpeed = 10f;

        [Header("Collision")]
        [SerializeField]
        [Tooltip("Enable camera collision with geometry to prevent clipping through walls.")]
        private bool _enableCollision = true;

        [SerializeField]
        [Range(0f, 2f)]
        [Tooltip("Radius of the camera sphere for collision detection.")]
        private float _collisionRadius = 0.25f;

        [SerializeField]
        [Tooltip("Layers the camera can collide with.")]
        private LayerMask _collisionMask = ~0;

        private Vector3 _currentOffset;
        private Vector3 _desiredPosition;
        private Vector3 _desiredLookAt;

        /// <summary>Current camera target (may be null).</summary>
        public Transform Target => _target;

        private void LateUpdate()
        {
            if (_target == null) return;

            UpdateDesiredPositions();

            if (_enableCollision)
            {
                HandleCameraCollision();
            }

            // Smoothly interpolate position and look-at
            transform.position = Vector3.Lerp(transform.position, _desiredPosition, _positionSmoothSpeed * Time.deltaTime);
            transform.LookAt(_desiredLookAt);

            // Smooth rotation toward target-facing
            var targetRotation = Quaternion.LookRotation(_desiredLookAt - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSmoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Set the camera target at runtime (e.g., after spawning the player).
        /// </summary>
        /// <param name="target">New target transform.</param>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                // Snap immediately on target change
                UpdateDesiredPositions();
                transform.position = _desiredPosition;
                transform.LookAt(_desiredLookAt);
            }
        }

        private void UpdateDesiredPositions()
        {
            var targetPosition = _target.position;
            var targetLookAt = targetPosition + _lookAtOffset;

            // Rotate offset by target's yaw so camera follows behind the character
            var targetRotation = Quaternion.Euler(0f, _target.eulerAngles.y, 0f);
            var rotatedOffset = targetRotation * _offset;

            _desiredPosition = targetPosition + rotatedOffset;
            _desiredLookAt = targetLookAt;
        }

        private void HandleCameraCollision()
        {
            var direction = (_desiredPosition - _target.position).normalized;
            var distance = Vector3.Distance(_target.position, _desiredPosition);

            if (Physics.SphereCast(_target.position + _lookAtOffset, _collisionRadius, direction, out var hit, distance, _collisionMask))
            {
                _desiredPosition = hit.point - direction * _collisionRadius;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_target == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_target.position + _lookAtOffset, _collisionRadius);
            Gizmos.DrawLine(_target.position + _lookAtOffset, _desiredPosition);
        }
    }
}