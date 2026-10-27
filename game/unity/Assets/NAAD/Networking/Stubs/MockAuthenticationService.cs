using System;
using System.Threading.Tasks;
using NAAD.Gameplay;
using NAAD.Networking;
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

        public Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult(_isAuthenticated ? "mock_token" : null);
        }

        public Task<string?> GetSessionTokenAsync()
        {
            return GetAccessTokenAsync();
        }

        public Task<string?> GetPlayerIdAsync()
        {
            return Task.FromResult(_isAuthenticated ? _playerId : null);
        }

        public Task<PlayerDto?> FetchCurrentPlayerAsync()
        {
            return Task.FromResult(new PlayerDto
            {
                Id = _playerId,
                DisplayName = "Mock Player",
                Balance = 50000,
                LocationId = NightDistrictIds.Apartment,
                Career = "DEVELOPER",
                IsConsentGiven = true
            });
        }

        public Task<bool> SignUpAsync(string email, string password, string? displayName = null)
        {
            if (email == null || password == null || password.Length < 6)
            {
                return Task.FromResult(false);
            }

            _isAuthenticated = true;
            _playerId = "mock_player";
            return Task.FromResult(true);
        }

        public Task<bool> SignInAsync(string email, string password)
        {
            if (email == null || password == null || password.Length < 6)
            {
                return Task.FromResult(false);
            }

            _isAuthenticated = true;
            _playerId = "mock_player";
            return Task.FromResult(true);
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