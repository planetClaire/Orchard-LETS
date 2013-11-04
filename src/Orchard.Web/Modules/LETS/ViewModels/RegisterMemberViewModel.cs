using System.ComponentModel.DataAnnotations;

namespace LETS.ViewModels
{
    public class RegisterMemberViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required(ErrorMessage = "The Confirm password field is required")]
        public string ConfirmPassword { get; set; }

        public dynamic UserProfile { get; set; }
    }
}