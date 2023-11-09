using Microsoft.AspNetCore.Identity;

namespace ChallengeGOIAR.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreationDate { get; set; }

        public List<Follower> Followers { get; set; }
    }
}
