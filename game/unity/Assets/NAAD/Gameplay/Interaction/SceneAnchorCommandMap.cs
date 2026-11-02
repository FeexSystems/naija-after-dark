using System.Collections.Generic;
using NAAD.World;

namespace NAAD.Gameplay.Interaction
{
    public enum InteractionKind
    {
        None = 0,
        Talk = 1,
        Travel = 2,
        Spend = 3,
        AttendEvent = 4,
        Examine = 5
    }

    /// <summary>Resolved player intent for one scene anchor.</summary>
    public readonly struct AnchorCommand
    {
        public readonly InteractionKind Kind;
        public readonly string Payload;

        public AnchorCommand(InteractionKind kind, string payload)
        {
            Kind = kind;
            Payload = payload ?? string.Empty;
        }

        public bool IsResolved => Kind != InteractionKind.None;
    }

    /// <summary>
    /// Translates a scene anchor's <c>contentId</c> into an authoritative command intent.
    ///
    /// The scene contentIds are a third vocabulary: they are neither the location UUIDs
    /// nor the SKUs in <see cref="NightDistrictIds"/> and the migrations. An anchor that
    /// is not listed here resolves to nothing and is reported by
    /// <c>NaadScenePipeline.ValidateScenes</c>, so a new marker cannot silently become a
    /// dead interaction.
    /// </summary>
    public static class SceneAnchorCommandMap
    {
        // SKUs / event ids come from supabase/migrations:
        //   0007_economy_relationships_phone_first_night.sql (price_list)
        //   0008_multiplayer_content_events_reactivity.sql   (world_events)
        public const string BeachDrinkSku = "BEACH_DRINK";
        public const string BeachPartyEventId = "f1111111-1111-1111-1111-111111111101";
        public const string ClubNightEventId = "f1111111-1111-1111-1111-111111111102";

        private static readonly Dictionary<string, AnchorCommand> CommandByContentId =
            new Dictionary<string, AnchorCommand>(System.StringComparer.Ordinal)
            {
                // Suya Spot ("syu" = suya). Both the grill and the counter sell a plate.
                { "syu.grill.open", new AnchorCommand(InteractionKind.Spend, NightDistrictIds.SuyaSku) },
                { "syu.counter.order", new AnchorCommand(InteractionKind.Spend, NightDistrictIds.SuyaSku) },

                // Nightclub
                { "ncl.bar.order", new AnchorCommand(InteractionKind.Spend, NightDistrictIds.ClubTicketSku) },
                { "ncl.stage.perform", new AnchorCommand(InteractionKind.AttendEvent, ClubNightEventId) },

                // Beach
                { "beach.bar.drinks", new AnchorCommand(InteractionKind.Spend, BeachDrinkSku) },
                { "beach.bonfire.gather", new AnchorCommand(InteractionKind.AttendEvent, BeachPartyEventId) },

                // NPCs — the payload is the NPC's content id.
                { "npc.crew.mama-seyi", new AnchorCommand(InteractionKind.Talk, NightDistrictIds.MamaSeyi) },
                { "npc.crew.emeka", new AnchorCommand(InteractionKind.Talk, NightDistrictIds.Emeka) },

                // Apartment props: presentation only for now.
                { "naad.scene.apartment-phone", new AnchorCommand(InteractionKind.Examine, string.Empty) },
                { "naad.scene.apartment-wardrobe", new AnchorCommand(InteractionKind.Examine, string.Empty) },

                // Travel markers. The payload is the destination location id; the scenes
                // are all in-realm travel, so this is authoritative-server travel, not a
                // scene load.
                { "naad.scene.apartment-exit-street", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Street) },
                { "naad.scene.street-to-apartment", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Apartment) },
                { "naad.scene.street-to-suya", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.SuyaSpot) },
                { "naad.scene.street-to-club", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Nightclub) },
                { "naad.scene.suya-spot-exit", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Street) },
                { "naad.scene.nightclub-exit", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Street) },
                { "naad.scene.beach-exit", new AnchorCommand(InteractionKind.Travel, NightDistrictIds.Street) },
            };

        /// <summary>True when the contentId maps to a dispatchable command.</summary>
        public static bool IsMapped(string contentId)
        {
            return !string.IsNullOrEmpty(contentId) && CommandByContentId.ContainsKey(contentId);
        }

        /// <summary>
        /// Resolve an anchor into a command intent. Spawn and landmark markers have no
        /// intent and return false — that is expected, not an error.
        /// </summary>
        public static bool TryResolve(SceneAnchor anchor, out AnchorCommand command)
        {
            command = default;
            if (anchor == null)
                return false;

            if (anchor.Kind == SceneAnchor.AnchorKind.PlayerSpawn ||
                anchor.Kind == SceneAnchor.AnchorKind.CameraRig)
                return false;

            return CommandByContentId.TryGetValue(anchor.ContentId ?? string.Empty, out command);
        }

        /// <summary>
        /// Content ids that must resolve but do not. Used by the scene validator so an
        /// unmapped interaction marker fails CI instead of doing nothing at runtime.
        /// </summary>
        public static IEnumerable<string> UnmappedContentIds(IEnumerable<SceneAnchor> anchors)
        {
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var anchor in anchors)
            {
                if (anchor == null)
                    continue;

                if (anchor.Kind == SceneAnchor.AnchorKind.PlayerSpawn ||
                    anchor.Kind == SceneAnchor.AnchorKind.CameraRig)
                    continue;

                var id = anchor.ContentId ?? string.Empty;
                if (id.Length == 0 || !CommandByContentId.ContainsKey(id))
                {
                    if (seen.Add(id))
                        yield return id;
                }
            }
        }
    }
}
