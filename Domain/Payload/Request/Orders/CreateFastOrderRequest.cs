using Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Payload.Request.Orders
{
    public class CreateFastOrderRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string ReceiverName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa nhỉ nhận hàng")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string ReceiverPhone { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public Guid ProductID {  get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
    }
}
