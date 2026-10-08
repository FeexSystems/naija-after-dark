using System.Threading.Tasks;

namespace NAAD.Networking
{
    /// <summary>
    /// Reads authoritative world state from the server.
    /// Client never invents game time or weather.
    /// </summary>
    public interface IWorldStateService
    {
        Task<WorldStateDto?> FetchWorldStateAsync();
    }

    /// <summary>
    /// Minimal DTO matching world-model WorldState (camelCase JSON).
    /// </summary>
    public sealed class WorldStateDto
    {
        public int Id { get; set; }
        public string GameTime { get; set; } = string.Empty;
        public string Weather { get; set; } = "CLEAR";
        public int TrafficLevel { get; set; }
        public int NightlifeLevel { get; set; }
    }
}
