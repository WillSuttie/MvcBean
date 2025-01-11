using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MvcBean.Areas.Identity.Services;

public class AccountService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountService> logger)
{
    private readonly UserManager<IdentityUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly SignInManager<IdentityUser> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    private readonly ILogger<AccountService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Logs a generic action asynchronously.
    /// </summary>
    public Task LogActionAsync(string message)
    {
        _logger.LogInformation(message);
        return Task.CompletedTask; // Keeps the method asynchronous.
    }

    /// <summary>
    /// Signs in a user with the provided credentials.
    /// </summary>
    public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe)
    {
        _logger.LogInformation("Attempting to sign in user with email: {Email}", email);

        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} signed in successfully.", email);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out.", email);
        }
        else
        {
            _logger.LogWarning("Failed login attempt for user {Email}.", email);
        }

        return result;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    public async Task<IdentityResult> RegisterUserAsync(string email, string password)
    {
        var user = new IdentityUser { UserName = email, Email = email };

        _logger.LogInformation("Attempting to register user with email: {Email}", email);

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} registered successfully.", email);
        }
        else
        {
            _logger.LogError("Registration failed for user {Email}. Errors: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    /// <summary>
    /// Generates a password reset token for a user.
    /// </summary>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        _logger.LogInformation("Generating password reset token for user with email: {Email}", email);

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("Password reset token generation failed. User with email {Email} not found.", email);
            return null;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("Password reset token generated successfully for user {Email}.", email);

        return token;
    }

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        _logger.LogInformation("Attempting to confirm email for user ID: {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("Email confirmation failed. User with ID {UserId} not found.", userId);
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed successfully for user ID: {UserId}", userId);
            return true;
        }

        _logger.LogError("Email confirmation failed for user ID {UserId}. Errors: {Errors}",
            userId, string.Join(", ", result.Errors.Select(e => e.Description)));

        return false;
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        _logger.LogInformation("Attempting to reset password for user with email: {Email}", email);

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("Password reset failed. User with email {Email} not found.", email);
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = $"User with email {email} not found."
            });
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successfully for user {Email}.", email);
        }
        else
        {
            _logger.LogError("Password reset failed for user {Email}. Errors: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    /// <summary>
    /// Finds a user by their ID.
    /// </summary>
    public async Task<IdentityUser?> FindUserByIdAsync(string userId)
    {
        _logger.LogInformation("Attempting to find user with ID: {UserId}", userId);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID is null or empty.");
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
        }
        else
        {
            _logger.LogInformation("User with ID {UserId} found: {Email}.", userId, user.Email);
        }

        return user;
    }
}
