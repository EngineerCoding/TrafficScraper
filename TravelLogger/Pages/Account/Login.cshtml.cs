using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelLogger.Models;

namespace TravelLogger.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty] public LoginInputModel Input { get; set; }

        public string ErrorString { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, false,
                        lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl?.Trim() ?? "/");
                }

                ErrorString = "Invalid credentials";
            }
            else
            {
                ErrorString = null;
            }

            return Page();
        }
    }
}
