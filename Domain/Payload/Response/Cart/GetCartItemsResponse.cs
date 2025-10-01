namespace Domain.Payload.Response.Cart
{
    public class GetCartItemsResponse
    {
        public Guid CartItemID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmounts { get; set; }
    }
}
