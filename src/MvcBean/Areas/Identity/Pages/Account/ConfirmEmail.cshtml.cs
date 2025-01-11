using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services; // For IEmailSender
using MvcBean.Areas.Identity.Services;

namespace MvcBean.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel(AccountService accountService, UserManager<IdentityUser> userManager, IEmailSender emailSender) : BaseAccountPageModel(accountService)
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly IEmailSender _emailSender = emailSender;

        public async Task<IActionResult> OnGetAsync(string? userId, string? token)
        {
            // Check for missing or invalid parameters
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                await AccountService.LogActionAsync("Email confirmation failed: Missing userId or token.");
                return RedirectToPage("/Index");
            }

            // Attempt to find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await AccountService.LogActionAsync($"Email confirmation failed: User with ID '{userId}' not found.");
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            // Confirm the user's email
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await AccountService.LogActionAsync($"Email confirmed successfully for user {user.Email}.");

                // Check for null or empty email and handle appropriately
                if (string.IsNullOrEmpty(user.Email))
                {
                    await AccountService.LogActionAsync("Email confirmation succeeded, but user email is null.");
                    return BadRequest("Email confirmation succeeded, but email address is missing.");
                }

                // Send a welcome email after successful email confirmation
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Welcome to MvcBean!",
                    "Your email has been successfully confirmed. Welcome to our service!"
                );

                return RedirectToPage("/Account/Login");
            }

            // Log and handle failed email confirmation
            await AccountService.LogActionAsync($"Email confirmation failed for user {user.Email}.");
            return BadRequest("Email confirmation failed.");
        }

    }
}
