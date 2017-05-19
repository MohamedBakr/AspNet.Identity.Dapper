using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Xunit;
using Xunit.Abstractions;
using Dapper;


namespace AspNet.Identity.Dapper.Tests
{
    [Collection("Database collection")]
    public class UserStoreTests : IDisposable
    {
        private readonly DatabaseFixture fixture;
        ITestOutputHelper output;
        private IdentityUser foundUser;

        public UserStoreTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;
            Setup();
        }

        [Fact]
        public async Task IUserStoreTests()
        {
            var defaultUser = new IdentityUser("TestUserName1");
            await IUserStoreGeneric<IdentityUser, IdentityRole>(defaultUser, fixture.UserStore);

            // Just in case, clean up any cols
            CleanUpCustomCols(); 
            // Add new columns to default schema
            fixture.RawDB.Query(@"ALTER TABLE AspNetUsers
                                    ADD Comment NVARCHAR(max),
                                    Title VARCHAR(256),
                                    Age INT");


            var myUser = new MyCustomUser("TestUserName1");
            await IUserStoreGeneric<MyCustomUser, MyCustomRole>(myUser, fixture.UserStoreCustom);

            // always clean up after test
            CleanUpCustomCols();

        }

        public async Task IUserStoreGeneric<T,A>(T TUser, UserStore<T, A> Store ) where T : class, IDapperIdentity<T> where A : IRole
        {
            // Create
            var u = TUser;
                       
            output.WriteLine("Creating using TestUserName1");
            await Store.CreateAsync(u);

            // Get
            var fetchedUser =  await fixture.UserStore.FindByNameAsync("TestUserName1");

            if (fetchedUser != null)
            {
                Assert.Equal(fetchedUser.UserName, "TestUserName1");
            }
            else
            {
                Assert.NotEqual(fetchedUser, null);
            }
            
            // Update
            fetchedUser.UserName = "ModifiedUserName1";
            await fixture.UserStore.UpdateAsync(fetchedUser);

            var moduser = await fixture.UserStore.FindByNameAsync("ModifiedUserName1");

            if (moduser != null)
            {
                Assert.Equal(moduser.UserName, "ModifiedUserName1");
            }
            else
            {
                Assert.NotEqual(moduser, null);
            }

            var foundByName = await fixture.UserStore.FindByIdAsync(u.Id.ToString());

            if (foundByName != null)
            {
                Assert.Equal(foundByName.UserName, "ModifiedUserName1");
            }
            else
            {
                Assert.NotEqual(foundByName, null);
            }

            await fixture.UserStore.DeleteAsync(foundByName);

            var foundDeleted = await fixture.UserStore.FindByIdAsync(u.Id.ToString());

            Assert.Equal(foundDeleted, null);
        }

        [Fact]
        // Does not test the inherited IUserStoreMembers
        public async Task IUserLoginStoreTests()
        {
            // add a test IUserLoginStoreTestUser
            IdentityUser u = new IdentityUser("TestUserName1");
            u.Id = Guid.NewGuid().ToString();
            output.WriteLine("Creating using IUserLoginStoreTestUser");
            await fixture.UserStore.CreateAsync(u);

            var testLoginInfo = new UserLoginInfo("MyOauthProvider", Guid.NewGuid().ToString());
            await fixture.UserStore.AddLoginAsync(u,testLoginInfo);
         
            foundUser = await fixture.UserStore.FindAsync(testLoginInfo);
            Assert.Equal(u.Id, foundUser.Id);

            // add a second login
            var secondLoginInfo = new UserLoginInfo("FaceBookProvider", Guid.NewGuid().ToString());
            await fixture.UserStore.AddLoginAsync(u, secondLoginInfo);

            // get them all
            var allLogins = fixture.UserStore.GetLoginsAsync(u);
         
            Assert.Contains(testLoginInfo, allLogins.Result, new UserLoginInfoComparer());
            Assert.Contains(secondLoginInfo, allLogins.Result, new UserLoginInfoComparer());

            // remove them
            await fixture.UserStore.RemoveLoginAsync(u, testLoginInfo);
            await fixture.UserStore.RemoveLoginAsync(u, secondLoginInfo);

            // see if there are any left
            var shouldBeEmpty = fixture.UserStore.GetLoginsAsync(u);
            Assert.Empty(shouldBeEmpty.Result);
            
            // delete the test user now           
            await fixture.UserStore.DeleteAsync(u);

            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("TestUserName1");
            Assert.Equal(shouldBeNullUser.Result, null);

        }

        [Fact]
        // Does not test the inherited IUserStoreMembers
        public async Task IUserClaimStoreTests()
        {
            // add a test IUserClaimStoreTest user
            IdentityUser u = new IdentityUser("IUserClaimStoreTest");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);

            var testClaims = new List<Claim>();

            var claim1 = new Claim("Test Claim Type 1", "Test Claim Value 1");
            await fixture.UserStore.AddClaimAsync(u, claim1);
            testClaims.Add(claim1);

            var claim2 = new Claim("Test Claim Type 2", "Test Claim Value 2");
            await fixture.UserStore.AddClaimAsync(u, claim2);
            testClaims.Add(claim2);
            
            var myClaims = await fixture.UserStore.GetClaimsAsync(u);
            Assert.Equal(myClaims.Count, testClaims.Count);

            // delete the claims
            await fixture.UserStore.RemoveClaimAsync(u, claim1);
            await fixture.UserStore.RemoveClaimAsync(u, claim2);

            var shouldBeEmpty = await fixture.UserStore.GetClaimsAsync(u);
            Assert.Equal(shouldBeEmpty.Count, 0);

            // delete the user
            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserClaimStoreTest");
            Assert.Equal(shouldBeNullUser.Result, null);

        }

        [Fact]
        // Does not test the inherited IUserStoreMembers
        public async Task IUserRoleStoreTests()
        {
            // add a test IUserClaimStoreTest user
            IdentityUser u = new IdentityUser("IUserRoleStoreTest");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);
            
            var r1 = new IdentityRole("Test Role 1",Guid.NewGuid().ToString());
            await fixture.RoleStore.CreateAsync(r1);

            var r2 = new IdentityRole("Test Role 2", Guid.NewGuid().ToString());
            await fixture.RoleStore.CreateAsync(r2);

            await fixture.UserStore.AddToRoleAsync(u, "Test Role 1");
            await fixture.UserStore.AddToRoleAsync(u, "Test Role 2");

            var roles = fixture.UserStore.GetRolesAsync(u);

            Assert.Equal(roles.Result.Count, 2);

            var isInRole1 = fixture.UserStore.IsInRoleAsync(u, "Test Role 1");
            Assert.Equal(isInRole1.Result, true);
            var isInRole2 = fixture.UserStore.IsInRoleAsync(u, "Test Role 2");
            Assert.Equal(isInRole2.Result, true);

            await fixture.UserStore.RemoveFromRoleAsync(u, "Test Role 1");
            await fixture.UserStore.RemoveFromRoleAsync(u, "Test Role 2");

            var notInRole1 = fixture.UserStore.IsInRoleAsync(u, "Test Role 1");
            Assert.Equal(notInRole1.Result, false);
            var notInRole2 = fixture.UserStore.IsInRoleAsync(u, "Test Role 2");
            Assert.Equal(notInRole2.Result, false);

            // delete the roles
            await fixture.RoleStore.DeleteAsync(r1);
            var r1Deleted = fixture.RoleStore.FindByNameAsync("Test Role 1");
            Assert.Equal(r1Deleted.Result, null);

            await fixture.RoleStore.DeleteAsync(r2);
            var r2Deleted = fixture.RoleStore.FindByNameAsync("Test Role 2");
            Assert.Equal(r2Deleted.Result, null);

            // delete the user
            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserRoleStoreTest");
            Assert.Equal(shouldBeNullUser.Result, null);

        }

        [Fact]
        public async Task IUserPasswordStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserPasswordStoreTest");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);
            PasswordHasher hasher = new PasswordHasher();
            await fixture.UserStore.SetPasswordHashAsync(u, hasher.HashPassword("secret_password!"));

            var hasHash = fixture.UserStore.HasPasswordAsync(u);
            Assert.Equal(hasHash.Result, true);

            var theHash = fixture.UserStore.GetPasswordHashAsync(u);
            Assert.Equal(hasher.VerifyHashedPassword(theHash.Result, "secret_password!"), PasswordVerificationResult.Success);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserPasswordStoreTest");
            Assert.Equal(shouldBeNullUser.Result, null);          
        }

        [Fact]
        public async Task IUserSecurityStampStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserSecurityStampStoreTests");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);

            var fakeStamp = Guid.NewGuid().ToString();
            await fixture.UserStore.SetSecurityStampAsync(u, fakeStamp);

            u = await fixture.UserStore.FindByIdAsync(u.Id);
            var userStamp = fixture.UserStore.GetSecurityStampAsync(u);
            Assert.Equal(userStamp.Result, fakeStamp);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserSecurityStampStoreTests");
            Assert.Equal(shouldBeNullUser.Result, null);
        }

        [Fact]
        public async Task IUserEmailStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserEmailStoreTests");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);

            await fixture.UserStore.SetEmailAsync(u, "IUserEmailStoreTests@somehost.local");
            var e1 = fixture.UserStore.GetEmailAsync(u);
            Assert.Equal(e1.Result, "IUserEmailStoreTests@somehost.local");

            var confirm = fixture.UserStore.GetEmailConfirmedAsync(u);
            Assert.Equal(confirm.Result, false);
            await fixture.UserStore.SetEmailConfirmedAsync(u, true);
            confirm = fixture.UserStore.GetEmailConfirmedAsync(u);
            Assert.Equal(confirm.Result, true);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserEmailStoreTests");
            Assert.Equal(shouldBeNullUser.Result, null);
        }

        [Fact]
        public async Task IUserPhoneNumberStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserPhoneNumberStoreTests");
            u.Id = Guid.NewGuid().ToString(); 
            await fixture.UserStore.CreateAsync(u);

            await fixture.UserStore.SetPhoneNumberAsync(u, "1-800-876-5309");
            var p1 = fixture.UserStore.GetPhoneNumberAsync(u);
            Assert.Equal(p1.Result, "1-800-876-5309");

            var confirm = fixture.UserStore.GetPhoneNumberConfirmedAsync(u);
            Assert.Equal(confirm.Result, false);
            await fixture.UserStore.SetPhoneNumberConfirmedAsync(u, true);
            confirm = fixture.UserStore.GetPhoneNumberConfirmedAsync(u);
            Assert.Equal(confirm.Result, true);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserPhoneNumberStoreTests");
            Assert.Equal(shouldBeNullUser.Result, null);
        }

        [Fact]
        public async Task IUserTwoFactorStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserTwoFactorStoreTests");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);

            var confirm = fixture.UserStore.GetTwoFactorEnabledAsync(u);
            Assert.Equal(confirm.Result, false);
            await fixture.UserStore.SetTwoFactorEnabledAsync(u, true);
            confirm = fixture.UserStore.GetTwoFactorEnabledAsync(u);
            Assert.Equal(confirm.Result, true);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserTwoFactorStoreTests");
            Assert.Equal(shouldBeNullUser.Result, null);
        }

        [Fact]
        public async Task IUserLockoutStoreTests()
        {
            IdentityUser u = new IdentityUser("IUserLockoutStoreTests");
            u.Id = Guid.NewGuid().ToString();
            await fixture.UserStore.CreateAsync(u);

            var count = fixture.UserStore.GetAccessFailedCountAsync(u);
            Assert.Equal(count.Result, 0);
            await fixture.UserStore.IncrementAccessFailedCountAsync(u);
            count = fixture.UserStore.GetAccessFailedCountAsync(u);
            Assert.Equal(count.Result, 1);
            await fixture.UserStore.ResetAccessFailedCountAsync(u);
            count = fixture.UserStore.GetAccessFailedCountAsync(u);
            Assert.Equal(count.Result, 0);

            var confirm = fixture.UserStore.GetLockoutEnabledAsync(u);
            Assert.Equal(confirm.Result, false);
            await fixture.UserStore.SetLockoutEnabledAsync(u, true);
            confirm = fixture.UserStore.GetLockoutEnabledAsync(u);
            Assert.Equal(confirm.Result, true);

            var lockoutDate = DateTimeOffset.UtcNow.AddMinutes(90);
            await fixture.UserStore.SetLockoutEndDateAsync(u, lockoutDate);
            var dbLockoutDate = fixture.UserStore.GetLockoutEndDateAsync(u);
            Assert.Equal(lockoutDate, dbLockoutDate.Result);

            await fixture.UserStore.DeleteAsync(u);
            var shouldBeNullUser = fixture.UserStore.FindByNameAsync("IUserLockoutStoreTests");
            Assert.Equal(shouldBeNullUser.Result, null);
        }

        [Fact]
        public async void IQueryProviderWithIncludesTest()
        {
            for (var x = 0; x <= 50; x++)
            {
                IdentityUser u = new IdentityUser("Q_User_Test" + x);
                u.Id = Guid.NewGuid().ToString();
                await fixture.UserStore.CreateAsync(u);
            }

            var iqUsers = fixture.UserStore.Users;
            var o = iqUsers.Where(x => x.UserName.EndsWith("3"));
            var threeList = o.ToList();

            Assert.Equal(5, threeList.Count);

            var iqUsers2 = fixture.UserStore.Users;
            var a = iqUsers2.Where(x => x.UserName.Contains("0"));
            var zeroList = a.ToList();

            Assert.Equal(6, zeroList.Count);
        }

        private void Setup()
        {
            
            fixture.RawDB.Execute(@"DELETE FROM AspNetUsers
                                            DELETE FROM AspNetUserLogins
                                            DELETE FROM AspNetUserRoles
                                            DELETE FROM AspNetUserClaims
                                            DELETE FROM AspNetRoles");


        }

        private void CleanUpCustomCols()
        {
            DropColumn("Comment");
            DropColumn("Title");
            DropColumn("Age");
        }

        private void DropColumn(string colName)
        {
            string s = string.Format(@"IF EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'{0}' AND Object_ID = Object_ID(N'AspNetUsers'))
            BEGIN
                Alter Table AspNetUsers
                Drop Column {0}
            END", colName);

            fixture.RawDB.Execute(s);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class UserLoginInfoComparer : IEqualityComparer<UserLoginInfo>
    {
        public bool Equals(UserLoginInfo x, UserLoginInfo y)
        {
            return x.LoginProvider == y.LoginProvider && x.ProviderKey == y.ProviderKey;
        }

        public int GetHashCode(UserLoginInfo obj)
        {
            return obj.GetHashCode();
        }
    }

}

