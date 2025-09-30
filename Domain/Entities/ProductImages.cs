using Domain.Domain.Entities;

namespace Domain.Entities
{
    public class ProductImages
    {
        public Guid ID { get; set; }
        public string Url { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid ProductID { get; set; }
        public virtual Product Product { get; set; }
    }
}
