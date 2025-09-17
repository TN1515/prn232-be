using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Product;
using Domain.Payload.Response.Product;
using Domain.Payload.Response.Shop;

namespace Application.Interface.IRepository.V1
{
    public interface IProductRepository
    {
        Task<ApiResponse<string>> CreateNewProduct(CreateProductRequest request);
        Task<ApiResponse<string>> UpdateProduct(Guid productId, UpdateProductRequest request);
        Task<ApiResponse<string>> DeleteProduct(Guid productId);
        Task<ApiResponse<ProcedurePagingResponse<GetProductsResponse>>> GetProducts(int pageNumber,
                                                                                    int pageSize,
                                                                                    string? search,
                                                                                    string? filter,
                                                                                    bool? isActive,
                                                                                    Guid? ShopID);
        Task<ApiResponse<GetDetail>> GetDetail(Guid productId);
        Task<ApiResponse<ProcedurePagingResponse<GetProductsResponse>>> GetProductsByShop(
                                                                                            int pageNumber,
                                                                                            int pageSize,
                                                                                            string? search = null,
                                                                                            string? filter = null,
                                                                                            bool? isActive = null,
                                                                                            Guid? ShopIDFromRequest = null);

        Task<ApiResponse<string>> FavoriteOrDisFavoriteProduct(FavoriteProductRequest request);
        Task<ApiResponse<ProcedurePagingResponse<GetFavoriteProductResponse>>> GetFavoriteProduct(int pageNumber, int pageSize);

    }
}
