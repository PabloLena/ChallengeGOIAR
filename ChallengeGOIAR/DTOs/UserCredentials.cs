using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChallengeGOIAR.DTOs
{
    public class UserCredentials
    {
        [Required]
        [StringLength(50)]
        public string NickName { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }
    }
}
