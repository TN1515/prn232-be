using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Product
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "CostPrice is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CostPrice must be greater than 1")]
        public decimal CostPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 1")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Stock must be greater than 1")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Material is required")]
        public string Material { get; set; }

        [Required(ErrorMessage = "CommonImage is required")]
        public string CommonImage { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
        public List<AddProductImagesRequest>? MoreImage { get; set; }

    }
}
