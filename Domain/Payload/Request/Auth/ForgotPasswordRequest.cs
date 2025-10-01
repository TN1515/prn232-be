using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Auth
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Please enter key from email")]
        public string Key { get; set; }

        [Required(ErrorMessage = "Please enter new password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must be at least 8 chars, include upper, lower, number and special char")]
        public string NewPassword { get; set; }
    }
}
