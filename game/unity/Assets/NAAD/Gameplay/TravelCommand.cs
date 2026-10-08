using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;
using UnityEngine;

namespace NAAD.Gameplay
{
    /// <summary>
    /// Gate 5 demo: Apartment → Suya Spot via server TRAVEL command.
    /// </summary>
    public sealed class TravelCommand : MonoBehaviour
    {
        public static readonly string ApartmentId = "a1111111-1111-1111-1111-111111111101";
        public static readonly string SuyaSpotId = "a1111111-1111-1111-1111-111111111102";

        [SerializeField] private bool travelOnStart;

        private async void Start()
        {
            if (travelOnStart)
                await TravelToSuyaSpotAsync();
        }

        public async Task<CommandResult?> TravelToSuyaSpotAsync()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null)
            {
                Debug.LogError("[NAAD][Travel] Application root missing");
                return null;
            }

            var playerId = await root.Auth.GetPlayerIdAsync() ?? "";
            var command = new GameCommand
            {
                RequestId = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                Type = GameCommandType.Travel,
                ClientTimestamp = DateTime.UtcNow.ToString("o"),
                Payload = new Dictionary<string, object>
                {
                    ["toLocationId"] = SuyaSpotId
                }
            };

            var result = await root.Commands.ExecuteAsync(command);
            if (result.Success)
            {
                string from = "";
                if (result.Payload != null && result.Payload.TryGetValue("fromLocationId", out var f))
                    from = f;
                root.Logger.Info("Travel", $"Arrived Suya Spot from={from}");
            }
            else
            {
                root.Logger.Warn("Travel", $"{result.ErrorCode}: {result.ErrorMessage}");
            }

            return result;
        }
    }
}
