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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChallengeGOIAR.Test.UnitTest
{
    [TestClass]
    public class PostsControllerTest : TestBase
    {
        [TestMethod]
        public async Task GetPosts()
        {
            //Arrange
            var dbName = Guid.NewGuid().ToString();
            var context2 = BuildContext(dbName);
            var postController = BuildPostsController(dbName);

            context2.Posts.Add(new Post() { Description = "Post 1" });
            context2.Posts.Add(new Post() { Description = "Post 2" });

            await context2.SaveChangesAsync();

            //Act             
            var response = await postController.Get();

            //Assert 
            var posts = response.Value;
            Assert.AreEqual(2, posts.Count);
        }

        [TestMethod]
        public async Task GetPostByIdNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            var postController = BuildPostsController(dbName);

            var response = await postController.Get(1);

            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        [TestMethod]
        public async Task GetPostById()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = BuildContext(dbName);
            var postController = BuildPostsController(dbName);

            context.Posts.Add(new Post() { Description = "Post 1" });
            context.Posts.Add(new Post() { Description = "Post 2" });
            await context.SaveChangesAsync();

            var id = 1;
            var response = await postController.Get(id);
            var result = response.Value;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public async Task CreatePost()
        {
            var dbName = Guid.NewGuid().ToString();

            var postController = BuildPostsController(dbName);
            var claims = SetClaims();
            postController.ControllerContext = new ControllerContext
            {
                HttpContext = claims
            };            

            var newPost = new PostCreation() { Description = "Nuevo Post" };

            var response = await postController.Post(newPost);
            var result = response as CreatedAtRouteResult;

            Assert.IsNotNull(result);

            var context = BuildContext(dbName);
            var count = await context.Posts.CountAsync();
            Assert.AreEqual(1, count);
        }

        private PostsController BuildPostsController(string dbName)
        {
            var context = BuildContext(dbName);
            var miUserStore = new UserStore<ApplicationUser>(context);

            var userManager = BuildUserManager(miUserStore);
            var newUser = new ApplicationUser();
            newUser.UserName = "Pablo";
            userManager.CreateAsync(newUser);

            return new PostsController(context, userManager);
        }
        private DefaultHttpContext SetClaims() {
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
