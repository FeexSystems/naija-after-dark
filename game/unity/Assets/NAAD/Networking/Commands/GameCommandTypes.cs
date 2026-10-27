using System.Collections.Generic;

namespace NAAD.Networking
{
    /// <summary>Typed command payload for TRAVEL.</summary>
    public sealed class TravelCommand
    {
        public string? ToLocationId { get; set; }
    }

    /// <summary>Typed command payload for SPEND.</summary>
    public sealed class SpendCommand
    {
        public string? Sku { get; set; }
    }

    /// <summary>Typed command payload for BUY.</summary>
    public sealed class BuyCommand
    {
        public string? Sku { get; set; }
        public string? LocationId { get; set; }
    }

    /// <summary>Typed command payload for START_NIGHT.</summary>
    public sealed class StartNightCommand
    {
    }

    /// <summary>Typed command payload for COMPLETE_NIGHT.</summary>
    public sealed class CompleteNightCommand
    {
    }

    /// <summary>Typed command payload for PHONE_REPLY.</summary>
    public sealed class PhoneReplyCommand
    {
        public string? MessageId { get; set; }
        public string? Action { get; set; }
    }

    /// <summary>Typed command payload for JOIN_ROOM.</summary>
    public sealed class JoinRoomCommand
    {
        public string? LocationId { get; set; }
    }

    /// <summary>Typed command payload for LEAVE_ROOM.</summary>
    public sealed class LeaveRoomCommand
    {
    }

    /// <summary>Typed command payload for ATTEND_EVENT.</summary>
    public sealed class AttendEventCommand
    {
        public string? EventId { get; set; }
    }

    /// <summary>Typed command payload for RELATIONSHIP_DELTA.</summary>
    public sealed class RelationshipDeltaCommand
    {
        public string? TargetId { get; set; }
        public int? Trust { get; set; }
    }

    /// <summary>Typed command payload for SET_CAREER.</summary>
    public sealed class SetCareerCommand
    {
        public string? CareerId { get; set; }
    }

    /// <summary>Typed command payload for ACCEPT_OPPORTUNITY.</summary>
    public sealed class AcceptOpportunityCommand
    {
        public string? OpportunityId { get; set; }
    }

    /// <summary>Typed command payload for COMMIT_MEMORY.</summary>
    public sealed class CommitMemoryCommand
    {
        public string? NpcId { get; set; }
        public string? MemoryType { get; set; }
        public string? Summary { get; set; }
    }

    /// <summary>Typed command payload for SET_WORLD_PERIOD.</summary>
    public sealed class SetWorldPeriodCommand
    {
        public string? Period { get; set; }
    }
}