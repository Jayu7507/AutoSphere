using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models.ViewModels
{
    public class VerifyOTPViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        public string OTP { get; set; } = string.Empty;
    }
}
