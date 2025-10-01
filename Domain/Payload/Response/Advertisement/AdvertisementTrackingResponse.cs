namespace Domain.Payload.Response.Advertisement
{
    public class AdvertisementTrackingResponse
    {
        public Guid ID { get; set; }
        public Guid AdvertisementID { get; set; }
        public string TrackingType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
