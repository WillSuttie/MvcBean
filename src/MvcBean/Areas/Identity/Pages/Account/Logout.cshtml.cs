using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcBean.Areas.Identity.Services;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class LogoutModel : BaseAccountPageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LogoutModel(AccountService accountService, SignInManager<IdentityUser> signInManager)
            : base(accountService)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Perform the logout
            await _signInManager.SignOutAsync();
            await AccountService.LogActionAsync("User logged out successfully.");

            // Redirect to the home page or another relevant page
            return RedirectToPage("/Index");
        }
    }
}
