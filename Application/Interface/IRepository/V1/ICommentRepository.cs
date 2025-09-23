using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Comments;
using Domain.Payload.Response.Blog;

namespace Application.Interface.IRepository.V1
{
    public interface ICommentRepository
    {
        Task<ApiResponse<string>> Comment(CommentRequest request);
        Task<ApiResponse<string>> DeleteComment(Guid commentID);
        Task<ApiResponse<string>> EditComment(Guid commentID, EditCommentRequest request);
        Task<ApiResponse<string>> RepliesComment(RepliesCommentRequest request);
        Task<ApiResponse<PagingResponse<CommentResponse>>> GetComments(Guid blogId, int pageNumber, int pageSize, int repliesPage, int repliesSize);
    }
}
