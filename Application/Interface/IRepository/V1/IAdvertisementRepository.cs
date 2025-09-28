using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Advertisement;
using Domain.Payload.Response.Advertisement;

namespace Application.Interface.IRepository.V1
{
    public interface IAdvertisementRepository
    {
        // Admin APIs
        Task<ApiResponse<string>> CreateAdvertisement(CreateAdvertisementRequest request);
        Task<ApiResponse<PagingResponse<GetAdvertisementResponse>>> GetAdvertisements(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
        Task<ApiResponse<AdvertisementDetailResponse>> GetAdvertisementDetail(Guid advertisementId);
        Task<ApiResponse<string>> UpdateAdvertisement(Guid advertisementId, UpdateAdvertisementRequest request);
        Task<ApiResponse<string>> DeleteAdvertisement(Guid advertisementId);
        Task<ApiResponse<string>> ToggleAdvertisementStatus(Guid advertisementId, ToggleAdvertisementStatusRequest request);
        Task<ApiResponse<PagingResponse<AdvertisementTrackingResponse>>> GetAdvertisementTracking(Guid advertisementId, int pageNumber = 1, int pageSize = 10);

        // Customer APIs
        Task<ApiResponse<List<ActiveAdvertisementResponse>>> GetActiveAdvertisements();
        Task<ApiResponse<string>> RecordImpression(RecordImpressionRequest request);
        Task<ApiResponse<string>> RecordClick(RecordClickRequest request);
    }
}
