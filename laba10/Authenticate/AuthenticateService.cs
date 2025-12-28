using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ResultsAuthenticate
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticateService(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> RegisterAsync(string login, string password, string role)
        {
            if (await _userManager.FindByNameAsync(login) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User with this login already exists." });
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                if (role != "READER" && role != "WRITER")
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Invalid role specified." });
                }
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = new IdentityUser { UserName = login };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return result;
        }

        public async Task<bool> SignInAsync(string login, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(login, password, isPersistent: true, lockoutOnFailure: false);
            return result.Succeeded;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}