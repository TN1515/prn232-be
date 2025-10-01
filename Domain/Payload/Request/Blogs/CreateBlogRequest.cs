using System.ComponentModel.DataAnnotations;

namespace Domain.Payload.Request.Blogs
{
    public class CreateBlogRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Content is require")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; }

    }
}
