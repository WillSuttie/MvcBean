using Microsoft.AspNetCore.Mvc.RazorPages;
using MvcBean.Areas.Identity.Services;

namespace MvcBean.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Base model for all Identity Razor Pages, providing shared functionality
    /// and access to AccountService.
    /// </summary>
    public abstract class BaseAccountPageModel(AccountService accountService) : PageModel
    {
        /// <summary>
        /// Provides shared access to the AccountService.
        /// </summary>
        protected AccountService AccountService { get; } = accountService;

        // Add shared logic here (if needed in the future)
    }
}
