using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Required]
        public int PostId { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
    }
}
