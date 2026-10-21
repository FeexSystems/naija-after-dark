using UnityEngine;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// High-level animator driver for the player character.
    /// <para>
    /// Maps locomotion state (speed, direction, run flag) to Animator parameters
    /// and exposes methods for one-shot social/phone/food animations triggered by
    /// the gameplay layer. Keeps the Animator logic separate from movement logic.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [Header("Animator Layers")]
        [SerializeField]
        [Tooltip("Index of the base locomotion layer.")]
        private int _locomotionLayerIndex = 0;

        [SerializeField]
        [Tooltip("Index of the social interaction overlay layer.")]
        private int _socialLayerIndex = 1;

        [SerializeField]
        [Tooltip("Index of the phone interaction overlay layer.")]
        private int _phoneLayerIndex = 2;

        [SerializeField]
        [Tooltip("Index of the seating override layer.")]
        private int _seatingLayerIndex = 3;

        private Animator _animator;

        // Parameter hashes (set in Awake)
        private int _speedHash;
        private int _directionHash;
        private int _isRunningHash;
        private int _isMovingHash;
        private int _socialTriggerHash;
        private int _phoneTriggerHash;
        private int _seatedHash;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("[PlayerAnimator] Animator component is missing.");
                return;
            }

            CacheHashes();
            SetupLayers();
        }

        /// <summary>
        /// Set locomotion blend values. Call from <see cref="PlayerLocomotionController"/>
        /// each frame while moving.
        /// </summary>
        /// <param name="speed">Current forward speed (m/s).</param>
        /// <param name="direction">Normalized input direction magnitude (0-1).</param>
        /// <param name="isRunning">Whether the run animation should be active.</param>
        /// <param name="isMoving">Whether the character is moving at all.</param>
        public void SetLocomotion(float speed, float direction, bool isRunning, bool isMoving)
        {
            if (_animator == null) return;

            _animator.SetFloat(_speedHash, speed, 0.1f, Time.deltaTime);
            _animator.SetFloat(_directionHash, direction, 0.1f, Time.deltaTime);
            _animator.SetBool(_isRunningHash, isRunning);
            _animator.SetBool(_isMovingHash, isMoving);

            // Ensure the locomotion layer is active
            SetLayerWeight(_locomotionLayerIndex, 1f);
        }

        /// <summary>
        /// Trigger a one-shot social animation (greeting, conversation, etc.).
        /// </summary>
        /// <param name="animationName">Name of the trigger parameter in the Animator.</param>
        public void TriggerSocial(string animationName)
        {
            if (_animator == null) return;
            SetLayerWeight(_socialLayerIndex, 1f);
            _animator.SetTrigger(animationName);
        }

        /// <summary>
        /// Trigger a one-shot phone animation (retrieve, browse, put away, etc.).
        /// </summary>
        /// <param name="animationName">Name of the trigger parameter in the Animator.</param>
        public void TriggerPhone(string animationName)
        {
            if (_animator == null) return;
            SetLayerWeight(_phoneLayerIndex, 1f);
            _animator.SetTrigger(animationName);
        }

        /// <summary>
        /// Set seated state (overrides base locomotion when sitting).
        /// </summary>
        /// <param name="isSeated">Whether the character is seated.</param>
        public void SetSeated(bool isSeated)
        {
            if (_animator == null) return;
            _animator.SetBool(_seatedHash, isSeated);
            SetLayerWeight(_seatingLayerIndex, isSeated ? 1f : 0f);
        }

        /// <summary>
        /// Set the weight of an Animator layer.
        /// </summary>
        /// <param name="layerIndex">Layer index.</param>
        /// <param name="weight">Weight (0-1).</param>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (_animator == null) return;
            if (layerIndex < 0 || layerIndex >= _animator.layerCount) return;

            _animator.SetLayerWeight(layerIndex, Mathf.Clamp01(weight));
        }

        private void CacheHashes()
        {
            _speedHash = Animator.StringToHash("Speed");
            _directionHash = Animator.StringToHash("Direction");
            _isRunningHash = Animator.StringToHash("IsRunning");
            _isMovingHash = Animator.StringToHash("IsMoving");
            _socialTriggerHash = Animator.StringToHash("SocialTrigger");
            _phoneTriggerHash = Animator.StringToHash("PhoneTrigger");
            _seatedHash = Animator.StringToHash("IsSeated");
        }

        private void SetupLayers()
        {
            if (_animator == null) return;

            // Ensure base locomotion layer is fully active
            SetLayerWeight(_locomotionLayerIndex, 1f);

            // Other layers start at 0 and are activated on demand
            SetLayerWeight(_socialLayerIndex, 0f);
            SetLayerWeight(_phoneLayerIndex, 0f);
            SetLayerWeight(_seatingLayerIndex, 0f);
        }
    }
}