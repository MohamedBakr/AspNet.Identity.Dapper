using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.Dapper
{
    public interface IDapperIdentity<T> : IUser where T : class, IUser<string>
    {
         string Email { get; set; }
         bool EmailConfirmed { get; set; }
         string PasswordHash { get; set; }
         string SecurityStamp { get; set; }
         string PhoneNumber { get; set; }
         bool PhoneNumberConfirmed { get; set; }
         bool TwoFactorEnabled { get; set; }
         DateTime? LockoutEndDateUtc { get; set; }
         bool LockoutEnabled { get; set; }
         int AccessFailedCount { get; set; }
         Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<T> manager);
    }
}
