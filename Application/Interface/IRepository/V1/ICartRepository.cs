using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.CartItem;
using Domain.Payload.Response.Cart;

namespace Application.Interface.IRepository.V1
{
    public interface ICartRepository
    {
        Task<ApiResponse<string>> AddCartItem(AddCartItemRequest request);
        Task<ApiResponse<string>> DeleteCartItem(Guid cartItemId);
        Task<ApiResponse<string>> EditCartItemQuantity(EditCartItemQuantityRequest request);
        Task<ApiResponse<ProcedurePagingResponse<GetCarts>>> GetCartsItem(int pageNumber, int pageSize);
    }
}
