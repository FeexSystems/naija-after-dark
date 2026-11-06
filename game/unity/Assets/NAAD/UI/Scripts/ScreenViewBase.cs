using UnityEngine;
using UnityEngine.UI;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// Base class for all NAAD UI screen views.
    /// <para>
    /// Views are dumb presentation layers: they expose data-binding methods and
    /// UI events, but never mutate authoritative game state. State mutations flow
    /// through <c>ICommandService</sub> → <c>naad_execute_command</c>.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenViewBase : MonoBehaviour
    {
        [Header("View Configuration")]
        [SerializeField]
        [Tooltip("Unique screen identifier for navigation.")]
        protected string _screenId = string.Empty;

        [SerializeField]
        [Tooltip("Whether this view blocks input to views beneath it.")]
        private bool _blocksInput = true;

        private CanvasGroup? _canvasGroup;

        /// <summary>
        /// The view's CanvasGroup, resolved on demand.
        /// <para>
        /// This must be lazy. Views are instantiated inactive so that
        /// <see cref="ScreenNavigationManager"/> can register them, which means
        /// <c>Awake</c> has not run yet — and <see cref="Hide"/> is called during the very
        /// first navigation. Assigning only in <c>Awake</c> left this null and threw a
        /// NullReferenceException on the first <c>NavigateTo</c>.
        /// </para>
        /// </summary>
        protected CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        /// <summary>Unique screen identifier.</summary>
        public string ScreenId => _screenId;

        /// <summary>Whether this view blocks input to views beneath it.</summary>
        public bool BlocksInput => _blocksInput;

        protected virtual void Awake()
        {
            // Touch the lazy property so the group exists (and is added if missing) as
            // soon as the object becomes active.
            _ = CanvasGroup;
        }

        /// <summary>Show the view with optional fade-in.</summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = _blocksInput;
            CanvasGroup.blocksRaycasts = _blocksInput;
        }

        /// <summary>Hide the view.</summary>
        public virtual void Hide()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the view state. Derived classes override to show/hide
        /// loading, empty, success, error, and retry panels accordingly.
        /// </summary>
        /// <param name="state">The screen state to display.</param>
        public virtual void SetState(ScreenState state)
        {
            // Base implementation: show/hide based on state
            var isLoading = state == ScreenState.Loading;
            var hasContent = state == ScreenState.Success || state == ScreenState.Empty;

            CanvasGroup.interactable = !isLoading;
            CanvasGroup.blocksRaycasts = !isLoading;
        }
    }
}
