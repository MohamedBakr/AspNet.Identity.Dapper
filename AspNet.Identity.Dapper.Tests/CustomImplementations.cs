using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using AspNet.Identity.Dapper;
using Dapper;

namespace AspNet.Identity.Dapper.Tests
{
    [Table("AspNetUsers")]
    public class MyCustomUser : IDapperIdentity<MyCustomUser>
    {
        public MyCustomUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        public MyCustomUser(string userName)
            : this()
        {
            UserName = userName;
        }

        [Editable(true)][Required]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string Comment { get; set; }
        public int Age { get; set; }
        public string Title { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<MyCustomUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }


    public class MyCustomRole : IRole
    {
        public MyCustomRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public MyCustomRole(string name) : this()
        {
            Name = name;
        }

        public MyCustomRole(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
