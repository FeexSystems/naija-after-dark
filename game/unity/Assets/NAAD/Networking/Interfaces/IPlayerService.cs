using System.Threading.Tasks;

namespace NAAD.Networking
{
    public interface IPlayerService
    {
        Task<PlayerDto?> FetchCurrentPlayerAsync();
    }

    public sealed class PlayerDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Balance { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public string Career { get; set; } = string.Empty;
        public bool IsConsentGiven { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
