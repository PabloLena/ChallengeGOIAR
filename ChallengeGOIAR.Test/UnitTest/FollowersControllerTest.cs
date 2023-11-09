using ChallengeGOIAR.Controllers;
using ChallengeGOIAR.DTOs;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChallengeGOIAR.Test.UnitTest
{
    [TestClass]
    public class FollowersControllerTest : TestBase
    {
        [TestMethod]
        public async Task CreateLike()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = BuildContext(dbName);
            var followersController = BuildFollowersController(dbName);
            var claims = SetClaims();
            followersController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };       

            var newFollow = new FollowerDTO() { FollowerId = "2" };

            var response = await followersController.Post(newFollow);
            var result = response as CreatedResult;

            Assert.IsNotNull(result);

            var context2 = BuildContext(dbName);
            var count = await context2.Followers.CountAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task DeleteLikeNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            var followersController = BuildFollowersController(dbName);
            var claims = SetClaims();
            followersController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };

            var newFollow = new FollowerDTO() { FollowerId = "1" };

            var response = await followersController.Delete(newFollow);
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);

        }

        [TestMethod]
        public async Task DeleteLike()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = BuildContext(dbName);
            var followersController = BuildFollowersController(dbName);
            var claims = SetClaims();
            followersController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };

            context.Followers.Add(new Follower() { FollowerId = "2", ApplicationUserId = "1" });

            await context.SaveChangesAsync();

            var newFollow = new FollowerDTO() { FollowerId = "2" };
            var respuesta = await followersController.Delete(newFollow);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);

            var contexto3 = BuildContext(dbName);
            var existe = await contexto3.Followers.AnyAsync();
            Assert.IsFalse(existe);
        }


        private FollowersController BuildFollowersController(string dbName)
        {
            var context = BuildContext(dbName);
            var miUserStore = new UserStore<ApplicationUser>(context);

            var userManager = BuildUserManager(miUserStore);
            var newUser = new ApplicationUser();
            newUser.UserName = "Pablo";
            newUser.Id = "1";
            userManager.CreateAsync(newUser);

            var newUser2 = new ApplicationUser();
            newUser2.UserName = "Pepe";
            newUser2.Id = "2";
            userManager.CreateAsync(newUser2);

            return new FollowersController(context, userManager);
        }
        private DefaultHttpContext SetClaims()
        {
            var context = new DefaultHttpContext();

            var claims = new List<Claim>
                {
                    new Claim("UserName", "Pablo"),
                };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            context.User = claimsPrincipal;
            return context;
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
    }
}
