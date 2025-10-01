namespace Domain.Payload.Response.Advertisement
{
    public class AdvertisementDetailResponse
    {
        public Guid ID { get; set; }
        public string MediaUrl { get; set; }
        public string? RedirectUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? Views { get; set; }
        public int? Clicks { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
