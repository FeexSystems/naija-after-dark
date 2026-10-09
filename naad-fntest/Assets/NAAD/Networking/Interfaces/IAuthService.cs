using System.Threading.Tasks;

namespace NAAD.Networking
{
    /// <summary>
    /// Auth boundary. UI → AuthController → IAuthService → Supabase Auth.
    /// Access token is the user JWT only — never service-role.
    /// </summary>
    public interface IAuthService
    {
        Task<bool> SignUpAsync(string email, string password, string? displayName = null);
        Task<bool> SignInAsync(string email, string password);
        Task<bool> SignOutAsync();
        Task<bool> RestoreSessionAsync();
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetPlayerIdAsync();
        bool IsAuthenticated { get; }
    }
}
