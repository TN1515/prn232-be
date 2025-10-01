using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Users
{
    public class EditUserRequest
    {
        [MaxLength(50, ErrorMessage = "User name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "User name can only contain letters, numbers, and underscores")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0|\+84)(\d{9})$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DBO { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
    }
}
