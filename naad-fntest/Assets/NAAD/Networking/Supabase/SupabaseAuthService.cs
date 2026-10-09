using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;
using UnityEngine;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Real Supabase Auth (email/password). Session stored in PlayerPrefs for MVP.
    /// Never uses service_role.
    /// </summary>
    public sealed class SupabaseAuthService : IAuthService
    {
        private const string PrefAccess = "naad.auth.access_token";
        private const string PrefRefresh = "naad.auth.refresh_token";
        private const string PrefUserId = "naad.auth.user_id";

        private readonly SupabaseConfig _config;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        private string? _accessToken;
        private string? _refreshToken;
        private string? _userId;

        public SupabaseAuthService(SupabaseConfig config, INAADLogger log)
        {
            _config = config;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && !string.IsNullOrEmpty(_userId);

        public async Task<bool> SignUpAsync(string email, string password, string? displayName = null)
        {
            if (!_config.IsConfigured)
            {
                _log.Error("Auth", "SupabaseConfig missing anon key or URL");
                return false;
            }

            var meta = string.IsNullOrWhiteSpace(displayName)
                ? "{}"
                : $"{{\"display_name\":\"{Escape(displayName)}\"}}";

            var body =
                $"{{\"email\":\"{Escape(email)}\",\"password\":\"{Escape(password)}\",\"data\":{meta}}}";

            var result = await _http.SendAsync(
                $"{_config.AuthBaseUrl}/signup",
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body,
                null);

            if (!result.Ok)
            {
                _log.Error("Auth", $"SignUp failed: {result.StatusCode} {result.Body}");
                return false;
            }

            return ApplySessionJson(result.Body, "SignUp");
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            if (!_config.IsConfigured)
            {
                _log.Error("Auth", "SupabaseConfig missing anon key or URL");
                return false;
            }

            var body = $"{{\"email\":\"{Escape(email)}\",\"password\":\"{Escape(password)}\"}}";
            var result = await _http.SendAsync(
                $"{_config.AuthBaseUrl}/token?grant_type=password",
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body,
                null);

            if (!result.Ok)
            {
                _log.Error("Auth", $"SignIn failed: {result.StatusCode} {result.Body}");
                return false;
            }

            return ApplySessionJson(result.Body, "SignIn");
        }

        public async Task<bool> SignOutAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                await _http.SendAsync(
                    $"{_config.AuthBaseUrl}/logout",
                    UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                    "{}",
                    _accessToken);
            }

            ClearSession();
            _log.Info("Auth", "Signed out");
            return true;
        }

        public async Task<bool> RestoreSessionAsync()
        {
            _accessToken = PlayerPrefs.GetString(PrefAccess, "");
            _refreshToken = PlayerPrefs.GetString(PrefRefresh, "");
            _userId = PlayerPrefs.GetString(PrefUserId, "");

            if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_userId))
            {
                ClearSession();
                return false;
            }

            // Validate token with /user
            var result = await _http.SendAsync(
                $"{_config.AuthBaseUrl}/user",
                UnityEngine.Networking.UnityWebRequest.kHttpVerbGET,
                null,
                _accessToken);

            if (!result.Ok)
            {
                _log.Warn("Auth", "Stored session invalid; clearing");
                ClearSession();
                return false;
            }

            // Prefer id from response
            NaadJson.TryGetString(result.Body, "id", out var id);
            if (!string.IsNullOrEmpty(id))
                _userId = id;

            PersistSession();
            _log.Info("Auth", $"Session restored for {_userId}");
            return true;
        }

        public Task<string?> GetAccessTokenAsync() => Task.FromResult(_accessToken);

        public Task<string?> GetPlayerIdAsync() => Task.FromResult(_userId);

        private bool ApplySessionJson(string json, string context)
        {
            // signup may return user without session if email confirm required
            NaadJson.TryGetString(json, "access_token", out var access);
            NaadJson.TryGetString(json, "refresh_token", out var refresh);
            var userId = ExtractNestedId(json);

            if (string.IsNullOrEmpty(access) || string.IsNullOrEmpty(userId))
            {
                // Email confirmation may be required — user object still present
                if (!string.IsNullOrEmpty(userId))
                {
                    _log.Warn("Auth", $"{context}: user created but no session (confirm email?) user={userId}");
                    _userId = userId;
                    return false;
                }

                _log.Error("Auth", $"{context}: missing access_token or user id in response");
                return false;
            }

            _accessToken = access;
            _refreshToken = refresh;
            _userId = userId;
            PersistSession();
            _log.Info("Auth", $"{context} ok user={_userId}");
            return true;
        }

        private void PersistSession()
        {
            PlayerPrefs.SetString(PrefAccess, _accessToken ?? "");
            PlayerPrefs.SetString(PrefRefresh, _refreshToken ?? "");
            PlayerPrefs.SetString(PrefUserId, _userId ?? "");
            PlayerPrefs.Save();
        }

        private void ClearSession()
        {
            _accessToken = null;
            _refreshToken = null;
            _userId = null;
            PlayerPrefs.DeleteKey(PrefAccess);
            PlayerPrefs.DeleteKey(PrefRefresh);
            PlayerPrefs.DeleteKey(PrefUserId);
            PlayerPrefs.Save();
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string? ExtractNestedId(string json)
        {
            // try top-level "user":{"id":"..."}
            if (NaadJson.TryGetObject(json, "user", out var userObj)
                && !string.IsNullOrEmpty(userObj)
                && NaadJson.TryGetString(userObj, "id", out var nestedId)
                && !string.IsNullOrEmpty(nestedId))
                return nestedId;

            return NaadJson.TryGetString(json, "id", out var id) ? id : null;
        }
    }
}
