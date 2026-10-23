using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the Phone home screen with app launcher.
    /// Bound to <see cref="NAAD.UI.PhoneHomeController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class PhoneHomeView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_Text? _unreadBadgeText;
        [SerializeField] private Transform? _appGridParent;
        [SerializeField] private GameObject? _appButtonPrefab;
        [SerializeField] private Button? _refreshButton;

        private readonly List<GameObject> _appButtons = new();

        protected override void Awake()
        {
            base.Awake();
            _screenId = "PhoneHome";

            if (_refreshButton != null)
            {
                _refreshButton.onClick.AddListener(OnRefreshClicked);
            }
        }

        private void OnDestroy()
        {
            if (_refreshButton != null)
            {
                _refreshButton.onClick.RemoveListener(OnRefreshClicked);
            }

            foreach (var btn in _appButtons)
            {
                if (btn != null) Object.Destroy(btn);
            }
            _appButtons.Clear();
        }

        public void UpdateUnreadCount(int count)
        {
            if (_unreadBadgeText != null)
            {
                _unreadBadgeText.text = count > 0 ? count.ToString() : string.Empty;
                _unreadBadgeText.gameObject.SetActive(count > 0);
            }
        }

        public void RegisterAppButton(string appName, System.Action onTapped)
        {
            if (_appButtonPrefab == null || _appGridParent == null) return;

            var btnGo = Object.Instantiate(_appButtonPrefab, _appGridParent);
            var btn = btnGo.GetComponentInChildren<Button>(true);
            var txt = btnGo.GetComponentInChildren<TMP_Text>(true);

            if (txt != null) txt.text = appName;
            if (btn != null) btn.onClick.AddListener(() => onTapped?.Invoke());

            _appButtons.Add(btnGo);
        }

        public void ClearAppButtons()
        {
            foreach (var btn in _appButtons)
            {
                if (btn != null) Object.Destroy(btn);
            }
            _appButtons.Clear();
        }

        public event System.Action? OnRefresh;

        private void OnRefreshClicked()
        {
            OnRefresh?.Invoke();
        }
    }
}