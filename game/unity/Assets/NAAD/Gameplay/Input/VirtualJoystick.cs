using UnityEngine;
using UnityEngine.EventSystems;

namespace NAAD.Gameplay.Input
{
    /// <summary>
    /// On-screen virtual joystick for touch movement.
    /// Attach to a full-screen uGUI panel; the visible stick base and handle are
    /// created automatically. Movement starts on touch anywhere within the dead
    /// zone radius of the base, and the stick re-centers on release — the standard
    /// "floating" behaviour mobile players expect.
    ///
    /// Output is camera-relative intent (Move/Forward, Move/Right in [-1, 1]),
    /// read by the player movement component. Legacy Input class + IPointer
    /// interfaces only: this project ships uGUI without the Input System package,
    /// and EventSystem touch routing works on both editor (mouse) and device.
    /// </summary>
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Visuals (auto-created if unassigned)")]
        [SerializeField] private RectTransform? stickBase;
        [SerializeField] private RectTransform? stickHandle;

        [Header("Feel")]
        [SerializeField] private float handleRange = 90f;
        [SerializeField] private float deadZone = 8f;

        [Header("Behaviour")]
        [Tooltip("Stick base follows the initial touch point instead of being fixed in place.")]
        [SerializeField] private bool floating = true;
        [Tooltip("When true the joystick also responds to editor/keyboard WASD for desktop testing.")]
        [SerializeField] private bool allowKeyboardFallback = true;

        /// <summary>Raw stick displacement, each axis in [-1, 1].</summary>
        public Vector2 Direction { get; private set; }

        /// <summary>True while the stick is held.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Magnitude after dead-zone shaping, in [0, 1] — use for walk/run blending.</summary>
        public float Magnitude => IsActive ? Direction.magnitude : 0f;

        private Canvas _canvas = null!;
        private Vector2 _baseHomePosition;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (stickBase == null) CreateStickVisuals();
            _baseHomePosition = stickBase!.anchoredPosition;
        }

        private void Update()
        {
            if (allowKeyboardFallback && !IsActive)
            {
                // Fully-qualified: this type lives in the NAAD.Gameplay.Input
                // namespace, so a bare "Input" would resolve to the namespace, not
                // UnityEngine.Input.
                var h = UnityEngine.Input.GetAxisRaw("Horizontal");
                var v = UnityEngine.Input.GetAxisRaw("Vertical");
                var keyboard = new Vector2(h, v);
                Direction = keyboard.sqrMagnitude > 1f ? keyboard.normalized : keyboard;
            }
            // When a touch is active, Direction comes from OnDrag only — keyboard
            // must not fight the stick.
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (floating && stickBase != null)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        transform as RectTransform, eventData.position,
                        eventData.pressEventCamera, out var localPoint))
                {
                    stickBase.anchoredPosition = localPoint;
                }
            }

            IsActive = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsActive || stickBase == null || stickHandle == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    stickBase, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            var offset = localPoint;
            if (offset.magnitude < deadZone)
            {
                stickHandle.anchoredPosition = Vector2.zero;
                Direction = Vector2.zero;
                return;
            }

            var clamped = Vector2.ClampMagnitude(offset, handleRange);
            stickHandle.anchoredPosition = clamped;
            Direction = clamped / handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsActive = false;
            Direction = Vector2.zero;
            if (stickHandle != null) stickHandle.anchoredPosition = Vector2.zero;
            if (floating && stickBase != null) stickBase.anchoredPosition = _baseHomePosition;
        }

        private void CreateStickVisuals()
        {
            var parentCanvas = GetComponentInParent<Canvas>().rootCanvas;

            var baseGo = new GameObject("StickBase", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var baseRect = (RectTransform)baseGo.transform;
            baseRect.SetParent(transform, false);
            baseRect.anchorMin = baseRect.anchorMax = new Vector2(0.15f, 0.15f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.sizeDelta = new Vector2(handleRange * 2.4f, handleRange * 2.4f);
            baseGo.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 0.15f);
            stickBase = baseRect;

            var handleGo = new GameObject("StickHandle", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var handleRect = (RectTransform)handleGo.transform;
            handleRect.SetParent(baseRect, false);
            handleRect.sizeDelta = new Vector2(handleRange, handleRange);
            handleGo.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 0.4f);
            stickHandle = handleRect;

            _ = parentCanvas; // rootCanvas resolved for future scaling work; keep linter quiet.
        }

        private void OnDisable()
        {
            // Never leave a stuck direction when the screen changes or the object
            // is disabled mid-touch.
            OnPointerUp(null!);
        }
    }
}
