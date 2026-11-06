using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.Logging;
using NAAD.Networking;
using NAAD.UI.Scripts;
using UnityEngine;

namespace NAAD.UI
{
    /// <summary>
    /// UI → AuthController → IAuthService.
    /// UI never calls Supabase or the database directly.
    /// </summary>
    public sealed class AuthController : MonoBehaviour
    {
        [SerializeField] private AuthView? _view;

        private IAuthService? _auth;
        private INAADLogger? _log;

        private void Awake()
        {
            Bind();
        }

        private void Start()
        {
            // This controller is not a BaseScreenController, so RegisterControllersAuto
            // does not reach it. Resolve the view directly so a login screen works
            // without any prefab wiring.
            if (_view == null)
                _view = FindAnyObjectByType<AuthView>(FindObjectsInactive.Include);

            if (_view == null) return;

            _view.OnSignIn += HandleSignIn;
            _view.OnSignUp += HandleSignUp;

            if (_view.ScreenId.Length == 0)
                _log?.Warn("UI.Auth", "AuthView has an empty screen id; navigation will not find it.");
        }

        private void OnDestroy()
        {
            if (_view == null) return;
            _view.OnSignIn -= HandleSignIn;
            _view.OnSignUp -= HandleSignUp;
        }

        private async void HandleSignIn(string email, string password)
        {
            _view?.HideError();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _view?.ShowError("Enter both an email and a password.");
                return;
            }

            var ok = await SignInAsync(email, password);
            if (ok)
            {
                _view?.HideAuthPanel();
            }
            else
            {
                _view?.ShowError(
                    "Sign-in failed. Check the email and password, and that the account is " +
                    "confirmed in Supabase (Authentication → Users → Auto Confirm).");
            }
        }

        private async void HandleSignUp(string email, string password)
        {
            _view?.HideError();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _view?.ShowError("Enter both an email and a password.");
                return;
            }

            var ok = await SignUpAsync(email, password, displayName: string.Empty);
            if (ok)
            {
                _view?.HideAuthPanel();
            }
            else
            {
                _view?.ShowError(
                    "Sign-up failed. If the project requires email confirmation, confirm the " +
                    "address before signing in, or turn confirmation off for development.");
            }
        }

        private void Bind()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;
            _auth = root.Auth;
            _log = root.Logger;
        }

        public async Task<bool> SignUpAsync(string email, string password, string displayName)
        {
            Bind();
            if (_auth == null) return false;
            _log?.Info("UI.Auth", "SignUp requested");
            return await _auth.SignUpAsync(email, password, displayName);
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            Bind();
            if (_auth == null) return false;
            _log?.Info("UI.Auth", "SignIn requested");
            return await _auth.SignInAsync(email, password);
        }

        public async Task<bool> SignOutAsync()
        {
            Bind();
            if (_auth == null) return false;
            _log?.Info("UI.Auth", "SignOut requested");
            return await _auth.SignOutAsync();
        }

        public async Task<bool> RestoreSessionAsync()
        {
            Bind();
            if (_auth == null) return false;
            return await _auth.RestoreSessionAsync();
        }
    }
}
