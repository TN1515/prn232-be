using Application.Interface.IRepository.V1;
using Domain.Payload.Base;
using Domain.Payload.Request.CartItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/carts")]
    public class CartController(ICartRepository _repository,
                                ILogger<Controller> _logger) : Controller
    {
        [HttpPost("add-item")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddCartItem([FromBody] AddCartItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = 400,
                        Message = string.Join("; ", ModelState.Values
                                                             .SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)),
                        Data = null
                    };
                    return BadRequest(badRequest);
                }

                var response = await _repository.AddCartItem(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = Domain.Share.Common.StatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };

                _logger.LogError($"[CartController][AddCartItem] - {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, error);
            }
        }

        [HttpDelete("delete-item/{cartItemId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteCartItem(Guid cartItemId)
        {
            try
            {
                var response = await _repository.DeleteCartItem(cartItemId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = Domain.Share.Common.StatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError($"[CartController][DeleteCartItem] - {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, error);
            }
        }

        [HttpPut("edit-item-quantity")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditItemQuantity([FromBody] EditCartItemQuantityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = 400,
                        Message = string.Join("; ", ModelState.Values
                                                             .SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)),
                        Data = null
                    };
                    return BadRequest(badRequest);
                }
                var response = await _repository.EditCartItemQuantity(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = Domain.Share.Common.StatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError($"[CartController][EditItemQuantity] - {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, error);
            }
        }

        [HttpGet("get-items")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetCartItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _repository.GetCartsItem(pageNumber, pageSize);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = Domain.Share.Common.StatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError($"[CartController][GetCartItems] - {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, error);
            }
        }
    }
}
