using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.World
{
    /// <summary>
    /// Presentation layer for server-owned world time.
    /// Device clock is never authoritative — only used for local interpolation pacing.
    /// </summary>
    public sealed class WorldClockPresenter : MonoBehaviour
    {
        [SerializeField] private float pollSeconds = 15f;
        [SerializeField] private bool demoCycleOnStart;

        public WorldClockDto? Current { get; private set; }
        public string Period => Current?.Period ?? "DAY";
        public int NightlifeLevel => Current?.NightlifeLevel ?? 50;

        private float _timer;

        private async void Start()
        {
            await RefreshAsync();
            if (demoCycleOnStart)
                await DemoDayNightCycleAsync();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= pollSeconds)
            {
                _timer = 0f;
                _ = RefreshAsync();
            }
        }

        public async Task RefreshAsync()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;
            Current = await root.World.FetchWorldClockAsync();
            ApplyPresentation();
        }

        public async Task AdvanceAsync()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;
            Current = await root.World.AdvanceWorldTimeAsync();
            ApplyPresentation();
        }

        public async Task SetPeriodAsync(string period)
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;
            Current = await root.World.SetPeriodAsync(period);
            ApplyPresentation();
        }

        /// <summary>
        /// Gate 6 pass demo: DAY → SUNSET → NIGHT → LATE NIGHT
        /// </summary>
        public async Task DemoDayNightCycleAsync()
        {
            string[] steps = { "DAY", "TRANSITION", "NIGHT", "LATE_NIGHT" };
            foreach (var step in steps)
            {
                await SetPeriodAsync(step);
                await Task.Delay(500);
            }
        }

        private void ApplyPresentation()
        {
            if (Current == null || !Current.Success) return;
            var root = NAADApplicationRoot.Instance;
            root?.Logger.Info("Clock",
                $"PRESENT period={Current.Period} time={Current.GameTime} nightlife={Current.NightlifeLevel}");

            // Hook for lighting/sky: drive from Period, not device clock
            // RenderSettings / custom lighting controllers can subscribe here later.
            switch (Current.Period)
            {
                case "DAY":
                    RenderSettings.ambientIntensity = 1.0f;
                    break;
                case "TRANSITION":
                    RenderSettings.ambientIntensity = 0.7f;
                    break;
                case "NIGHT":
                    RenderSettings.ambientIntensity = 0.35f;
                    break;
                case "LATE_NIGHT":
                case "AFTER_HOURS":
                    RenderSettings.ambientIntensity = 0.2f;
                    break;
                default:
                    RenderSettings.ambientIntensity = 0.85f;
                    break;
            }
        }
    }
}
