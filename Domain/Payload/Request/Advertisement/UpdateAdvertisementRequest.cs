namespace Domain.Payload.Request.Advertisement
{
    public class UpdateAdvertisementRequest
    {
        public string? MediaUrl { get; set; }
        public string? RedirectUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
