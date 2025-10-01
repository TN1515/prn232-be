namespace Domain.Payload.Request.Slider
{
    public class ToggleSliderStatusRequest
    {
        public Guid ID { get; set; }
        public bool IsActive { get; set; }
    }
}
