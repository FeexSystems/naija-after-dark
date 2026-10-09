using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.Logging;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.UI
{
    /// <summary>
    /// UI → AuthController → IAuthService.
    /// UI never calls Supabase or the database directly.
    /// </summary>
    public sealed class AuthController : MonoBehaviour
    {
        private IAuthService? _auth;
        private INAADLogger? _log;

        private void Awake()
        {
            Bind();
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
