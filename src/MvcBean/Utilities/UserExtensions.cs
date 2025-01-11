using System.Security.Claims;

namespace MvcBean.Utilities
{
    public static class UserExtensions
    {
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }
    }
}