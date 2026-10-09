using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.Logging;
using NAAD.Networking;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Gameplay.FirstNight
{
    /// <summary>
    /// Phase B — First Night vertical slice.
    /// All mutations go through ICommandService → naad_execute_command.
    /// </summary>
    public sealed class FirstNightController : MonoBehaviour
    {
        [Header("Flow options")]
        [SerializeField] private bool includeClubTicket = true;
        [SerializeField] private bool autoRunAfterAuth;
        [SerializeField] private float stepDelaySeconds = 0.35f;

        public FirstNightStep Step { get; private set; } = FirstNightStep.Idle;
        public string StatusMessage { get; private set; } = "Ready";
        public string PendingMessageId { get; private set; }
        public string PendingMessageBody { get; private set; }
        public NightSummaryDto LastSummary { get; private set; }
        public int? LastBalance { get; private set; }
        public string LastError { get; private set; }
        public bool IsBusy { get; private set; }

        public event Action Changed;

        private NAADApplicationRoot Root => NAADApplicationRoot.Instance;
        private INAADLogger Log => Root.Logger;

        private async void Start()
        {
            if (autoRunAfterAuth)
            {
                await Task.Delay(500);
                if (Root != null && Root.Auth.IsAuthenticated)
                    await RunFullNightAsync("GO");
            }
        }

        public async Task<bool> StartNightAsync()
        {
            if (!EnsureRoot()) return false;
            IsBusy = true;
            SetStep(FirstNightStep.StartingNight, "Starting night...");

            var cmd = NewCommand(GameCommandType.StartNight);
            var result = await Root.Commands.ExecuteAsync(cmd);
            if (!result.Success)
                return Fail(result.ErrorCode, result.ErrorMessage);

            await RefreshBalanceAsync();
            SetStep(FirstNightStep.Phone, "Night started — check phone");
            await LoadPhoneInviteAsync();
            IsBusy = false;
            return true;
        }

        public async Task LoadPhoneInviteAsync()
        {
            if (!EnsureRoot()) return;
            await Root.Phone.SeedTundeInviteAsync();
            var json = await Root.Phone.ListMessagesJsonAsync();
            PendingMessageId = ExtractOpenInviteId(json);
            PendingMessageBody = ExtractOpenInviteBody(json)
                                 ?? "Beach dey hot tonight. You pulling up?";
            SetStep(FirstNightStep.AwaitingPhoneChoice,
                PendingMessageBody + "\n[GO] [ASK_DETAILS] [DECLINE]");
            Notify();
        }

        public async Task<bool> RespondToInviteAsync(string action)
        {
            if (!EnsureRoot()) return false;
            if (string.IsNullOrEmpty(PendingMessageId))
            {
                await LoadPhoneInviteAsync();
                if (string.IsNullOrEmpty(PendingMessageId))
                    return Fail("NO_MESSAGE", "No open Tunde invite");
            }

            IsBusy = true;
            SetStep(FirstNightStep.Phone, "Replying " + action + "...");

            var cmd = NewCommand(GameCommandType.PhoneReply);
            cmd.Payload["messageId"] = PendingMessageId;
            cmd.Payload["action"] = action.ToUpperInvariant();

            var result = await Root.Commands.ExecuteAsync(cmd);
            if (!result.Success)
                return Fail(result.ErrorCode, result.ErrorMessage);

            StatusMessage = "Phone: " + action;
            IsBusy = false;
            Notify();
            return true;
        }

        public async Task<bool> GoToSuyaAndBuyAsync()
        {
            if (!EnsureRoot()) return false;
            IsBusy = true;

            SetStep(FirstNightStep.TravelSuya, "Travel to Suya Spot...");
            var travel = NewCommand(GameCommandType.Travel);
            travel.Payload["toLocationId"] = NightDistrictIds.SuyaSpot;
            var tr = await Root.Commands.ExecuteAsync(travel);
            if (!tr.Success) return Fail(tr.ErrorCode, tr.ErrorMessage);
            await Delay();

            SetStep(FirstNightStep.BuySuya, "Buying suya...");
            var spend = NewCommand(GameCommandType.Spend);
            spend.Payload["sku"] = NightDistrictIds.SuyaSku;
            spend.Payload["locationId"] = NightDistrictIds.SuyaSpot;
            var sr = await Root.Commands.ExecuteAsync(spend);
            if (!sr.Success) return Fail(sr.ErrorCode, sr.ErrorMessage);

            await RefreshBalanceAsync();
            StatusMessage = "Ate suya at the spot";
            IsBusy = false;
            Notify();
            return true;
        }

        public async Task<bool> GoToClubAndBuyTicketAsync()
        {
            if (!EnsureRoot()) return false;
            IsBusy = true;

            SetStep(FirstNightStep.TravelClub, "Travel to Nightclub...");
            var travel = NewCommand(GameCommandType.Travel);
            travel.Payload["toLocationId"] = NightDistrictIds.Nightclub;
            var tr = await Root.Commands.ExecuteAsync(travel);
            if (!tr.Success) return Fail(tr.ErrorCode, tr.ErrorMessage);
            await Delay();

            SetStep(FirstNightStep.BuyTicket, "Buying club ticket...");
            var spend = NewCommand(GameCommandType.Spend);
            spend.Payload["sku"] = NightDistrictIds.ClubTicketSku;
            spend.Payload["locationId"] = NightDistrictIds.Nightclub;
            var sr = await Root.Commands.ExecuteAsync(spend);
            if (!sr.Success) return Fail(sr.ErrorCode, sr.ErrorMessage);

            await RefreshBalanceAsync();
            StatusMessage = "Inside the club";
            IsBusy = false;
            Notify();
            return true;
        }

        public async Task<bool> CompleteNightAsync()
        {
            if (!EnsureRoot()) return false;
            IsBusy = true;
            SetStep(FirstNightStep.CompletingNight, "Ending night...");

            var cmd = NewCommand(GameCommandType.CompleteNight);
            var result = await Root.Commands.ExecuteAsync(cmd);
            if (!result.Success)
                return Fail(result.ErrorCode, result.ErrorMessage);

            LastSummary = new NightSummaryDto
            {
                Success = true,
                SpentNgn = ParseInt(result.Payload, "spentNgn"),
                Transactions = ParseInt(result.Payload, "transactions"),
                PeopleMet = ParseInt(result.Payload, "peopleMet"),
                NewConnection = GetPayload(result.Payload, "newConnection"),
                TrustWithTunde = ParseIntNullable(result.Payload, "trustWithTunde"),
                BestMoment = GetPayload(result.Payload, "bestMoment") ?? "Night out"
            };

            await RefreshBalanceAsync();
            SetStep(FirstNightStep.Summary, FormatSummary(LastSummary));
            IsBusy = false;
            return true;
        }

        public async Task<bool> RunFullNightAsync(string phoneAction = "GO")
        {
            if (!await StartNightAsync()) return false;
            await Delay();
            if (!await RespondToInviteAsync(phoneAction)) return false;
            await Delay();
            if (!await GoToSuyaAndBuyAsync()) return false;
            await Delay();
            if (includeClubTicket)
            {
                if (!await GoToClubAndBuyTicketAsync()) return false;
                await Delay();
            }
            return await CompleteNightAsync();
        }

        private async Task RefreshBalanceAsync()
        {
            LastBalance = await Root.Phone.GetBalanceAsync();
        }

        private GameCommand NewCommand(GameCommandType type)
        {
            return new GameCommand
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Type = type,
                ClientTimestamp = DateTime.UtcNow.ToString("o"),
                Payload = new Dictionary<string, object>()
            };
        }

        private bool EnsureRoot()
        {
            if (NAADApplicationRoot.Instance != null) return true;
            LastError = "NAADApplicationRoot missing";
            SetStep(FirstNightStep.Failed, LastError);
            return false;
        }

        private bool Fail(string code, string message)
        {
            LastError = (code ?? "?") + ": " + (message ?? "");
            SetStep(FirstNightStep.Failed, LastError);
            IsBusy = false;
            Log.Error("FirstNight", LastError);
            return false;
        }

        private void SetStep(FirstNightStep step, string message)
        {
            Step = step;
            StatusMessage = message;
            Log.Info("FirstNight", step + ": " + message);
            Notify();
        }

        private void Notify()
        {
            if (Changed != null) Changed.Invoke();
        }

        private async Task Delay()
        {
            if (stepDelaySeconds <= 0) return;
            await Task.Delay(TimeSpan.FromSeconds(stepDelaySeconds));
        }

        private static string FormatSummary(NightSummaryDto s)
        {
            return "YOUR NIGHT\nSpent NGN " + s.SpentNgn
                + "\nPeople: " + s.PeopleMet
                + "\nConnection: " + (s.NewConnection ?? "-")
                + "\nTrust Tunde: " + (s.TrustWithTunde.HasValue ? s.TrustWithTunde.Value.ToString() : "-")
                + "\nMoment: " + s.BestMoment;
        }

        private static string GetPayload(Dictionary<string, string> payload, string key)
        {
            if (payload == null) return null;
            string v;
            return payload.TryGetValue(key, out v) ? v : null;
        }

        private static int ParseInt(Dictionary<string, string> payload, string key)
        {
            var v = GetPayload(payload, key);
            int n;
            return v != null && int.TryParse(v, out n) ? n : 0;
        }

        private static int? ParseIntNullable(Dictionary<string, string> payload, string key)
        {
            var v = GetPayload(payload, key);
            int n;
            if (v != null && int.TryParse(v, out n)) return n;
            return null;
        }

        private static string ExtractOpenInviteId(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            // naad_list_messages returns {success, messages:[...]} — scan each
            // message object for the open beach invite instead of slicing raw text.
            if (NaadJson.TryGetObject(json, "messages", out var msgs) && !string.IsNullOrEmpty(msgs))
            {
                foreach (var msg in SplitTopLevelObjects(msgs))
                {
                    if (!NaadJson.TryGetString(msg, "thread_key", out var thread)) continue;
                    if (thread != "tunde_beach_invite") continue;
                    // open invite = no chosen_action yet
                    if (NaadJson.TryGetString(msg, "chosen_action", out _)) continue;
                    if (NaadJson.TryGetString(msg, "id", out var inviteId)) return inviteId;
                }
            }
            // Fallback: first message id in the envelope.
            if (NaadJson.TryGetObject(json, "messages", out var fallback) && !string.IsNullOrEmpty(fallback))
            {
                foreach (var msg in SplitTopLevelObjects(fallback))
                {
                    if (NaadJson.TryGetString(msg, "id", out var anyId)) return anyId;
                }
            }
            return null;
        }

        private static string ExtractOpenInviteBody(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            if (NaadJson.TryGetObject(json, "messages", out var msgs) && !string.IsNullOrEmpty(msgs))
            {
                foreach (var msg in SplitTopLevelObjects(msgs))
                {
                    if (NaadJson.TryGetString(msg, "thread_key", out var thread)
                        && thread == "tunde_beach_invite"
                        && NaadJson.TryGetString(msg, "body", out var body))
                        return body;
                }
                foreach (var msg in SplitTopLevelObjects(msgs))
                {
                    if (NaadJson.TryGetString(msg, "body", out var anyBody)) return anyBody;
                }
            }
            return null;
        }

        /// <summary>Yields each top-level {...} inside a JSON array body.</summary>
        private static System.Collections.Generic.IEnumerable<string> SplitTopLevelObjects(string arrayJson)
        {
            var start = arrayJson.IndexOf('{');
            while (start >= 0)
            {
                var depth = 0;
                var inStr = false;
                for (var i = start; i < arrayJson.Length; i++)
                {
                    var c = arrayJson[i];
                    if (inStr)
                    {
                        if (c == '\\') { i++; continue; }
                        if (c == '"') inStr = false;
                        continue;
                    }
                    if (c == '"') { inStr = true; continue; }
                    if (c == '{') depth++;
                    else if (c == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            yield return arrayJson.Substring(start, i - start + 1);
                            start = arrayJson.IndexOf('{', i + 1);
                            break;
                        }
                    }
                }
                if (depth != 0) break;
                if (start < 0) break;
            }
        }
    }
}
