using Application.Interface.IRepository.V1;
using Domain.Entities.Enum;
using Domain.Payload.Base;
using Domain.Payload.Request.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController(IOrdersRepository _repository,
                                  ILogger<OrdersController> _logger) : Controller
    {
        [HttpPost("create")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var response = await _repository.CreateOrder(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Create Order API]" + "\n" + ex.Message);
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("get-by-user")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetOrdersByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? Filer = null)
        {
            try
            {
                var response = await _repository.GetOrdersByUser(pageNumber, pageSize, Filer);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Orders By User]" + "\n" + ex.StackTrace);
                var erros = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };

                return StatusCode(erros.StatusCode, erros);
            }
        }

        [HttpGet("get-details")]
        public async Task<IActionResult> GetDetails([FromQuery] Guid orderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 9999)
        {
            try
            {
                var response = await _repository.GetDetails(orderId, pageNumber, pageSize);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var errors = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };

                return StatusCode(errors.StatusCode, errors);
            }

        }

        [HttpGet("get-by-shop")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> GetByShops([FromQuery] int pageNumber = 1,
                                                    [FromQuery] int pageSize = 999,
                                                    [FromQuery] string? filter = null)
        {
            try
            {
                var response = await _repository.GetOrderByShop(pageNumber, pageSize, filter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var errors = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(errors.StatusCode, errors);
            }
        }

        [HttpPost("create-qr-payment")]
        public async Task<IActionResult> CreateQrPayment([FromBody] CreatePayment request)
        {
            try
            {
                var response = await _repository.CreateQRPayment(request.OrderID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Create Order]" + "\n" + ex.InnerException.Message);
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPatch("edit-order-status/{orderID}")]
        public async Task<IActionResult> EditOrderStatus(Guid orderID, [FromForm] OrderStatusEnum status)
        {
            try
            {
                var response = await _repository.EditOrderStatus(orderID, status);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Order Status]" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("cancel-order")]
        [Authorize]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            try
            {
                var response = await _repository.CancelOrder(request.OrderID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Cancel Order API]" + "\n" + ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost("fast-order")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> FastOrder([FromBody] CreateFastOrderRequest request)
        {
            try
            {
                var response = await _repository.FastOrder(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Fast Order API]" + "\n" + ex.ToString());
                return StatusCode(500,ex.ToString());
            }
        }
    }
}
