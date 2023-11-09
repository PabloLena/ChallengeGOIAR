namespace ChallengeGOIAR.Entities
{
    public class Likes
    {
        public int Id { get; set; }
        public bool Like { get; set; }
        public string ApplicationUserId { get; set; }
        public int PostID { get; set; }
    }
}
