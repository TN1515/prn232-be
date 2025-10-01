using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Please enter current password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 chars, include upper, lower, number and special char")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Please enter new password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must be at least 8 chars, include upper, lower, number and special char")]
        public string NewPassword { get; set; }
    }
}
