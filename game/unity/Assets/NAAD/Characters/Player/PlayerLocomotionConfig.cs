using UnityEngine;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// Serialized configuration for <see cref="PlayerLocomotionController"/>.
    /// Create as a ScriptableObject asset or embed on the prefab via [SerializeField].
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerLocomotionConfig", menuName = "NAAD/Player/Locomotion Config", order = 100)]
    public sealed class PlayerLocomotionConfig : ScriptableObject
    {
        [Header("Movement Speeds (m/s)")]
        [SerializeField]
        [Tooltip("Forward walking speed.")]
        private float _walkSpeed = 1.6f;

        [SerializeField]
        [Tooltip("Forward running speed.")]
        private float _runSpeed = 3.2f;

        [SerializeField]
        [Tooltip("Rotation speed (degrees per second) when aligning to movement direction.")]
        private float _rotationSpeed = 360f;

        [Header("Navigation")]
        [SerializeField]
        [Tooltip("Stopping distance from the NavMesh destination.")]
        private float _stoppingDistance = 0.15f;

        [SerializeField]
        [Tooltip("Area mask the NavMeshAgent can traverse.")]
        private int _navMeshAreaMask = NavMesh.AllAreas;

        [Header("Input")]
        [SerializeField]
        [Tooltip("Deadzone for virtual joystick input (0-1). Values below this are treated as zero.")]
        [Range(0f, 1f)]
        private float _joystickDeadzone = 0.1f;

        [SerializeField]
        [Tooltip("Input threshold (0-1) above which the character transitions from idle to walk.")]
        [Range(0f, 1f)]
        private float _walkThreshold = 0.25f;

        [SerializeField]
        [Tooltip("Input threshold (0-1) above which the character transitions from walk to run.")]
        [Range(0f, 1f)]
        private float _runThreshold = 0.7f;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float StoppingDistance => _stoppingDistance;
        public int NavMeshAreaMask => _navMeshAreaMask;
        public float JoystickDeadzone => _joystickDeadzone;
        public float WalkThreshold => _walkThreshold;
        public float RunThreshold => _runThreshold;
    }
}