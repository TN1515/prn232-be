using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enum;

namespace Domain.Payload.Request.Orders
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string ReceiverName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa nhỉ nhận hàng")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string ReceiverPhone { get; set; }
        public List<Guid> CartItemIDs { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
