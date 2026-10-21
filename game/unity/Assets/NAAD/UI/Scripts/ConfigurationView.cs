using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for configuration and error display.
    /// Bound to <see cref="NAAD.UI.ConfigurationUIController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ConfigurationView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _statusText;
        [SerializeField] private TMP_Text? _errorText;
        [SerializeField] private Button? _refreshButton;
        [SerializeField] private Button? _retryButton;
        [SerializeField] private GameObject? _configPanel;
        [SerializeField] private GameObject? _errorPanel;

        protected override void Awake()
        {
            base.Awake();
            _screenId = "Configuration";

            if (_refreshButton != null) _refreshButton.onClick.AddListener(OnRefreshClicked);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void OnDestroy()
        {
            if (_refreshButton != null) _refreshButton.onClick.RemoveListener(OnRefreshClicked);
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        public void SetStatus(string status)
        {
            if (_statusText != null) _statusText.text = status;
        }

        public void ShowConfigurationError(string errorMessage)
        {
            if (_errorText != null) _errorText.text = errorMessage;
            _errorPanel?.SetActive(true);
            _configPanel?.SetActive(false);
        }

        public void ShowConfiguration()
        {
            _errorPanel?.SetActive(false);
            _configPanel?.SetActive(true);
        }

        public override void SetState(ScreenState state)
        {
            base.SetState(state);

            switch (state)
            {
                case ScreenState.Loading:
                    _configPanel?.SetActive(false);
                    _errorPanel?.SetActive(false);
                    break;
                case ScreenState.Error:
                    ShowConfigurationError("Configuration error. Tap Retry.");
                    break;
                case ScreenState.Success:
                case ScreenState.Empty:
                    ShowConfiguration();
                    break;
                case ScreenState.Retry:
                    ShowConfigurationError("Previous attempt failed. Tap Retry.");
                    break;
            }
        }

        public event System.Action? OnRefresh;
        public event System.Action? OnRetry;

        private void OnRefreshClicked() => OnRefresh?.Invoke();
        private void OnRetryClicked() => OnRetry?.Invoke();
    }
}