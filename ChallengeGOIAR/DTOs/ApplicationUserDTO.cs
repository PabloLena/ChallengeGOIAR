using ChallengeGOIAR.Entities;

namespace ChallengeGOIAR.DTOs
{
    public class ApplicationUserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Follower> Followers { get; set; }
    }
}
