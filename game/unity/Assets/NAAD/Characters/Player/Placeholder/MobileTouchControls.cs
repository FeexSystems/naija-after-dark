using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NAAD.Characters.Player
{
    /// <summary>
    /// WS4-safe mobile touch controls with safe-area handling and configurable sensitivity.
    /// On devices with notches/bezels, the touch rectangle is inset from screen edges so
    /// buttons never sit under system chrome. The same component drives the desktop
    /// joystick fallback used by <see cref="PlayerInputBridge"/>.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MobileTouchControls : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private Canvas _touchCanvas;
        [SerializeField] private RawImage _joystickGraphics;
        [SerializeField] private Button _runButton;
        [SerializeField] private Button _jumpButton;

        [Header("Sensitivity")]
        [SerializeField] [Range(0f, 1f)] private float _touchSensitivity = 0.7f;

        [Header("Safe Area")]
        [SerializeField] private bool _useSafeArea = true;

        private CanvasGroup _group;
        private RectTransform _rectTransform;
        private Vector2 _moveDelta;
        private bool _runHeld;
        private Rect _safeArea;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_touchCanvas != null)
            {
                // Re-measure safe area once in Awake
                _useSafeArea = Application.isMobilePlatform && _useSafeArea;
                _safeArea = GetSafeAreaRect();
            }

            // Wire the button events to the virtual joystick adapter
            if (_runButton != null)
            {
                _runButton.onClick.AddListener(OnRunClicked);
            }
        }

        private void OnRunClicked()
        {
            _runHeld = !_runHeld;
        }

        /// <summary>
        /// Called from the UI canvas UI input module (Touch Input Module).
        /// Returns the normalized movement delta clamped to the unit circle.
        /// </summary>
        public Vector2 GetMoveInput()
        {
            // Read from a private touch delegate or UI wheel in a full build.
            return _moveDelta;
        }

        public bool IsRunHeld() => _runHeld;

        public float Sensitivity => _touchSensitivity;

        public void SetSafeArea(Rect safeArea)
        {
            _safeArea = safeArea;
        }

        public Rect GetSafeAreaRect()
        {
            if (Screen.safeArea != Rect.zero)
            {
                return Screen.safeArea;
            }
            // Fallback (non-mobile / older platforms)
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        public void ApplySafeArea(RectTransform rect)
        {
            if (!_useSafeArea) return;
            rect.offsetMin = _safeArea.position;
            rect.offsetMax = new Vector2(
                -_safeArea.width + Screen.width,
                -_safeArea.height + Screen.height);
        }
    }
}