using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.ProductFeedbacks;
using Domain.Payload.Response.ProductFeedbaks;

namespace Application.Interface.IRepository.V1
{
    public interface IProductFeedbacksRepository
    {
        Task<ApiResponse<string>> FeedBacks(FeedbackRequest request);
        Task<ApiResponse<string>> EditFeedback(Guid feedbackID, EditFeedbackRequest request);
        Task<ApiResponse<ProcedurePagingResponse<GetFeedbacksResponse>>> GetFeedbacks(Guid productID, int pageNumber, int pageSize, decimal? ratingFilter);
        Task<ApiResponse<string>> DeleteFeedback(Guid feebackID);
    }
}
