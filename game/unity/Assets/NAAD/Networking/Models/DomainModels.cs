using System;
using System.Collections.Generic;
using UnityEngine;

namespace NAAD.Networking
{
    /// <summary>Represents a pending travel command stored in world state.</summary>
    public sealed class PendingTravelCommand
    {
        public string RequestId { get; set; } = string.Empty;
        public string ToLocationId { get; set; } = string.Empty;
        public Vector3 Coordinates { get; set; } = Vector3.zero;
    }

    /// <summary>Tracks a relationship with an NPC.</summary>
    public sealed class Relationship
    {
        public string NpcId { get; set; } = string.Empty;
        public int Trust { get; set; }
        public int Respect { get; set; }
        public int Affection { get; set; }
        public int Loyalty { get; set; }
        public int Conflict { get; set; }
    }

    /// <summary>Represents an NPC memory committed via COMMIT_MEMORY.</summary>
    public sealed class NpcMemory
    {
        public string NpcId { get; set; } = string.Empty;
        public string MemoryType { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float Score { get; set; }
    }

    /// <summary>Represents an opportunity available to the player.</summary>
    public sealed class Opportunity
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TrustThreshold { get; set; }
        public string Status { get; set; } = "OPEN";
    }

    /// <summary>Represents a scheduled world event.</summary>
    public sealed class WorldEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Period { get; set; } = "NIGHT";
        public int Attendance { get; set; }
    }

    /// <summary>Represents an in-game phone message.</summary>
    public sealed class PhoneMessage
    {
        public string Id { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ThreadKey { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string SenderType { get; set; } = string.Empty;
        public string? ChosenAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResponseTime { get; set; }
        public bool IsRead { get; set; }
    }
}