using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Shop;
using Domain.Payload.Response.Shop;

namespace Application.Interface.IRepository.V1
{
    public interface IShopRepository
    {
        Task<ApiResponse<string>> ShopRegister(ShopRegisterRequest request);
        Task<ApiResponse<string>> EditShopInfo(Guid shopId, EditShopRequest request);
        Task<ApiResponse<ProcedurePagingResponse<GetShopsResponse>>> GetShops(int pageNumber, int pageSize, string? search, string? Filter, bool? IsActive);
        Task<ApiResponse<string>> DeleteShop(Guid shopId);
        Task<ApiResponse<GetShopDetail>> GetShopDetail(Guid shopId);
        Task<ApiResponse<GetShopByOwner>> GetShopByOwner();
        Task<ApiResponse<string>> BlockAndUnBlockShop(Guid shopID);
    }
}
