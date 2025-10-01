using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Shop
{
    public class ShopRegisterRequest
    {
        [Required(ErrorMessage = "Shop name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0|\+84)(\d{9})$", ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; }

        [Required(ErrorMessage = "LogoUrl is required")]
        public string LogoUrl { get; set; }
        [Required(ErrorMessage = "CoverImageUrl is required")]
        public string CoverImageUrl { get; set; }
        public string QRBanking { get; set; }
        [Required(ErrorMessage = "UserID is required")]
        public Guid UserID { get; set; }

    }
}
