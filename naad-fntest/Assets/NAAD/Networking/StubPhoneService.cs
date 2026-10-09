using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    public sealed class StubPhoneService : IPhoneService
    {
        private readonly INAADLogger _log;
        private string _messageId = "stub-msg-001";
        private bool _responded;

        public StubPhoneService(INAADLogger log)
        {
            _log = log;
        }

        public Task<bool> SeedTundeInviteAsync()
        {
            _log.Info("Phone", "Stub seed Tunde invite");
            return Task.FromResult(true);
        }

        public Task<string?> ListMessagesJsonAsync()
        {
            var json =
                "{\"success\":true,\"messages\":[{\"id\":\"" + _messageId +
                "\",\"body\":\"Beach dey hot tonight. You pulling up?\",\"thread_key\":\"tunde_beach_invite\"," +
                "\"action_options\":[\"GO\",\"ASK_DETAILS\",\"DECLINE\"],\"chosen_action\":" +
                (_responded ? "\"GO\"" : "null") + "}]}";
            return Task.FromResult<string?>(json);
        }

        public Task<bool> RespondAsync(string requestId, string messageId, string action)
        {
            _responded = true;
            _log.Info("Phone", $"Stub respond {action}");
            return Task.FromResult(true);
        }

        public Task<bool> StartNightAsync(string requestId)
        {
            _log.Info("Phone", "Stub start night");
            return Task.FromResult(true);
        }

        public Task<NightSummaryDto?> CompleteNightAsync(string requestId)
        {
            return Task.FromResult<NightSummaryDto?>(new NightSummaryDto
            {
                Success = true,
                SpentNgn = 9000,
                Transactions = 2,
                PeopleMet = 1,
                NewConnection = "Tunde",
                TrustWithTunde = 55,
                BestMoment = "Night out on the district"
            });
        }

        public Task<int?> GetBalanceAsync() => Task.FromResult<int?>(100000);
    }
}
