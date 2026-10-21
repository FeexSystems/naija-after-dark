using System;
using System.Collections.Generic;
using UnityEngine;
using NAAD.Core.Bootstrap;

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
            where TController : class
            where TView : ScreenViewBase
        {
            if (!_views.TryGetValue(screenId, out var view)) return;
            var typedView = view as TView;
            if (typedView == null) return;

            // The controller instance is found via FindObjectOfType or passed externally
            var controller = FindObjectOfType<TController>();
            if (controller == null)
            {
                Debug.LogWarning($"[ScreenNavigationManager] Controller '{typeof(TController).Name}' not found for screen '{screenId}'.");
                return;
            }

            wire(controller, typedView);
        }

        private void Start()
        {
            if (_defaultScreen != null)
            {
                NavigateTo(_defaultScreen.ScreenId, addToHistory: false);
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