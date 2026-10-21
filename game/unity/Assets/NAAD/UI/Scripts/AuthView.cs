using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// View for authentication (sign up / sign in / restore session).
    /// Bound to <see cref="NAAD.UI.AuthController"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class AuthView : ScreenViewBase
    {
        [Header("References")]
        [SerializeField] private TMP_InputField? _emailInput;
        [SerializeField] private TMP_InputField? _passwordInput;
        [SerializeField] private Button? _signUpButton;
        [SerializeField] private Button? _signInButton;
        [SerializeField] private Button? _restoreButton;
        [SerializeField] private TMP_Text? _errorText;
        [SerializeField] private GameObject? _errorPanel;
        [SerializeField] private GameObject? _authPanel;

        protected override void Awake()
        {
            base.Awake();
            _screenId = "Auth";

            if (_signUpButton != null) _signUpButton.onClick.AddListener(OnSignUpClicked);
            if (_signInButton != null) _signInButton.onClick.AddListener(OnSignInClicked);
            if (_restoreButton != null) _restoreButton.onClick.AddListener(OnRestoreClicked);
        }

        private void OnDestroy()
        {
            if (_signUpButton != null) _signUpButton.onClick.RemoveListener(OnSignUpClicked);
            if (_signInButton != null) _signInButton.onClick.RemoveListener(OnSignInClicked);
            if (_restoreButton != null) _restoreButton.onClick.RemoveListener(OnRestoreClicked);
        }

        public string GetEmail() => _emailInput != null ? _emailInput.text : string.Empty;
        public string GetPassword() => _passwordInput != null ? _passwordInput.text : string.Empty;

        public void ShowError(string message)
        {
            if (_errorText != null) _errorText.text = message;
            _errorPanel?.SetActive(true);
        }

        public void HideError()
        {
            _errorPanel?.SetActive(false);
        }

        public void ShowAuthPanel() => _authPanel?.SetActive(true);
        public void HideAuthPanel() => _authPanel?.SetActive(false);

        public event System.Action<string, string>? OnSignUp;
        public event System.Action<string, string>? OnSignIn;
        public event System.Action? OnRestoreSession;

        private void OnSignUpClicked()
        {
            HideError();
            OnSignUp?.Invoke(GetEmail(), GetPassword());
        }

        private void OnSignInClicked()
        {
            HideError();
            OnSignIn?.Invoke(GetEmail(), GetPassword());
        }

        private void OnRestoreClicked()
        {
            HideError();
            OnRestoreSession?.Invoke();
        }
    }
}