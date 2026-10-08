using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    public sealed class StubWorldStateService : IWorldStateService
    {
        private readonly INAADLogger _log;

        public StubWorldStateService(INAADLogger log)
        {
            _log = log;
        }

        public Task<WorldStateDto?> FetchWorldStateAsync()
        {
            _log.Info("World", "Stub fetch world state");
            return Task.FromResult<WorldStateDto?>(new WorldStateDto
            {
                Id = 1,
                GameTime = System.DateTime.UtcNow.ToString("o"),
                Weather = "CLEAR",
                TrafficLevel = 50,
                NightlifeLevel = 50
            });
        }
    }
}
