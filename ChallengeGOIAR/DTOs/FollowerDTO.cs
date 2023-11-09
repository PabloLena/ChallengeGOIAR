using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.DTOs
{
    public class FollowerDTO
    {
        [Required]
        public string FollowerId { get; set; }
    }
}
