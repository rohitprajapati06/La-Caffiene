using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class OtpVerificationDto
    {
        [Required(ErrorMessage = "OTP is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a 6-digit number")]
        public string OtpInput { get; set; }
    }
}
