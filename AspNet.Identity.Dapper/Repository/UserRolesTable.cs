using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;

namespace AspNet.Identity.Dapper
{
    public class UserRolesTable<TUser> where TUser: class, IDapperIdentity<TUser>
    {
        private DbConnection _database;
        
        public UserRolesTable(DbConnection database)
        {
            _database = database;
        }

        public List<string> FindByUserId(string userId)
        {
            var ret = new List<string>();
            var sql = "Select AspNetRoles.Name from AspNetUserRoles, AspNetRoles where AspNetUserRoles.UserId = @userId and AspNetUserRoles.RoleId = AspNetRoles.Id";
            return _database.Query<string>(sql, new { userId }).AsList<string>();
        }

        public int Delete(string userId)
        {
            string sql = "Delete from AspNetUserRoles where UserId = @userId";
            _database.Query(sql, new { userId });
            return 1;
        }

        public int Insert(TUser user, string roleId)
        {
            string sql = "Insert into AspNetUserRoles (UserId, RoleId) values (@UserId, @RoleId)";
            _database.Query(sql, new { UserId = user.Id, RoleId = roleId });
            return 1;
        }

        public int RemoveUserFromRole(TUser user, string roleName)
        {
            string sql = @"DELETE u FROM AspNetUserRoles u
                                                Inner join AspNetRoles r
                                                on u.RoleId = r.Id
                                                where r.Name = @RoleName and u.userid = @UserId";

            _database.Query(sql, new { RoleName = roleName, UserId = user.Id });
            return 1;
        }
    }
}
