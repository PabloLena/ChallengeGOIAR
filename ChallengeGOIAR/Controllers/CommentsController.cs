using ChallengeGOIAR.DTOs;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChallengeGOIAR.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {

        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public CommentsController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] CommentDTO commentDTO)
        {
            var post = await applicationDbContext.Posts.FirstOrDefaultAsync(x => x.Id == commentDTO.PostId);
            if (post == null)
            {
                return BadRequest();
            }
            if (commentDTO.Description.Length > 100)
            {
                commentDTO.Description = commentDTO.Description.Substring(0, 100);
            }
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);

            var comment = new Comment() { Description = commentDTO.Description, ApplicationUserId = user.Id, PostId = commentDTO.PostId };


            applicationDbContext.Add(comment);
            await applicationDbContext.SaveChangesAsync();

            return new CreatedAtRouteResult("getPost", new { id = comment.Id }, comment);
        }

    }
}
