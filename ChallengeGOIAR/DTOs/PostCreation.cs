using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.DTOs
{
    public class PostCreation
    {
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
    }
}
