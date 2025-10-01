using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.CartItem
{
    public class EditCartItemQuantityRequest
    {
        [Required(ErrorMessage = "CartItemID is required")]
        public Guid CartItemID { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
