namespace Domain.Payload.Request.Slider
{
    public class UpdateSliderOrderRequest
    {
        public Guid SliderID { get; set; }
        public int NewOrderIndex { get; set; }
    }
}
