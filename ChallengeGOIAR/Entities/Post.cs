using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.Entities
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
        public string ApplicationUserId { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Likes> Likes { get; set; }

    }
}
