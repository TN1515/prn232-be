using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.CartItem
{
    public class AddCartItemRequest
    {
        [Required(ErrorMessage = "ProductId is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greaster than 0")]
        public int Quantity { get; set; }

        public string? Note { get; set; }
    }
}
