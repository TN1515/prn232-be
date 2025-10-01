namespace Domain.Payload.Response.Orders
{
    public class GetOrdersResponse
    {
        public Guid OrderID { get; set; }
        public string OrderCode { get; set; }
        public decimal TotalAmounts { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DeliveryAddress { get; set; }
        public bool IsActive { get; set; }
        public string PaymentMethod { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }

    }
}
