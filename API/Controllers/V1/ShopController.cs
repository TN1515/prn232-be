using Application.Interface.IRepository.V1;
using Domain.Payload.Base;
using Domain.Payload.Request.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/shops")]
    public class ShopController(IShopRepository _repository,
                                ILogger<ShopController> _logger) : Controller
    {
        [HttpPost("register")]
        public async Task<IActionResult> ShopRegister([FromBody] ShopRegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                                          .SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();

                    return BadRequest(new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", errors), // nối tất cả message lại
                        Data = null
                    });
                }

                var reponse = await _repository.ShopRegister(request);
                return StatusCode(reponse.StatusCode, reponse);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };

                _logger.LogError("[Register Shop]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPatch("edit-shop-info/{shopId}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> EditShopID(Guid shopId, [FromBody] EditShopRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                                          .SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
                    return BadRequest(new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", errors),
                        Data = null
                    });
                }
                var reponse = await _repository.EditShopInfo(shopId, request);
                return StatusCode(reponse.StatusCode, reponse);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError("[Edit Shop Info]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpGet("get-shops")]
        public async Task<IActionResult> GetShops([FromQuery] int pageNumber = 1,
                                                  [FromQuery] int pageSize = 10,
                                                  [FromQuery] string? search = null,
                                                  [FromQuery] string? filter = null,
                                                  [FromQuery] bool? IsActive = true)
        {
            try
            {
                var response = await _repository.GetShops(pageNumber, pageSize, search, filter, IsActive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError("[Get Shops]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpDelete("delete-shop/{shopId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShop(Guid shopId)
        {
            try
            {
                var response = await _repository.DeleteShop(shopId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError("[Delete Shop]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpGet("get-shop-detail")]
        public async Task<IActionResult> GetShopDetail([FromQuery] Guid shopId)
        {
            try
            {
                var response = await _repository.GetShopDetail(shopId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError("[Get Shop Detail]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpGet("get-shop-by-owner")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> GetShopByOwner()
        {
            try
            {
                var response = await _repository.GetShopByOwner();
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError("[Get Shop By Owner]" + "\n" + error);
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPatch("block-unblock-shop/{shopID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUnBlockShop(Guid shopID)
        {
            try
            {
                var response = await _repository.BlockAndUnBlockShop(shopID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Block and Unblock Shop]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
