using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the Main in-game HUD.
    /// Bound to <see cref="NAAD.UI.MainHudController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class MainHudView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _locationText;
        [SerializeField] private TMP_Text? _balanceText;
        [SerializeField] private Button? _interactionButton;
        [SerializeField] private GameObject? _interactionPromptPanel;
        [SerializeField] private TMP_Text? _interactionPromptText;

        protected override void Awake()
        {
            base.Awake();
            _screenId = "MainHud";

            if (_interactionButton != null)
            {
                _interactionButton.onClick.AddListener(OnInteractionClicked);
            }

            _interactionPromptPanel?.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_interactionButton != null)
            {
                _interactionButton.onClick.RemoveListener(OnInteractionClicked);
            }
        }

        public void UpdateLocation(string locationName)
        {
            if (_locationText != null) _locationText.text = locationName;
        }

        public void UpdateBalance(int balance)
        {
            if (_balanceText != null) _balanceText.text = $"₦{balance:N0}";
        }

        public void ShowInteractionPrompt(string promptText)
        {
            if (_interactionPromptText != null) _interactionPromptText.text = promptText;
            _interactionPromptPanel?.SetActive(true);
        }

        public void HideInteractionPrompt()
        {
            _interactionPromptPanel?.SetActive(false);
        }

        public event System.Action? OnInteractionTapped;

        private void OnInteractionClicked()
        {
            OnInteractionTapped?.Invoke();
        }
    }
}