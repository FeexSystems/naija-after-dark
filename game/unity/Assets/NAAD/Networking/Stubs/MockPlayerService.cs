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
    /// Mock implementation of <see cref="IPlayerService"/> for automated testing.
    /// <para>
    /// Tracks player state in-memory, supporting location, balance, career,
    /// relationship deltas, and consent flags. All state is reset by <see cref="Reset"/>.
    /// </para>
    /// </summary>
    public sealed class MockPlayerService : IPlayerService
    {
        private static readonly Dictionary<string, MockPlayerService> _instances =
            new(StringComparer.OrdinalIgnoreCase);

        public static MockPlayerService GetOrCreate()
        {
            var key = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            if (!_instances.TryGetValue(key, out var instance))
            {
                instance = new MockPlayerService();
                _instances[key] = instance;
            }
            return instance;
        }

        private MockPlayerService()
        {
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Tunde, Trust = 50, Respect = 40, Affection = 30, Loyalty = 20, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.MamaSeyi, Trust = 70, Respect = 60, Affection = 80, Loyalty = 90, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Amaka, Trust = 30, Respect = 40, Affection = 20, Loyalty = 10, Conflict = 5 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Emeka, Trust = 80, Respect = 70, Affection = 60, Loyalty = 80, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Dami, Trust = 40, Respect = 50, Affection = 30, Loyalty = 30, Conflict = 0 });
        }

        // State
        public int CurrentBalance { get => _currentBalance; }
        public string CurrentLocation => _currentLocation;
        public string CurrentCareer => _currentCareer;
        public List<Relationship> Relationships => _relationships;
        public bool IsConsentGiven => _isConsentGiven;
        public Dictionary<string, int> NpcMessageCounts => new(_npcMessageCounts);

        private int _currentBalance = 50000;
        private string _currentLocation = NightDistrictIds.Apartment;
        private string _currentCareer = "DEVELOPER";
        private readonly List<Relationship> _relationships = new();
        private bool _isConsentGiven = false;
        private readonly Dictionary<string, int> _npcMessageCounts = new();

        public Task<PlayerDto?> FetchCurrentPlayerAsync()
        {
            return Task.FromResult(new PlayerDto
            {
                Id = "mock_player",
                DisplayName = "Player",
                Balance = _currentBalance,
                LocationId = _currentLocation,
                Career = _currentCareer,
                IsConsentGiven = _isConsentGiven
            });
        }

        public Task<bool> UpdateBalanceAsync(int amount, string requestId)
        {
            _currentBalance = amount;
            return Task.FromResult(true);
        }

        public Task<bool> TravelAsync(string toLocationId, string requestId)
        {
            _currentLocation = toLocationId;
            return Task.FromResult(true);
        }

        public Task<bool> ApplyRelationshipDelta(string npcId, int trust)
        {
            var npc = _relationships.Find(r => r.NpcId == npcId);
            if (npc == null) return Task.FromResult(false);

            npc.Trust = Mathf.Clamp(npc.Trust + trust, 0, 100);
            return Task.FromResult(true);
        }

        public Task<bool> SetConsentGiven(bool given, string requestId)
        {
            _isConsentGiven = given;
            return Task.FromResult(true);
        }

        public Task<bool> SetCareer(string careerId, string requestId)
        {
            _currentCareer = careerId;
            return Task.FromResult(true);
        }

        public void SetBalance(int amount)
        {
            _currentBalance = amount;
        }

        public void SetCareer(string careerId)
        {
            _currentCareer = careerId;
        }

        public Task<bool> UpdateNpcMessageCount(string npcId, string requestId)
        {
            if (_npcMessageCounts.TryGetValue(npcId, out var count))
            {
                _npcMessageCounts[npcId] = count + 1;
            }
            else
            {
                _npcMessageCounts[npcId] = 1;
            }
            return Task.FromResult(true);
        }

        public Task<bool> DeletePlayerAsync(string requestId)
        {
            return Task.FromResult(true);
        }

        public void Reset()
        {
            _currentBalance = 50000;
            _currentLocation = NightDistrictIds.Apartment;
            _currentCareer = "DEVELOPER";
            _relationships.Clear();
            _isConsentGiven = false;
            _npcMessageCounts.Clear();

            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Tunde, Trust = 50, Respect = 40, Affection = 30, Loyalty = 20, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.MamaSeyi, Trust = 70, Respect = 60, Affection = 80, Loyalty = 90, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Amaka, Trust = 30, Respect = 40, Affection = 20, Loyalty = 10, Conflict = 5 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Emeka, Trust = 80, Respect = 70, Affection = 60, Loyalty = 80, Conflict = 0 });
            _relationships.Add(new Relationship { NpcId = NightDistrictIds.Dami, Trust = 40, Respect = 50, Affection = 30, Loyalty = 30, Conflict = 0 });
        }
    }
}