namespace Domain.Payload.Response.Shop
{
    public class GetShopsResponse
    {
        public Guid ShopID { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Logo { get; set; } = null!;
        public string City { get; set; }
        public string Province { get; set; }
        public decimal RatingAverage { get; set; }
        public bool IsActive { get; set; }
    }
}
