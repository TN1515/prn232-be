namespace Domain.Payload.Request.Advertisement
{
    public class CreateAdvertisementRequest
    {
        public string MediaUrl { get; set; }
        public string? RedirectUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
