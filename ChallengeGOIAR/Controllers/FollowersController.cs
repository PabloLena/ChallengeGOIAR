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
    [Route("api/followers")]
    public class FollowersController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public FollowersController(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] FollowerDTO followerDTO)
        {
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);

            if (followerDTO.FollowerId == user.Id) 
            {
                return BadRequest();
            }

            var exists = await applicationDbContext.Users.AnyAsync(x => x.Id == followerDTO.FollowerId);

            if (!exists)
            {
                return NotFound();
            }

            var existsFollow = await applicationDbContext.Followers.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id
                                                                        && x.FollowerId == followerDTO.FollowerId);
            if (existsFollow != null)
            {
                return StatusCode(409, $"Already Followed.");
            }

            var follow = new Follower() { ApplicationUserId = user.Id, FollowerId = followerDTO.FollowerId };

            applicationDbContext.Add(follow);
            await applicationDbContext.SaveChangesAsync();

            var followerResponse = new FollowerResponseDTO() { UserId = follow.FollowerId, FollowerId = follow.FollowerId };

            return new CreatedResult("getFollower",  followerResponse);

        }

        [HttpDelete]
        [Route("Unfollow")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete([FromBody] FollowerDTO followerDTO)
        {
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await userManager.FindByNameAsync(currentUserName.Value);

            var follow = await applicationDbContext.Followers.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id
                                                                        && x.FollowerId == followerDTO.FollowerId);
            if (follow == null)
            {
                return NotFound();
            }
            else
            {
                applicationDbContext.Remove(follow);
                await applicationDbContext.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
