using System;
using Xunit;
using System.Data.Common;
using AspNet.Identity.Dapper;

namespace AspNet.Identity.Dapper.Tests
{
    public class DatabaseFixture : IDisposable
    {

        readonly string connString = "server = localhost\\sqlexpress;initial catalog = aspnet_NPoco_Identiy_Provider; persist security info=True;Integrated Security = SSPI;";        
        public UserStore<IdentityUser, IdentityRole> UserStore { get; private set; }
        public RoleStore<IdentityRole> RoleStore { get; private set; }
        public UserStore<MyCustomUser, MyCustomRole> UserStoreCustom { get; private set; }
        public RoleStore<MyCustomRole> RoleStoreCustom { get; private set; }

        // used for setup
        public DbConnection RawDB { get; private set; }

        public DatabaseFixture()
        {
            var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=F:\Projects\Database\Sample.mdf;Integrated Security=True;Connect Timeout=30");
            UserStore = new UserStore<IdentityUser, IdentityRole>(connection);
            RoleStore = new RoleStore<IdentityRole>(connection);

            UserStoreCustom = new UserStore<MyCustomUser, MyCustomRole>(connection);
            RoleStoreCustom = new RoleStore<MyCustomRole>(connection);

            RawDB =connection;
        }

        public void Dispose()
        {
            UserStore.Dispose();
            RoleStore.Dispose();
            UserStoreCustom.Dispose();
            RoleStoreCustom.Dispose();
            RawDB.Dispose();
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
