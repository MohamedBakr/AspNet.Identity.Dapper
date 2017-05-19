using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace AspNet.Identity.Dapper
{
    class UserTable<TUser> where TUser : class, IDapperIdentity<TUser>
    {
        private DbConnection _database;

        public UserTable(DbConnection database)
        {
            _database = database;
        }


        public TUser GetUserById(string userId)
        {
            TUser user = null;
            user = _database.Get<TUser>(userId);

            return user;
        }

        public TUser GetUserByName(string userName)
        {
            var result = _database.GetList<TUser>(new { UserName = userName });
            if (result.Count() > 0)
                return result.First();
            else
                return null;
        }

        public TUser GetUserByEmail(string email)
        {
            var result = _database.GetList<TUser>(new { Email = email });
            if (result.Count() > 0)
                return result.First();
            else
                return null;
        }

        public string GetPasswordHash(string userId)
        {
            string pwHash;

            var user = GetUserById(userId);
            pwHash = user.PasswordHash;

            return pwHash;
        }

        public string Insert(TUser user)
        {
            _database.Insert<string>(user);
            return user.Id;
        }


        public bool Delete(TUser user)
        {
            return _database.Delete<TUser>(user) > 0;
        }

        public bool Update(TUser user)
        {
            return _database.Update(user) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TUser> Users => _database.GetList<TUser>();
    }
}