using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ChallengeGOIAR.DTOs;

namespace ChallengeGOIAR.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public PostsController(ApplicationDbContext applicationDbContext,
                               UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;        
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<Post>>> Get() 
        {
            return await applicationDbContext.Posts
                .Include(c => c.Comments)
                .Include(l => l.Likes)
                .ToListAsync(); 
        }

        [HttpGet("{id:int}", Name = "getPost")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Post>> Get(int id)
        {
            var posts = await applicationDbContext.Posts
                .Include(c => c.Comments)
                .Include(l => l.Likes)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (posts == null) 
            {
                return NotFound();           
            }
            return posts;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] PostCreation creationPost)
        {

            var currentUserName =  User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);
            
            var post = new Post() { Description = creationPost.Description, ApplicationUserId = user.Id };
             applicationDbContext.Add(post);
             await applicationDbContext.SaveChangesAsync();

            return new CreatedAtRouteResult("getPost", new { id = post.Id }, post);
        }
    }
}
