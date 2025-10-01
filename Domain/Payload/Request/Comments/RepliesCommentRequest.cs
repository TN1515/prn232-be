using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Comments
{
    public class RepliesCommentRequest
    {
        [Required(ErrorMessage = "Please enter replies content")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Please select comment want replies")]
        public Guid CommentID { get; set; }
    }
}
