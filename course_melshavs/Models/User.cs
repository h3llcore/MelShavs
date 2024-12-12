using System.ComponentModel.DataAnnotations;

namespace course_melshavs.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [Phone]
        [StringLength(15, ErrorMessage = "Номер телефону не може перевищувати 15 символів.")]
        public string? PhoneNumber { get; set; }

        public string ConfirmPassword { get; set; }
        public bool AgreePrivacyPolicy { get; set; }

        public bool IsAdmin { get; set; }


    }
}