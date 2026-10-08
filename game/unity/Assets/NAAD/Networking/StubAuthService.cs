using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    /// <summary>
    /// Gate 3 stub — succeeds without network so bootstrap flow can be verified offline.
    /// Replace with Supabase-backed implementation in Gate 4.
    /// </summary>
    public sealed class StubAuthService : IAuthService
    {
        private readonly INAADLogger _log;
        private string? _playerId;
        private string? _token;

        public StubAuthService(INAADLogger log)
        {
            _log = log;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public Task<bool> SignInAsync(string email, string password)
        {
            _log.Info("Auth", $"Stub sign-in for {email}");
            _playerId = "00000000-0000-0000-0000-000000000001";
            _token = "stub-access-token";
            return Task.FromResult(true);
        }

        public Task<bool> SignOutAsync()
        {
            _log.Info("Auth", "Stub sign-out");
            _playerId = null;
            _token = null;
            return Task.FromResult(true);
        }

        public Task<string?> GetAccessTokenAsync() => Task.FromResult(_token);

        public Task<string?> GetPlayerIdAsync() => Task.FromResult(_playerId);
    }
}
