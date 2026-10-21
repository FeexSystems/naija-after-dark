using System;
using UnityEngine;
using UnityEngine.EventSystems;
using NAAD.Gameplay.Input;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// Bridges <see cref="VirtualJoystick"/> (and legacy keyboard input) into a
    /// move-input event consumed by <see cref="PlayerLocomotionController"/>.
    /// <para>
    /// This decouples the input source from the movement system so the same
    /// locomotion controller works with touch, keyboard, or future Input System
    /// integration without modification.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(VirtualJoystick))]
    [DisallowMultipleComponent]
    public sealed class PlayerInputBridge : MonoBehaviour
    {
        /// <summary>Fired every frame with normalized movement direction and run state.</summary>
        public event Action<Vector2, bool> OnMoveInput;

        [Header("Run Input")]
        [SerializeField]
        [Tooltip("Key held to run (desktop fallback only). Touch run can be added via UI button.")]
        private KeyCode _runKey = KeyCode.LeftShift;

        private VirtualJoystick _joystick;

        private void Awake()
        {
            _joystick = GetComponent<VirtualJoystick>();
            if (_joystick == null)
            {
                Debug.LogError("[PlayerInputBridge] VirtualJoystick component is missing.");
            }
        }

        private void Update()
        {
            if (_joystick == null) return;

            var direction = _joystick.Direction;
            var isRunPressed = _joystick.IsActive && UnityEngine.Input.GetKey(_runKey);

            // Also support toggle-run via double-tap or a UI button in the future.
            // For now, LeftShift on desktop; touch devices use walk speed by default.

            OnMoveInput?.Invoke(direction, isRunPressed);
        }
    }
}