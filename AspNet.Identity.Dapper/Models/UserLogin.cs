using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AspNet.Identity.Dapper
{
    [Table("AspNetUserLogins")]
    public class UserLogin
    {
        [Key]
        public string LoginProvider { get; set; }
        [Key]
        public string ProviderKey { get; set; }
        [Key]
        public string UserId { get; set; }
    }
}
