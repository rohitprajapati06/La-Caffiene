using System.ComponentModel.DataAnnotations;


namespace MyProject.Models
{
    public class BookingDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Number of persons must be between 1 and 20.")]
        public int NoOfPerson { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public long Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Mail { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Date and time must be at least 1 hour from now.")]
        public DateTime DateTime { get; set; }

        [StringLength(500, ErrorMessage = "Message too long.")]
        public string Message { get; set; }
    }


}
