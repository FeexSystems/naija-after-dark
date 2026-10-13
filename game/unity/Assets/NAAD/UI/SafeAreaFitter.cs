using UnityEngine;

namespace NAAD.UI
{
    /// <summary>
    /// Fits a uGUI panel inside the device safe area (notches, camera cutouts,
    /// status/navigation bars). Attach to the root panel of any full-screen UI.
    ///
    /// Recomputes on every frame the screen geometry changes, and also responds to
    /// RectTransform-driven resizes, so orientation changes and split-screen are
    /// handled. Pure math on the RectTransform — no package dependency, works with
    /// the uGUI stack this project ships (com.unity.ugui).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [Tooltip("Also apply a minimum padding so content never touches the screen edge on devices that report a full-screen safe area.")]
        [SerializeField] private Vector2 minPadding = new(16f, 24f);

        private RectTransform _panel = null!;
        private Rect _appliedSafeArea;
        private ScreenOrientation _appliedOrientation;
        private Vector2 _appliedSize;

        private void OnEnable()
        {
            _panel = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (HasGeometryChanged())
                Apply();
        }

        private bool HasGeometryChanged()
        {
            return Screen.safeArea != _appliedSafeArea
                   || Screen.orientation != _appliedOrientation
                   || _panel.rect.size != _appliedSize;
        }

        private void Apply()
        {
            var safeArea = Screen.safeArea;

            // Guard against degenerate values during device switches.
            if (safeArea.width <= 0 || safeArea.height <= 0 ||
                Screen.width <= 0 || Screen.height <= 0)
                return;

            var anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            var anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

            // Minimum padding keeps content off the edge when the device reports a
            // full-bleed safe area (many older Android devices do).
            var padLeft = Mathf.Max(minPadding.x, 0) / Screen.width;
            var padBottom = Mathf.Max(minPadding.y, 0) / Screen.height;
            var padRight = Mathf.Max(minPadding.x, 0) / Screen.width;
            var padTop = Mathf.Max(minPadding.y, 0) / Screen.height;

            _panel.anchorMin = new Vector2(
                Mathf.Min(anchorMin.x + padLeft, 0.5f),
                Mathf.Min(anchorMin.y + padBottom, 0.5f));
            _panel.anchorMax = new Vector2(
                Mathf.Max(anchorMax.x - padRight, 0.5f),
                Mathf.Max(anchorMax.y - padTop, 0.5f));

            _appliedSafeArea = safeArea;
            _appliedOrientation = Screen.orientation;
            _appliedSize = _panel.rect.size;
        }
    }
}
