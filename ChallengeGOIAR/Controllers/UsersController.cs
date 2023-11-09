using ChallengeGOIAR.DTOs;
using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChallengeGOIAR.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext applicationDbContext;

        public UsersController(UserManager<ApplicationUser> userManager, 
            IConfiguration configuration, 
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.applicationDbContext = applicationDbContext;
        }

        [HttpGet()]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ApplicationUserDTO>> Get()
        {
            var currentUserName = User.Claims.FirstOrDefault(x => x.Type == "UserName");
            ApplicationUser user = await applicationDbContext.Users
                .Include(f => f.Followers)
                .FirstOrDefaultAsync(x => x.UserName == currentUserName.Value);

            var userResponse = new ApplicationUserDTO() {
                Id= user.Id,
                Followers = user.Followers,
                CreationDate = user.CreationDate,
                UserName = user.UserName
            };


            return userResponse;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponse>> Register(UserCredentials userCredentials) 
        {
            var user = new ApplicationUser { UserName = userCredentials.NickName , CreationDate = DateTime.Now};
            var result = await userManager.CreateAsync(user, userCredentials.Password);

            if (result.Succeeded)
            {
                return BuildToken(userCredentials);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login(UserCredentials userCredentials)
        {
            var result = await signInManager.PasswordSignInAsync(userCredentials.NickName, userCredentials.Password,
                isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded) 
            { 
                return BuildToken(userCredentials);
            }
            else
            {
                return BadRequest("Login Fail");
            }
        }

        private AuthenticationResponse BuildToken(UserCredentials userCredentials) 
        {
            var claims = new List<Claim>()
            {
                new Claim("UserName", userCredentials.NickName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTkey"]));
                                                                                                 
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: creds);

            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiracion
            };

        }
    }
}
