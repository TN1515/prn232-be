namespace Domain.Domain.Entities
{
    public class AdvertisementTracking
    {
        public Guid ID { get; set; }
        public Guid AdvertisementID { get; set; }
        public string TrackingType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
