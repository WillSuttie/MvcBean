using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MvcBean.Areas.Identity.Services;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class LoginModel : BaseAccountPageModel
    {
        private readonly AccountService _accountService;

        public LoginModel(AccountService accountService) : base(accountService)
        {
            _accountService = accountService;
            Input = new InputModel
            {
                Email = string.Empty,
                Password = string.Empty,
                RememberMe = false
            };
        }

        [BindProperty]
        public InputModel Input { get; set; } // Non-nullable property initialized in the constructor.

        public async Task OnGetAsync(string? returnUrl = null)
        {
            // Log that the login page was accessed (example shared logic in BaseAccountPageModel).
            await _accountService.LogActionAsync("Accessed Login Page");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Validate if Email and Password are provided.
            if (string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            // Attempt to sign in the user via AccountService.
            var result = await _accountService.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "~/");
            }

            // Handle login failure.
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty; // Prevent null by defaulting to empty.

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty; // Prevent null by defaulting to empty.

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}
