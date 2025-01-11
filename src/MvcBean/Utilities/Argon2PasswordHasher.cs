using Microsoft.AspNetCore.Identity;
using Sodium;

namespace MvcBean.Utilities
{
    public class Argon2PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        public string HashPassword(TUser user, string password)
        {
            return PasswordHash.ArgonHashString(password, PasswordHash.StrengthArgon.Moderate);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            bool isMatch = PasswordHash.ArgonHashStringVerify(hashedPassword, providedPassword);

            return isMatch
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}