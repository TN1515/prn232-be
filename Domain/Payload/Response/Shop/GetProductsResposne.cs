namespace Domain.Payload.Response.Shop
{
    public class GetProductsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Material { get; set; }
        public string CommonImage { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }

        public Guid ShopID { get; set; }
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public string ShopPhone { get; set; }
        public decimal Rating { get; set; } = 0;
    }
}
