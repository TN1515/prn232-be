namespace Domain.Payload.Response.Orders
{
    public class GetOrderDetailsResponse
    {
        public Guid ID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public string CommonImage { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmounts { get; set; }
        public string? Note { get; set; }
        public Guid OrderID { get; set; }
    }
}
