using Microsoft.AspNetCore.Mvc;
using MvcBean.Areas.Identity.Services;
using System.ComponentModel.DataAnnotations;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class RegisterModel : BaseAccountPageModel
    {
        private readonly AccountService _accountService;

        public RegisterModel(AccountService accountService) : base(accountService)
        {
            _accountService = accountService;
            // Initialize 'Input' to satisfy the compiler's non-nullable property requirement
            Input = new InputModel
            {
                Email = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };
        }

        [BindProperty]
        public InputModel Input { get; set; } // Non-nullable property initialized in the constructor

        public async Task<IActionResult> OnPostAsync()
        {
            // Check for null or empty email and password
            if (string.IsNullOrEmpty(Input?.Email) || string.IsNullOrEmpty(Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            var result = await _accountService.RegisterUserAsync(Input.Email, Input.Password);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty; // Explicitly initialized to avoid warnings

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty; // Explicitly initialized to avoid warnings

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty; // Explicitly initialized to avoid warnings
        }
    }
}
