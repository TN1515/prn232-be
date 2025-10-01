using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Payload.Request.Orders
{
    public class CancelOrderRequest
    {
        public Guid OrderID {  get; set; }
    }
}
