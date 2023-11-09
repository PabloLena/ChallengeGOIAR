using ChallengeGOIAR.DTOs;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace ChallengeGOIAR.Controllers
{
    [ApiController]
    [Route("api/likes")]
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public LikesController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] LikeDTO likeDTO) {
            var post = await applicationDbContext.Posts.FirstOrDefaultAsync(x => x.Id == likeDTO.PostID);
            if (post == null)
            {
                return BadRequest();
            }
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);

            var existsLike = await applicationDbContext.Likes.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id
                                                                        && x.PostID == likeDTO.PostID);
            if (existsLike != null) {
                return StatusCode(409, $"Already liked.");
            }

            var like = new Likes() { ApplicationUserId = user.Id, Like = true, PostID = likeDTO.PostID };
            applicationDbContext.Add(like);
            await applicationDbContext.SaveChangesAsync();

            return new CreatedResult("getLike", new { Id = like.Id });
        }

        [HttpDelete]
        [Route("Unlike")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] LikeDTO likeDTO) 
        {
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);

            var like = await applicationDbContext.Likes.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id
                                                                        && x.PostID == likeDTO.PostID);
            if (like == null)
            {
                return NotFound();
            }
            else
            {
                applicationDbContext.Remove(like);
                await applicationDbContext.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
