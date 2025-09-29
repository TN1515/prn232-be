using Domain.Domain.Entities;

namespace Domain.Entities
{
    public class BankSettings
    {
        public Guid ID { get; set; }
        public string BankName { get; set; }
        public string BankNo { get; set; }
        public bool IsUse { get; set; }
        public Guid ShopID { get; set; }
        public virtual Shop Shop { get; set; }
    }
}
