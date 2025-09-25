using Domain.Entities.Enum;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Orders;
using Domain.Payload.Response.Orders;
using SePaySerivce.PayLoad.Response;

namespace Application.Interface.IRepository.V1
{
    public interface IOrdersRepository
    {
        Task<ApiResponse<List<Guid>>> CreateOrder(CreateOrderRequest request);
        Task<ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>> GetOrdersByUser(int pageNumber, int pageSize, string? Filter);
        Task<ApiResponse<ProcedurePagingResponse<GetOrderDetailsResponse>>> GetDetails(Guid orderId, int pageNumber, int pageSize);
        Task<ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>> GetOrderByShop(int pageNumber, int pageSize, string? Filer);
        Task<ApiResponse<CreateQrResponse>> CreateQRPayment(Guid orderID);
        Task<ApiResponse<string>> EditOrderStatus(Guid orderID, OrderStatusEnum status);
        Task<ApiResponse<string>> CancelOrder(Guid orderID);
        Task<ApiResponse<string>> FastOrder(CreateFastOrderRequest request);    
    }
}
