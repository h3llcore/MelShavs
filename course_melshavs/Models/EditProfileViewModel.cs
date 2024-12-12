using System.ComponentModel.DataAnnotations;

namespace course_melshavs.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача обов'язкове.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Номер телефону обов'язковий.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Для підтвердження внесених змін введіть ваш поточний пароль.")]
        public string CurrentPassword { get; set; }
    }
}
