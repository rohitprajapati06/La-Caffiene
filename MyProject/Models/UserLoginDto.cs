using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
