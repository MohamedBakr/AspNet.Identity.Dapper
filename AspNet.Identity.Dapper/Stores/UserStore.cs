using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Data.Common;

namespace AspNet.Identity.Dapper
{
    public class UserStore<TUser, TRole> : IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,        
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserTwoFactorStore<TUser, string>,
        IUserLockoutStore<TUser, string>,
        IUserStore<TUser>
        where TUser : class, IDapperIdentity<TUser>
        where TRole : IRole 

    {
        private UserTable<TUser> userTable;
        private RoleTable<TRole> roleTable;
        private UserRolesTable<TUser> userRolesTable;
        private UserClaimsTable<TUser> userClaimsTable;
        private UserLoginsTable<TUser> userLoginsTable;
        public DbConnection Database { get; private set; }

        public UserStore(DbConnection database)
        {
            Database = database;
            userTable = new UserTable<TUser>(database);
            roleTable = new RoleTable<TRole>(database);
            userRolesTable = new UserRolesTable<TUser>(database);
            userClaimsTable = new UserClaimsTable<TUser>(database);
            userLoginsTable = new UserLoginsTable<TUser>(database);
        }

        public IEnumerable<TUser>  Users => userTable.Users;

        public Task AddClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("user");
            }

            userClaimsTable.Insert(user, claim);

            return Task.FromResult<object>(null);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            userLoginsTable.Insert(user, login);

            return Task.FromResult<object>(null);
        }

        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("Argument cannot be null or empty: roleName.");
            }

            var identRole = roleTable.GetRoleByName(roleName);
            if (!string.IsNullOrEmpty(identRole.Id))
            {
                userRolesTable.Insert(user, identRole.Id);
            }

            return Task.FromResult<object>(null);
        }

        public Task CreateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            userTable.Insert(user);

            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(TUser user)
        {
            if (user != null)
            {
                userTable.Delete(user);
            }

            return Task.FromResult<Object>(null);
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            var userId = userLoginsTable.FindUserIdbyLogin(login);
            if (userId != null)
            {
                TUser user = userTable.GetUserById(userId) as TUser;
                if (user != null)
                {
                    return Task.FromResult<TUser>(user);
                }
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<TUser> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Null or empty argument: email");
            }

            TUser user = userTable.GetUserByEmail(email);
            return Task.FromResult<TUser>(user);
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("Null or empty argument: userId");
            }

            TUser result = userTable.GetUserById(userId) as TUser;
            if (result != null)
            {
                return Task.FromResult<TUser>(result);
            }

            return Task.FromResult<TUser>(null);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Null or empty argument: userName");
            }

            var result = userTable.GetUserByName(userName);

            return Task.FromResult<TUser>(result);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            var claims = userClaimsTable.FindByUserId(user.Id);

            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            return Task.FromResult(user.LockoutEndDateUtc.HasValue
                              ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                              : new DateTimeOffset());
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {            
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            List<UserLoginInfo> logins = userLoginsTable.FindAllByUserId(user.Id);
            if (logins != null)
            {
                return Task.FromResult<IList<UserLoginInfo>>(logins);
            }

            return Task.FromResult<IList<UserLoginInfo>>(null);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            string passwordHash = userTable.GetPasswordHash(user.Id);

            return Task.FromResult<string>(passwordHash);
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            List<string> roles = userRolesTable.FindByUserId(user.Id);
            {
                if (roles != null)
                {
                    return Task.FromResult<IList<string>>(roles);
                }
            }

            return Task.FromResult<IList<string>>(null);
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            var hasPassword = !string.IsNullOrEmpty(userTable.GetPasswordHash(user.Id));

            return Task.FromResult<bool>(Boolean.Parse(hasPassword.ToString()));
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount++;
            userTable.Update(user);

            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("role");
            }

            List<string> roles = userRolesTable.FindByUserId(user.Id);
            {
                if (roles != null && roles.Contains(roleName))
                {
                    return Task.FromResult<bool>(true);
                }
            }

            return Task.FromResult<bool>(false);
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            userClaimsTable.DeleteSingleclaim(user, claim);

            return Task.FromResult<object>(null);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            userRolesTable.RemoveUserFromRole(user, roleName);
            return Task.FromResult<object>(null);
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            userLoginsTable.Delete(user, login);

            return Task.FromResult<Object>(null);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            user.AccessFailedCount = 0;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            user.Email = email;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            userTable.Update(user);
            return Task.FromResult<Object>(null);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            user.PhoneNumberConfirmed = confirmed;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            userTable.Update(user);
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            userTable.Update(user);

            return Task.FromResult(0);
        }

        public Task UpdateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            userTable.Update(user);

            return Task.FromResult<object>(null);
        }


        public void Dispose()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

    }
}
