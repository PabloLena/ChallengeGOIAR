using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.DTOs
{
    public class CommentDTO
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public int PostId { get; set; }
    }
}
