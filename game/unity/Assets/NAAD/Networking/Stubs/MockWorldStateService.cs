using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Gameplay;
using NAAD.Networking;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Mock
{
    /// <summary>
    /// Mock implementation of <see cref="IWorldStateService"/> for automated testing.
    /// <para>
    /// Tracks world state (pending travels, relationships, memories, world period,
    /// opportunities, events) in-memory. All changes are deterministic and
    /// reset via <see cref="Reset"/>.
    /// </para>
    /// </summary>
    public sealed class MockWorldStateService : IWorldStateService
    {
        private readonly List<PendingTravelCommand> _pendingTravels = new();
        private readonly List<Relationship> _relationships = new();
        private readonly List<NpcMemory> _memories = new();
        private int _worldPeriod = 0; // 0 = AFTER_HOURS, 1 = NIGHT
        private readonly List<Opportunity> _opportunities = new();
        private readonly List<WorldEvent> _events = new();

        public List<PendingTravelCommand> PendingTravels => _pendingTravels;
        public List<Relationship> Relationships => _relationships;
        public List<NpcMemory> Memories => _memories;
        public int WorldPeriod => _worldPeriod;
        public List<Opportunity> Opportunities => _opportunities;
        public List<WorldEvent> Events => _events;

        public MockWorldStateService()
        {
            // Seed relationships matching initial player relationships
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Tunde, Trust = 50, Respect = 40, Affection = 30, Loyalty = 20, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.MamaSeyi, Trust = 70, Respect = 60, Affection = 80, Loyalty = 90, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Amaka, Trust = 30, Respect = 40, Affection = 20, Loyalty = 10, Conflict = 5 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Emeka, Trust = 80, Respect = 70, Affection = 60, Loyalty = 80, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Dami, Trust = 40, Respect = 50, Affection = 30, Loyalty = 30, Conflict = 0 });

            // Seed opportunities
            _opportunities.Add(new Opportunity { Id = "opp_free_dj_show", Title = "Free DJ Show", Type = "TUNDE_INVITE", TrustThreshold = 55 });
            _opportunities.Add(new Opportunity { Id = "opp_android_job", Title = "Android Developer Job", Type = "EMPLOYMENT", TrustThreshold = 70 });

            // Seed events
            _events.Add(new WorldEvent { Id = "evt_beach_party", Name = "Beach Party", Period = "NIGHT", Attendance = 0 });
            _events.Add(new WorldEvent { Id = "evt_club_night", Name = "Club Night", Period = "NIGHT", Attendance = 0 });
        }

        public Task<bool> TravelAsync(string fromLocation, string toLocation, string requestId)
        {
            _pendingTravels.Add(new PendingTravelCommand
            {
                RequestId = requestId,
                ToLocationId = toLocation,
                Coordinates = new Vector3(0f, 0f, 0f)
            });
            return Task.FromResult(true);
        }

        public Task<bool> ApplyRelationshipDelta(string npcId, int trust)
        {
            var npc = _relationships.Find(r => r.NpcId == npcId);
            if (npc == null) return Task.FromResult(false);

            npc.Trust = Mathf.Clamp(npc.Trust + trust, 0, 100);
            return Task.FromResult(true);
        }

        public Task<bool> CommitMemory(string npcId, string memoryType, string summary)
        {
            _memories.Add(new NpcMemory
            {
                NpcId = npcId,
                MemoryType = memoryType,
                Summary = summary,
                Timestamp = System.DateTime.UtcNow
            });
            return Task.FromResult(true);
        }

        public Task<bool> RemoveOldMemory(int hours)
        {
            var cutoff = System.DateTime.UtcNow.AddHours(-hours);
            _memories.RemoveAll(m => m.Timestamp < cutoff);
            return Task.FromResult(true);
        }

        public Task<List<NpcMemory>> GetMemoryContext(int limit)
        {
            var top = _memories.OrderByDescending(m => m.Score).Take(limit).ToList();
            return Task.FromResult(top);
        }

        public Task<bool> SetWorldPeriod(string period)
        {
            _worldPeriod = period == "NIGHT" ? 1 : 0;
            return Task.FromResult(true);
        }

        public Task<WorldStateDto?> FetchWorldStateAsync()
        {
            return Task.FromResult<WorldStateDto?>(new WorldStateDto
            {
                Id = 1,
                GameTime = "12:00",
                Weather = "CLEAR",
                TrafficLevel = 0,
                NightlifeLevel = _worldPeriod == 1 ? 100 : 0,
                TimeScale = 60f
            });
        }

        public Task<WorldClockDto?> FetchWorldClockAsync()
        {
            return Task.FromResult(new WorldClockDto
            {
                Success = true,
                GameDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
                GameTime = "12:00",
                MinutesSinceMidnight = _worldPeriod == 1 ? 1200 : 720,
                Period = _worldPeriod == 1 ? "NIGHT" : "AFTER_HOURS",
                TimeScale = 60f,
                Weather = "CLEAR",
                TrafficLevel = _worldPeriod == 1 ? 100 : 0,
                NightlifeLevel = _worldPeriod == 1 ? 100 : 0
            });
        }

        public Task<WorldClockDto?> AdvanceWorldTimeAsync()
        {
            return FetchWorldClockAsync();
        }

        public Task<WorldClockDto?> SetPeriodAsync(string period)
        {
            _worldPeriod = period == "NIGHT" ? 1 : 0;
            return FetchWorldClockAsync();
        }

        public Task<bool> SetWorldReactivity(float attendance, float trafficLevel)
        {
            // Mock: adjust world period if attendance is high
            if (attendance > 0.5f) _worldPeriod = 1;
            return Task.FromResult(true);
        }

        public Task<bool> AddOpportunity(Opportunity opportunity)
        {
            _opportunities.Add(opportunity);
            return Task.FromResult(true);
        }

        public Task<List<Opportunity>> GetAvailableOpportunities()
        {
            return Task.FromResult(_opportunities);
        }

        public Task<bool> AcceptOpportunity(string opportunityId)
        {
            var opp = _opportunities.Find(o => o.Id == opportunityId);
            if (opp != null)
            {
                opp.Status = "ACCEPTED";
            }
            return Task.FromResult(true);
        }

        public Task<bool> AddEvent(WorldEvent worldEvent)
        {
            _events.Add(worldEvent);
            return Task.FromResult(true);
        }

        public void AddPendingTravel(PendingTravelCommand travel)
        {
            _pendingTravels.Add(travel);
        }

        public Task<List<WorldEvent>> GetWorldEvents(string period)
        {
            var result = _events.FindAll(e => e.Period == period);
            return Task.FromResult(result);
        }

        public void Reset()
        {
            _pendingTravels.Clear();
            _relationships.Clear();
            _memories.Clear();
            _opportunities.Clear();
            _events.Clear();
        }
    }
}