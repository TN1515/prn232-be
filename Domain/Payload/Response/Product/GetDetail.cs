using Domain.Payload.Response.ProductImage;

namespace Domain.Payload.Response.Product
{
    public class GetDetail
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

        public decimal Rating { get; set; }
        public List<ProductImagesResponse> MoreImages { get; set; }

        public string ShopID { get; set; }
        public string ShopName { get; set; }
        public string Address { get; set; }

        public string ShopPhone {  get; set; }
    }
}
