namespace Domain.Payload.Request.ProductFeedbacks
{
    public class FeedbackRequest
    {
        public string Content { get; set; }
        public decimal Rating { get; set; }
        public Guid ProductID { get; set; }

    }
}
