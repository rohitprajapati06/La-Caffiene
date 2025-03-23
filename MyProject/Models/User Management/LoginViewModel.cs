using System.ComponentModel.DataAnnotations;

namespace MyProject.Models.User_Management
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }


        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
