using ChallengeGOIAR.Entities;

namespace ChallengeGOIAR.DTOs
{
    public class PostResponseDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ApplicationUserDTO User { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Likes> Likes { get; set; }
    }
}
