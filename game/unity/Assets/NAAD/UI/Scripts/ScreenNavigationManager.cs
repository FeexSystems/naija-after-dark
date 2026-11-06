using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NAAD.Core.Bootstrap;
using NAAD.UI;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// Manages screen navigation and view-controller wiring for the NAAD UI layer.
    /// <para>
    /// Screens are registered with their view and controller references. The manager
    /// handles showing/hiding views, routing UI events to controller actions, and
    /// maintaining a navigation stack.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(EventSystem))]
    [DisallowMultipleComponent]
    public sealed class ScreenNavigationManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private ScreenViewBase? _defaultScreen;

        [Header("Runtime")]
        [SerializeField]
        private List<ScreenViewBase> _history = new();

        private ScreenViewBase? _currentScreen;
        private readonly Dictionary<string, ScreenViewBase> _views = new();
        private readonly Dictionary<string, Type> _controllerTypes = new();

        public ScreenViewBase? CurrentScreen => _currentScreen;

        private void Awake()
        {
            // Ensure SafeAreaFitter is present on the canvas
            var canvas = GetComponent<Canvas>();
            if (canvas.GetComponent<SafeAreaFitter>() == null)
            {
                canvas.gameObject.AddComponent<SafeAreaFitter>();
            }
        }

        /// <summary>
        /// Stable screen key for a view: the type name without the "View" suffix.
        /// <para>
        /// Deliberately does NOT use <see cref="ScreenViewBase.ScreenId"/>: views are
        /// instantiated inactive, so their <c>Awake</c> never runs and the serialized
        /// <c>_screenId</c> is still empty when this is called. Deriving from the type
        /// name is deterministic and matches how controllers are keyed.
        /// </para>
        /// </summary>
        private static string ScreenKeyOf(ScreenViewBase view)
        {
            var name = view.GetType().Name;
            return name.EndsWith("View", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - "View".Length)
                : name;
        }

        /// <summary>
        /// Stable screen key for a controller: strip the "Controller" suffix, then the
        /// "Screen" / "UI" infixes the two older naming styles added.
        /// <c>BootstrapScreenController</c> → Bootstrap, <c>ConfigurationUIController</c> →
        /// Configuration, <c>MainHudController</c> → MainHud.
        /// </summary>
        private static string ScreenKeyOfController(BaseScreenController controller)
        {
            var name = controller.GetType().Name;

            if (name.EndsWith("Controller", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Controller".Length);
            if (name.EndsWith("Screen", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Screen".Length);
            if (name.EndsWith("UI", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "UI".Length);

            return name;
        }

        /// <summary>
        /// Register every <see cref="ScreenViewBase"/> under this canvas.
        /// <para>
        /// Nothing ever called <see cref="RegisterView"/>, so <c>_views</c> stayed empty and
        /// <see cref="NavigateTo"/> could only ever log "View not registered".
        /// </para>
        /// </summary>
        public int RegisterChildViews()
        {
            var registered = 0;
            foreach (var view in GetComponentsInChildren<ScreenViewBase>(includeInactive: true))
            {
                if (view == null) continue;

                var key = ScreenKeyOf(view);
                if (string.IsNullOrEmpty(key) || _views.ContainsKey(key)) continue;

                _views[key] = view;
                view.gameObject.SetActive(false);
                registered++;
            }

            return registered;
        }

        /// <summary>
        /// Register a view with the navigation manager.
        /// </summary>
        /// <param name="view">The view instance to register.</param>
        public void RegisterView(ScreenViewBase view)
        {
            if (view == null || string.IsNullOrEmpty(view.ScreenId)) return;

            if (_views.ContainsKey(view.ScreenId))
            {
                Debug.LogWarning($"[ScreenNavigationManager] View '{view.ScreenId}' already registered.");
                return;
            }

            _views[view.ScreenId] = view;
            view.gameObject.SetActive(false);
        }

        /// <summary>
        /// Navigate to a screen by ID.
        /// </summary>
        /// <param name="screenId">The screen ID to navigate to.</param>
        /// <param name="addToHistory">Whether to push the current screen onto the history stack.</param>
        public void NavigateTo(string screenId, bool addToHistory = true)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogError($"[ScreenNavigationManager] View '{screenId}' not registered.");
                return;
            }

            if (addToHistory && _currentScreen != null)
            {
                _history.Add(_currentScreen);
            }

            _currentScreen?.Hide();
            _currentScreen = view;
            _currentScreen.Show();
        }

        /// <summary>
        /// Navigate back to the previous screen in history.
        /// </summary>
        public bool NavigateBack()
        {
            if (_history.Count == 0) return false;

            _currentScreen?.Hide();
            var previous = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            _currentScreen = previous;
            _currentScreen.Show();
            return true;
        }

        /// <summary>
        /// Get a registered view by screen ID.
        /// </summary>
        public T? GetView<T>(string screenId) where T : ScreenViewBase
        {
            if (_views.TryGetValue(screenId, out var view))
            {
                return view as T;
            }
            return null;
        }

        /// <summary>
        /// Wire a controller's events to a view's events by screen ID.
        /// </summary>
        /// <typeparam name="TController">The controller type.</typeparam>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <param name="screenId">The screen ID.</param>
        /// <param name="wire">Action to perform the wiring.</param>
        public void WireController<TController, TView>(string screenId, Action<TController, TView> wire)
            where TController : MonoBehaviour
            where TView : ScreenViewBase
        {
            if (!_views.TryGetValue(screenId, out var view)) return;
            var typedView = view as TView;
            if (typedView == null) return;

            // FindAnyObjectByType does not depend on instance-ID ordering, unlike the
            // deprecated FindObjectOfType it replaces.
            var controller = FindAnyObjectByType<TController>();
            if (controller == null)
            {
                Debug.LogWarning($"[ScreenNavigationManager] Controller '{typeof(TController).Name}' not found for screen '{screenId}'.");
                return;
            }

            wire(controller, typedView);
        }

        /// <summary>
        /// Bind every <see cref="BaseScreenController"/> in the scene to the registered view
        /// whose screen id matches the controller's type name (strip the "Controller" suffix:
        /// <c>MainHudController</c> → screen id <c>MainHud</c>).
        /// <para>
        /// This is the glue that did not exist. The Views (<c>NAAD.UI.Scripts.*View</c>) were
        /// implemented and the Controllers (<c>NAAD.UI.*Controller</c>) held the data, but
        /// nothing connected them — every render method was an empty comment. Bind once at
        /// startup rather than per navigation; the controller keeps its view for its lifetime.
        /// </para>
        /// </summary>
        /// <param name="logWhenMissing">Warn for controllers that have no matching view.</param>
        /// <returns>The number of controllers bound.</returns>
        public int RegisterControllersAuto(bool logWhenMissing = true)
        {
            var controllers = FindObjectsByType<BaseScreenController>(FindObjectsInactive.Include);
            var bound = 0;

            foreach (var controller in controllers)
            {
                if (controller == null) continue;

                var screenId = ScreenKeyOfController(controller);

                if (_views.TryGetValue(screenId, out var view))
                {
                    controller.BindView(view);
                    bound++;
                }
                else if (logWhenMissing)
                {
                    Debug.LogWarning(
                        $"[ScreenNavigationManager] No registered view for controller " +
                        $"'{controller.GetType().Name}' (expected screen id '{screenId}').");
                }
            }

            return bound;
        }

        private void Start()
        {
            RegisterChildViews();

            // Bind before the first navigation so the default screen renders with data.
            RegisterControllersAuto();

            if (_defaultScreen == null)
            {
                // Deterministic fallback: start-up screen, then sign-in, then anything.
                foreach (var preferred in new[] { "Bootstrap", "Auth" })
                {
                    if (_views.TryGetValue(preferred, out var view))
                    {
                        _defaultScreen = view;
                        break;
                    }
                }

                if (_defaultScreen == null)
                {
                    foreach (var view in _views.Values)
                    {
                        _defaultScreen = view;
                        break;
                    }
                }
            }

            if (_defaultScreen != null)
            {
                NavigateTo(ScreenKeyOf(_defaultScreen), addToHistory: false);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_defaultScreen != null && string.IsNullOrEmpty(_defaultScreen.ScreenId))
            {
                Debug.LogWarning("[ScreenNavigationManager] Default screen has no ScreenId set.");
            }
        }
#endif
    }
}
