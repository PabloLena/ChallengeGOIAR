using ChallengeGOIAR.Controllers;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChallengeGOIAR.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ChallengeGOIAR.Test.UnitTest
{
    [TestClass]
    public class LikesControllerTest : TestBase
    {
        [TestMethod]
        public async Task CreateLike()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = BuildContext(dbName);
            var likesController = BuildLikesController(dbName);
            var claims = SetClaims();
            likesController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };

            context.Posts.Add(new Post() { Description = "post", Id = 1 });
            await context.SaveChangesAsync();
            var newLike = new LikeDTO() { PostID = 1 };

            var response = await likesController.Post(newLike);
            var result = response as CreatedResult;

            Assert.IsNotNull(result);

            var context2 = BuildContext(dbName);
            var count = await context2.Likes.CountAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task DeleteLikeNotExist() 
        {
            var dbName = Guid.NewGuid().ToString();

            var likesController = BuildLikesController(dbName);
            var claims = SetClaims();
            likesController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };

            var newLike = new LikeDTO() { PostID = 1 };
     
            var response = await likesController.Delete(newLike);
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);

        }

        [TestMethod]
        public async Task DeleteLike()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = BuildContext(dbName);
            var likesController = BuildLikesController(dbName);
            var claims = SetClaims();
            likesController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };
         
            context.Likes.Add(new Likes() { Like = true ,PostID=1, ApplicationUserId = "1"});

            await context.SaveChangesAsync();
           

            var newLike = new LikeDTO() { PostID = 1 };
            var respuesta = await likesController.Delete(newLike);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);

            var contexto3 = BuildContext(dbName);
            var existe = await contexto3.Likes.AnyAsync();
            Assert.IsFalse(existe);
        }


        private LikesController BuildLikesController(string dbName)
        {
            var context = BuildContext(dbName);
            var miUserStore = new UserStore<ApplicationUser>(context);

            var userManager = BuildUserManager(miUserStore);
            var newUser = new ApplicationUser();
            newUser.UserName = "Pablo";
            newUser.Id = "1";
            userManager.CreateAsync(newUser);
           
            return new LikesController(context, userManager);
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
