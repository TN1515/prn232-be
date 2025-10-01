using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Product
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CostPrice must be greater than 1")]
        public decimal? CostPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 1")]
        public decimal? Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Stock must be greater than 1")]
        public int? Stock { get; set; }

        public string? Material { get; set; }

        public string? CommonImage { get; set; }

        public string? Category { get; set; }

        public List<AddProductImagesRequest> MoreImages { get; set; }
    }
}
