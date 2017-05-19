using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Data.Common;
using Dapper;

namespace AspNet.Identity.Dapper
{
    public class UserLoginsTable<TUser> where TUser: class, IDapperIdentity<TUser>
    {
        private DbConnection _database;

        public UserLoginsTable(DbConnection database)
        {
            _database = database;
        }
        public int Delete(TUser user, UserLoginInfo login)
        {
            return _database.Delete(new UserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                UserId = user.Id
            });
        }

        public string Insert(TUser user, UserLoginInfo login)
        {
            var userlogin = new UserLogin { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey, UserId = user.Id };
             _database.Query("INSERT INTO AspNetUserLogins Values(@LoginProvider,@ProviderKey,@UserId);", userlogin);
            return string.Empty;
        }

        public string FindUserIdbyLogin(UserLoginInfo login)
        {
            string ret = null;
            var lgi = _database.GetList<UserLogin>(new
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey
            });

            if (lgi.Count()> 0)
            {
                ret = lgi.First().UserId;
            }

            return ret;
        }

        public List<UserLoginInfo> FindAllByUserId(string userId)
        {
            List<UserLoginInfo> ret = new List<UserLoginInfo>();
            var lst = _database.GetList<UserLogin>(new { UserId = userId });
            ret.AddRange(lst.Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)));

            return ret;
        }
    }
}
