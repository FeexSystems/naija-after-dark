using System.Threading.Tasks;

namespace NAAD.Networking
{
    /// <summary>
    /// Auth boundary. UI never talks to Supabase directly.
    /// Access token is for server API calls only — never service-role.
    /// </summary>
    public interface IAuthService
    {
        Task<bool> SignInAsync(string email, string password);
        Task<bool> SignOutAsync();
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetPlayerIdAsync();
        bool IsAuthenticated { get; }
    }
}
