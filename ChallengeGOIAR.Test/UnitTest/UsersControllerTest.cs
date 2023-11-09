using ChallengeGOIAR.Controllers;
using ChallengeGOIAR.DTOs;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChallengeGOIAR.Test.UnitTest
{
    [TestClass]
    public class UsersControllerTest: TestBase
    {
        [TestMethod]
        public async Task RegisterUser() 
        {
            var dbName = Guid.NewGuid().ToString();
            await CreateUserHelper(dbName);
            var context2 = BuildContext(dbName);
            var count = await context2.Users.CountAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task UserCantLogin()
        {
            var dbName = Guid.NewGuid().ToString();
            await CreateUserHelper(dbName);

            var controller = BuildUsersController(dbName);
            var user = new UserCredentials() { NickName = "prueba", Password = "mal" };
            var response = await controller.Login(user);
            
            Assert.IsNull(response.Value); 

        }
        [TestMethod]
        public async Task UserCanLogin()
        {
            var dbName = Guid.NewGuid().ToString();
            await CreateUserHelper(dbName);

            var controller = BuildUsersController(dbName);
            var user = new UserCredentials() { NickName = "prueba", Password = "aA123!" };
            var response = await controller.Login(user);

            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);

        }
        private async Task CreateUserHelper(string dbName)
        {
            var usersController = BuildUsersController(dbName);
            var user = new UserCredentials() { NickName = "prueba", Password = "aA123!" };
            await usersController.Register(user);
        }


        private UsersController BuildUsersController(string dbName)
        {
            var context = BuildContext(dbName);
            var miUserStore = new UserStore<ApplicationUser>(context);
            var userManager = BuildUserManager(miUserStore);
     

            var httpContext = new DefaultHttpContext();
            MockAuth(httpContext);
            var signInManager = SetupSignInManager(userManager, httpContext);

            var miConfiguracion = new Dictionary<string, string>
            {
                {"JWTkey", "asfjhiue2whiurhjiqehfeiheqifhiewhnvcndsvabnskjbnafjnsjkshjfkjahjskfjv" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(miConfiguracion)
                .Build();

            return new UsersController(userManager, configuration, signInManager, context);
        }

        private Mock<IAuthenticationService> MockAuth(HttpContext context)
        {
            var auth = new Mock<IAuthenticationService>();
            context.RequestServices = new ServiceCollection().AddSingleton(auth.Object).BuildServiceProvider();
            return auth;
        }

        private UserManager<TUser> BuildUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;

            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<TUser>>();

            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());

            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);

            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        private static SignInManager<TUser> SetupSignInManager<TUser>(UserManager<TUser> manager,
            HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null,
            IAuthenticationSchemeProvider schemeProvider = null) where TUser : class
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context);
            identityOptions = identityOptions ?? new IdentityOptions();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(identityOptions);
            var claimsFactory = new UserClaimsPrincipalFactory<TUser>(manager, options.Object);
            schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
            var sm = new SignInManager<TUser>(manager, contextAccessor.Object, claimsFactory, options.Object, null, schemeProvider, new DefaultUserConfirmation<TUser>());
            sm.Logger = logger ?? (new Mock<ILogger<SignInManager<TUser>>>()).Object;
            return sm;
        }
    }
}
