using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Blogs;
using Domain.Payload.Response.Blogs;

namespace Application.Interface.IRepository.V1
{
    public interface IBlogRepository
    {
        Task<ApiResponse<string>> CreateBlog(CreateBlogRequest request);
        Task<ApiResponse<string>> EditBlog(Guid blogId, EditBlogRequest request);
        Task<ApiResponse<string>> DeleteBlog(Guid blogID);
        Task<ApiResponse<ProcedurePagingResponse<GetBlogs>>> GetBlogs(int pageNumber, int pageSize, string? search, string? filter);
        Task<ApiResponse<ProcedurePagingResponse<GetBlogs>>> GetBlogsByAuthor(
                 int pageNumber, int pageSize, string? search, string? filter);
        Task<ApiResponse<GetDetail>> GetDetail(Guid blogID);
    }
}
