using System;
using System.Collections.Generic;
using UnityEngine;

namespace NAAD.Gameplay.World
{
    /// <summary>
    /// Tracks the authoritative world clock, replacing the stub world state with real
    /// game time. Used by the runtime to advance time, set periods, and query world state.
    /// </summary>
    public interface IWorldClockService
    {
        void AdvanceTime(float minutes);
        void SetPeriod(string period);
        void SetGameTime(string date, string time);
        int GetMinutesSinceMidnight();
        string GetPeriod();
        string GetGameDate();
        string GetGameTime();
    }

    /// <summary>
    /// Default implementation of <see cref="IWorldClockService"/>.
    /// In production, replaced by a Supabase Edge Function.
    /// </summary>
    public sealed class WorldClockService : IWorldClockService
    {
        private const int MinutesPerDay = 24 * 60;

        private int _minutesSinceMidnight;

        public WorldClockService()
        {
            _minutesSinceMidnight = 6 * 60 + 30; // 06:30 - Morning startup
        }

        public void AdvanceTime(float minutes)
        {
            _minutesSinceMidnight = (_minutesSinceMidnight + (int)minutes) % MinutesPerDay;
        }

        public void SetPeriod(string period)
        {
            // Validate period (in production, would write to server / RPC)
            var valid = new[] { "MORNING", "DAY", "TRANSITION", "NIGHT", "LATE_NIGHT", "AFTER_HOURS" };
            if (Array.IndexOf(valid, period) < 0)
                throw new ArgumentException($"Invalid period: {period}. Valid: " + string.Join(", ", valid));
        }

        public void SetGameTime(string date, string time)
        {
            // In production, would be set by the server RPC.
            _minutesSinceMidnight = ParseMinutes(time);
        }

        public int GetMinutesSinceMidnight() => _minutesSinceMidnight;

        public string GetPeriod()
        {
            var m = _minutesSinceMidnight;
            if (m >= 360 && m < 540) return "MORNING";
            if (m >= 540 && m < 960) return "DAY";
            if (m >= 960 && m < 1140) return "TRANSITION";
            if (m >= 1140) return "NIGHT";
            if (m < 180) return "LATE_NIGHT";
            return "AFTER_HOURS";
        }

        public string GetGameDate() => DateTime.UtcNow.ToString("yyyy-MM-dd");

        public string GetGameTime() => DateTime.UtcNow.ToString("o");

        private static int ParseMinutes(string time)
        {
            if (string.IsNullOrEmpty(time)) return 0;
            var parts = time.Split(':');
            if (parts.Length < 2) return 0;
            if (int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m))
                return h * 60 + m;
            return 0;
        }
    }
}
