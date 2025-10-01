using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enum;
#pragma warning disable
namespace Domain.Payload.Request.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "User name is required")]
        [MaxLength(50, ErrorMessage = "User name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "User name can only contain letters, numbers, and underscores")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]

        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 chars, include upper, lower, number and special char")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0|\+84)(\d{9})$", ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public RoleEnum Role { get; set; }
    }
}
