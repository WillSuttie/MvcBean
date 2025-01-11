using Microsoft.AspNetCore.Mvc;
using MvcBean.Areas.Identity.Services;
using System.ComponentModel.DataAnnotations;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : BaseAccountPageModel
    {
        private readonly AccountService _accountService;

        public ResetPasswordModel(AccountService accountService) : base(accountService)
        {
            _accountService = accountService;
            Input = new InputModel
            {
                UserId = string.Empty,
                Token = string.Empty,
                Password = string.Empty
            };
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(Input.UserId) || string.IsNullOrEmpty(Input.Token))
            {
                ModelState.AddModelError(string.Empty, "Invalid reset password request.");
                return Page();
            }

            var result = await _accountService.ResetPasswordAsync(Input.UserId, Input.Token, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
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
            public string UserId { get; set; } = string.Empty; // New field

            [Required]
            public string Token { get; set; } = string.Empty; // New field

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
