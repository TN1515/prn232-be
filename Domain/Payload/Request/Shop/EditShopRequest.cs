using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Shop
{
    public class EditShopRequest
    {
        public string? Name { get; set; }

        public string? Address { get; set; }

        [RegularExpression(@"^(0|\+84)(\d{9})$", ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? City { get; set; }

        public string? Province { get; set; }

        public string? LogoUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? QRBanking { get; set; }
    }
}
