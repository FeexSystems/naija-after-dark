using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the end-of-night summary screen.
    /// Bound to <see cref="NAAD.UI.NightSummaryController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class NightSummaryView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _spentText;
        [SerializeField] private TMP_Text? _peopleMetText;
        [SerializeField] private TMP_Text? _bestMomentText;
        [SerializeField] private Button? _continueButton;
        [SerializeField] private Button? _skipButton;
        [SerializeField] private Button? _retryButton;
        [SerializeField] private GameObject? _summaryPanel;

        protected override void Awake()
        {
            base.Awake();
            _screenId = "NightSummary";

            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);
            if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void OnDestroy()
        {
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinueClicked);
            if (_skipButton != null) _skipButton.onClick.RemoveListener(OnSkipClicked);
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        public void SetSummary(int spent, int peopleMet, string bestMoment)
        {
            if (_spentText != null) _spentText.text = $"₦{spent:N0}";
            if (_peopleMetText != null) _peopleMetText.text = peopleMet.ToString();
            if (_bestMomentText != null) _bestMomentText.text = bestMoment;

            _summaryPanel?.SetActive(true);
        }

        public event System.Action? OnContinue;
        public event System.Action? OnSkip;
        public event System.Action? OnRetry;

        private void OnContinueClicked() => OnContinue?.Invoke();
        private void OnSkipClicked() => OnSkip?.Invoke();
        private void OnRetryClicked() => OnRetry?.Invoke();
    }
}