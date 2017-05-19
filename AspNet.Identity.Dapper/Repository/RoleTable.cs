using System.Collections.Generic;
using Dapper;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.Dapper
{
    public class RoleTable<TRole> 
    {
        private DbConnection _database;

        public RoleTable(DbConnection database)
        {
            _database = database;
        }

        //public bool Delete(string roleId)
        //{
        //    var role = new TRole();
        //    role.Id = roleId;
        //    return _database.Delete(role) > 0;
        //}

        public bool Delete(TRole role)
        {

                string sql = "Delete from AspNetRoles where Name = @Name";
                _database.Query(sql, role);
                return true;
            
        }

        public string Insert(TRole role)
        {
            return _database.Insert<string>(role);
        }

        public TRole GetRoleById(string roledId)
        {
            return _database.Get<TRole>(roledId);
        }

        public TRole GetRoleByName(string roleName)
        {
            var result = _database.GetList<TRole>(new { Name = roleName });
            
                return result.FirstOrDefault();
            
        }

        public bool Update(TRole role)
        {
            return _database.Update(role) > 0;
        }

        public IEnumerable<TRole> Roles => _database.GetList<TRole>();
    }
}
