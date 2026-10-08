using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    public sealed class StubPlayerService : IPlayerService
    {
        private readonly IAuthService _auth;
        private readonly INAADLogger _log;

        public StubPlayerService(IAuthService auth, INAADLogger log)
        {
            _auth = auth;
            _log = log;
        }

        public async Task<PlayerDto?> FetchCurrentPlayerAsync()
        {
            var id = await _auth.GetPlayerIdAsync();
            if (string.IsNullOrEmpty(id))
            {
                _log.Warn("Player", "No authenticated player id");
                return null;
            }

            _log.Info("Player", $"Stub load player {id}");
            return new PlayerDto
            {
                Id = id,
                DisplayName = "Stub Player",
                CreatedAt = System.DateTime.UtcNow.ToString("o"),
                UpdatedAt = System.DateTime.UtcNow.ToString("o")
            };
        }
    }
}
