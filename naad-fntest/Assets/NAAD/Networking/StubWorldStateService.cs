using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    public sealed class StubWorldStateService : IWorldStateService
    {
        private readonly INAADLogger _log;
        private string _period = "NIGHT";
        private int _minutes = 21 * 60;

        public StubWorldStateService(INAADLogger log)
        {
            _log = log;
        }

        public Task<WorldStateDto?> FetchWorldStateAsync()
        {
            return Task.FromResult<WorldStateDto?>(new WorldStateDto
            {
                Id = 1,
                GameTime = System.DateTime.UtcNow.ToString("o"),
                Weather = "CLEAR",
                TrafficLevel = 50,
                NightlifeLevel = 80,
                TimeScale = 60
            });
        }

        public Task<WorldClockDto?> FetchWorldClockAsync() => Task.FromResult<WorldClockDto?>(Clock());

        public Task<WorldClockDto?> AdvanceWorldTimeAsync()
        {
            _minutes = (_minutes + 30) % (24 * 60);
            _period = PeriodFromMinutes(_minutes);
            _log.Info("World", $"Stub advance → {_period}");
            return Task.FromResult<WorldClockDto?>(Clock());
        }

        public Task<WorldClockDto?> SetPeriodAsync(string period)
        {
            _period = period.ToUpperInvariant();
            _minutes = _period switch
            {
                "MORNING" => 6 * 60 + 30,
                "DAY" => 12 * 60,
                "TRANSITION" or "SUNSET" => 17 * 60 + 30,
                "NIGHT" => 21 * 60,
                "LATE_NIGHT" => 60,
                "AFTER_HOURS" => 4 * 60,
                _ => _minutes
            };
            _log.Info("World", $"Stub set period → {_period}");
            return Task.FromResult<WorldClockDto?>(Clock());
        }

        private WorldClockDto Clock() => new()
        {
            Success = true,
            GameDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
            GameTime = System.DateTime.UtcNow.Date.AddMinutes(_minutes).ToString("o"),
            MinutesSinceMidnight = _minutes,
            Period = _period,
            TimeScale = 60,
            Weather = "CLEAR",
            TrafficLevel = 50,
            NightlifeLevel = _period switch
            {
                "NIGHT" => 80,
                "LATE_NIGHT" => 90,
                "TRANSITION" => 55,
                "DAY" => 25,
                _ => 50
            }
        };

        private static string PeriodFromMinutes(int m)
        {
            if (m >= 360 && m < 540) return "MORNING";
            if (m >= 540 && m < 960) return "DAY";
            if (m >= 960 && m < 1140) return "TRANSITION";
            if (m >= 1140) return "NIGHT";
            if (m < 180) return "LATE_NIGHT";
            return "AFTER_HOURS";
        }
    }
}
