using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for the bottom navigation bar (Home, Social, City, Life, Profile).
    /// Bound to <see cref="NAAD.UI.BottomNavigationController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class BottomNavigationView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private Transform? _tabButtonParent;
        [SerializeField] private GameObject? _tabButtonPrefab;
        [SerializeField] private Transform? _badgeParent;
        [SerializeField] private TMP_Text? _badgeText;

        private readonly List<GameObject> _tabButtons = new();
        private int _activeTabIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            _screenId = "BottomNavigation";
        }

        private void OnDestroy()
        {
            foreach (var btn in _tabButtons)
            {
                if (btn != null) Object.Destroy(btn);
            }
            _tabButtons.Clear();
        }

        public void RegisterTab(int index, string label, System.Action onTapped)
        {
            if (_tabButtonPrefab == null || _tabButtonParent == null) return;

            var btnGo = Object.Instantiate(_tabButtonPrefab, _tabButtonParent);
            var btn = btnGo.GetComponentInChildren<Button>(true);
            var txt = btnGo.GetComponentInChildren<TMP_Text>(true);

            if (txt != null) txt.text = label;
            if (btn != null)
            {
                var capturedIndex = index;
                btn.onClick.AddListener(() =>
                {
                    SetActiveTab(capturedIndex);
                    onTapped?.Invoke();
                });
            }

            _tabButtons.Add(btnGo);
            UpdateTabVisuals(index, index == _activeTabIndex);
        }

        public void SetActiveTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Count) return;
            _activeTabIndex = index;

            for (var i = 0; i < _tabButtons.Count; i++)
            {
                UpdateTabVisuals(i, i == index);
            }
        }

        public int GetActiveTab() => _activeTabIndex;

        public void UpdateBadgeCount(int count)
        {
            if (_badgeText != null)
            {
                _badgeText.text = count > 0 ? count.ToString() : string.Empty;
                _badgeParent?.gameObject.SetActive(count > 0);
            }
        }

        private void UpdateTabVisuals(int index, bool isActive)
        {
            var btnGo = _tabButtons[index];
            var img = btnGo.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? new Color(0.2f, 0.7f, 1f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        public event System.Action<int>? OnTabSwitched;

        public void NotifyTabSwitched(int index)
        {
            OnTabSwitched?.Invoke(index);
        }
    }
}