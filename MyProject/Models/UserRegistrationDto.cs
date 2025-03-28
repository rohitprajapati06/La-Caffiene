using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class UserRegistrationDto
    {
        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;
    }
}
