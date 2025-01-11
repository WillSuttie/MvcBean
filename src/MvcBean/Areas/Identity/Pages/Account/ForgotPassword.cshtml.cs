using Microsoft.AspNetCore.Mvc;
using MvcBean.Areas.Identity.Services;
using System.ComponentModel.DataAnnotations;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : BaseAccountPageModel
    {
        private readonly AccountService _accountService;

        public ForgotPasswordModel(AccountService accountService) : base(accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new(); // Initialize Input.

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(Input.Email)) // Check for null or invalid Email.
            {
                ModelState.AddModelError(string.Empty, "A valid email address is required.");
                return Page();
            }

            var token = await _accountService.GeneratePasswordResetTokenAsync(Input.Email);
            if (token != null)
            {
                // Log token generation
                await _accountService.LogActionAsync($"Generated password reset token for {Input.Email}");
            }

            // Redirect to confirmation
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty; // Default to empty string.
        }
    }
}
