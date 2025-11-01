namespace Domain.Entities.Base
{
    public class BaseEntity<T>
    {
        public T ID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
    }
}
