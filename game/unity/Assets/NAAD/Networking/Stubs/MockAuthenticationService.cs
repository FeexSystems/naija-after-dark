using System;
using System.Threading.Tasks;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Mock
{
    /// <summary>
    /// Mock implementation of <see cref="IAuthService"/> for automated testing.
    /// <para>
    /// Simulates authentication state transitions (sign up, sign in, sign out,
    /// restore session) in-memory with deterministic results. No network calls.
    /// </para>
    /// </summary>
    public sealed class MockAuthenticationService : IAuthService
    {
        public bool IsAuthenticated => _isAuthenticated;
        public string PlayerId => _playerId;

        private bool _isAuthenticated = false;
        private string _playerId = "mock_player";

        public Task<string?> GetSessionTokenAsync()
        {
            return Task.FromResult(_isAuthenticated ? "mock_token" : null);
        }

        public Task<PlayerDto?> FetchCurrentPlayerAsync()
        {
            return Task.FromResult(new PlayerDto
            {
                PlayerId = _playerId,
                DisplayName = "Mock Player",
                Balance = 50000,
                LocationId = NightDistrictIds.Apartment,
                Career = "DEVELOPER",
                IsConsentGiven = true
            });
        }

        public Task<string?> SignUpAsync(string email, string password)
        {
            if (email == null || password == null || password.Length < 6)
            {
                return Task.FromResult<string?>(null);
            }

            _isAuthenticated = true;
            _playerId = "mock_player";
            return Task.FromResult<string?>("mock_token");
        }

        public Task<string?> SignInAsync(string email, string password)
        {
            if (email == null || password == null || password.Length < 6)
            {
                return Task.FromResult<string?>(null);
            }

            _isAuthenticated = true;
            _playerId = "mock_player";
            return Task.FromResult<string?>("mock_token");
        }

        public Task<bool> SignOutAsync()
        {
            _isAuthenticated = false;
            _playerId = string.Empty;
            return Task.FromResult(true);
        }

        public Task<bool> RestoreSessionAsync()
        {
            _isAuthenticated = true;
            _playerId = "mock_player";
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAccountAsync(string requestId)
        {
            _isAuthenticated = false;
            _playerId = string.Empty;
            return Task.FromResult(true);
        }

        public void Reset()
        {
            _isAuthenticated = false;
            _playerId = "mock_player";
        }
    }
}