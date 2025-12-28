using System.Threading.Tasks;

namespace ResultsAuthenticate
{
    public interface IAuthenticateService
    {
        Task<bool> SignInAsync(string login, string password);

        Task SignOutAsync();

        Task<Microsoft.AspNetCore.Identity.IdentityResult> RegisterAsync(string login, string password, string role);
    }
}