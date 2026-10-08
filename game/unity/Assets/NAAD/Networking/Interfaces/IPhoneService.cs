using System.Threading.Tasks;

namespace NAAD.Networking
{
    public sealed class NightSummaryDto
    {
        public bool Success;
        public int SpentNgn;
        public int Transactions;
        public int PeopleMet;
        public string? NewConnection;
        public int? TrustWithTunde;
        public string BestMoment = "";
    }

    public interface IPhoneService
    {
        Task<bool> SeedTundeInviteAsync();
        Task<string?> ListMessagesJsonAsync();
        Task<bool> RespondAsync(string requestId, string messageId, string action);
        Task<bool> StartNightAsync(string requestId);
        Task<NightSummaryDto?> CompleteNightAsync(string requestId);
        Task<int?> GetBalanceAsync();
    }
}
