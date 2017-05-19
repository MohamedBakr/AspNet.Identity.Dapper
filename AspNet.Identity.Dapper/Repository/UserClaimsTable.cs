using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AspNet.Identity.Dapper
{

    class UserClaimsTable<TUser> where TUser : class, IDapperIdentity<TUser>
    {
        private DbConnection _database;

        public UserClaimsTable(DbConnection database)
        {
            _database = database;
        }

        public IList<Claim> FindByUserId(string userId)
        {
            var userClaims = _database.GetList<UserClaim>(new { UserId = userId });
            return userClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public int DeleteSingleclaim(TUser user, Claim claim)
        {
            _database.DeleteList<UserClaim>("Where UserId = @UserId AND  ClaimValue= @ClaimValue AND ClaimType = @ClaimType", new { UserId = user.Id, ClaimValue = claim.Value, ClaimType = claim.Type });
            return 1;
        }

        public int Insert(TUser user, Claim claim)
        {
            var newUserClaim = new UserClaim()
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                UserId = user.Id
            };

            return _database.Insert<int>(newUserClaim);
        }
    }
}
