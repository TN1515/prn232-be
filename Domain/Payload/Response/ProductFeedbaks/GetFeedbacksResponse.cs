namespace Domain.Payload.Response.ProductFeedbaks
{
    public class GetFeedbacksResponse
    {
        public Guid ID { get; set; }
        public decimal Rating { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public Guid UserID { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }

    }
}
