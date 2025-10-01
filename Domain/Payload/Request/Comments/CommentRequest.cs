using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Comments
{
    public class CommentRequest
    {
        [Required(ErrorMessage = "Please enter content")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Please select blog want comment")]
        public Guid BlogID { get; set; }
    }
}
