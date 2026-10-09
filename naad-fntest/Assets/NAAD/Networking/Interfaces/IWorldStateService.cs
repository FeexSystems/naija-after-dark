using System.Threading.Tasks;

namespace NAAD.Networking
{
    /// <summary>
    /// Reads authoritative world clock from the server.
    /// Client never invents game time.
    /// </summary>
    public interface IWorldStateService
    {
        Task<WorldStateDto?> FetchWorldStateAsync();
        Task<WorldClockDto?> FetchWorldClockAsync();
        Task<WorldClockDto?> AdvanceWorldTimeAsync();
        Task<WorldClockDto?> SetPeriodAsync(string period);
    }

    public sealed class WorldStateDto
    {
        public int Id { get; set; }
        public string GameTime { get; set; } = string.Empty;
        public string Weather { get; set; } = "CLEAR";
        public int TrafficLevel { get; set; }
        public int NightlifeLevel { get; set; }
        public float TimeScale { get; set; } = 60f;
    }

    public sealed class WorldClockDto
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string GameDate { get; set; } = string.Empty;
        public string GameTime { get; set; } = string.Empty;
        public int MinutesSinceMidnight { get; set; }
        public string Period { get; set; } = "DAY";
        public float TimeScale { get; set; } = 60f;
        public string Weather { get; set; } = "CLEAR";
        public int TrafficLevel { get; set; }
        public int NightlifeLevel { get; set; }
    }
}
