using System;
using Microsoft.AspNet.Identity;
using Dapper;

namespace AspNet.Identity.Dapper
{
    [Table("AspNetRoles")]
    public class IdentityRole : IRole
    {
        public IdentityRole()
        {
            //Id = Guid.NewGuid().ToString();
        }

        public IdentityRole(string name) : this()
        {
            Name = name;
        }

        public IdentityRole(string name, string id)
        {
            Name = name;
            Id = id;
        }
        [Key][Required]
        public string Id { get; set; }
        
        public string Name { get; set; }
    }
}
