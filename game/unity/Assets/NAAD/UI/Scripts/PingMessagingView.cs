using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the Ping messaging interface.
    /// Bound to <see cref="NAAD.UI.PingMessagingController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class PingMessagingView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _messageBodyText;
        [SerializeField] private TMP_Text? _contactNameText;
        [SerializeField] private Transform? _responseButtonParent;
        [SerializeField] private GameObject? _responseButtonPrefab;
        [SerializeField] private Button? _sendButton;
        [SerializeField] private TMP_InputField? _inputField;
        [SerializeField] private Button? _refreshButton;

        private readonly List<GameObject> _responseButtons = new();

        protected override void Awake()
        {
            base.Awake();
            _screenId = "PingMessaging";

            if (_sendButton != null) _sendButton.onClick.AddListener(OnSendClicked);
            if (_refreshButton != null) _refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        private void OnDestroy()
        {
            if (_sendButton != null) _sendButton.onClick.RemoveListener(OnSendClicked);
            if (_refreshButton != null) _refreshButton.onClick.RemoveListener(OnRefreshClicked);

            foreach (var btn in _responseButtons)
            {
                if (btn != null) Object.Destroy(btn);
            }
            _responseButtons.Clear();
        }

        public void SetMessage(string contactName, string body)
        {
            if (_contactNameText != null) _contactNameText.text = contactName;
            if (_messageBodyText != null) _messageBodyText.text = body;
        }

        public void AddResponseOption(string label, System.Action onSelected)
        {
            if (_responseButtonPrefab == null || _responseButtonParent == null) return;

            var btnGo = Object.Instantiate(_responseButtonPrefab, _responseButtonParent);
            var btn = btnGo.GetComponentInChildren<Button>(true);
            var txt = btnGo.GetComponentInChildren<TMP_Text>(true);

            if (txt != null) txt.text = label;
            if (btn != null) btn.onClick.AddListener(() =>
            {
                onSelected?.Invoke();
                OnResponseSelected?.Invoke(label);
            });

            _responseButtons.Add(btnGo);
        }

        public void ClearResponseOptions()
        {
            foreach (var btn in _responseButtons)
            {
                if (btn != null) Object.Destroy(btn);
            }
            _responseButtons.Clear();
        }

        public string GetInputText() => _inputField != null ? _inputField.text : string.Empty;

        public void ClearInput()
        {
            if (_inputField != null) _inputField.text = string.Empty;
        }

        public event System.Action<string>? OnResponseSelected;
        public event System.Action? OnSend;
        public event System.Action? OnRefresh;

        private void OnSendClicked()
        {
            OnSend?.Invoke();
        }

        private void OnRefreshClicked()
        {
            OnRefresh?.Invoke();
        }
    }
}