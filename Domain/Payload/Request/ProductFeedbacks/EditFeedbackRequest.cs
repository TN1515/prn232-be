namespace Domain.Payload.Request.ProductFeedbacks
{
    public class EditFeedbackRequest
    {
        public string? Content { get; set; }
        public decimal? Rating { get; set; }
    }
}
