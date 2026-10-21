using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the Bootstrap/loading screen.
    /// Bound to <see cref="NAAD.UI.BootstrapScreenController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class BootstrapView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _statusText;
        [SerializeField] private TMP_Text? _errorText;
        [SerializeField] private Button? _retryButton;
        [SerializeField] private GameObject? _loadingPanel;
        [SerializeField] private GameObject? _contentPanel;
        [SerializeField] private GameObject? _errorPanel;

        private Button _retryBtn => _retryButton ??= GetComponentInChildren<Button>(true);

        protected override void Awake()
        {
            base.Awake();
            _screenId = "Bootstrap";

            if (_retryBtn != null)
            {
                _retryBtn.onClick.AddListener(OnRetryClicked);
            }
        }

        private void OnDestroy()
        {
            if (_retryBtn != null)
            {
                _retryBtn.onClick.RemoveListener(OnRetryClicked);
            }
        }

        /// <summary>Set the status message displayed on screen.</summary>
        public void SetStatus(string message)
        {
            if (_statusText != null) _statusText.text = message;
        }

        /// <summary>Show an error message.</summary>
        public void ShowError(string message)
        {
            if (_errorText != null) _errorText.text = message;
            _errorPanel?.SetActive(true);
            _contentPanel?.SetActive(false);
            _loadingPanel?.SetActive(false);
        }

        /// <summary>Show the loading state.</summary>
        public void ShowLoading()
        {
            _errorPanel?.SetActive(false);
            _contentPanel?.SetActive(false);
            _loadingPanel?.SetActive(true);
        }

        /// <summary>Show the main content state.</summary>
        public void ShowContent()
        {
            _errorPanel?.SetActive(false);
            _loadingPanel?.SetActive(false);
            _contentPanel?.SetActive(true);
        }

        public override void SetState(ScreenState state)
        {
            base.SetState(state);

            switch (state)
            {
                case ScreenState.Loading:
                    ShowLoading();
                    break;
                case ScreenState.Error:
                    ShowError("An error occurred. Tap Retry to continue.");
                    break;
                case ScreenState.Success:
                case ScreenState.Empty:
                    ShowContent();
                    break;
                case ScreenState.Retry:
                    ShowError("Previous attempt failed. Tap Retry.");
                    break;
            }
        }

        private void OnRetryClicked()
        {
            // The controller binds to this via the navigation manager
            // or directly through the view's event
            OnRetry?.Invoke();
        }

        /// <summary>Fired when the user taps Retry.</summary>
        public event System.Action? OnRetry;
    }
}