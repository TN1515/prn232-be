namespace Domain.Payload.Response.Cart
{
    public class GetCarts
    {
        public Decimal TotalAmounts { get; set; }
        public List<GetCartItemsResponse> CartItems { get; set; }
    }
}
