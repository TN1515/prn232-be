namespace Domain.Payload.Request.Slider
{
    public class UpdateSliderRequest
    {
        public Guid ID { get; set; }
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public int? OrderIndex { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
