using Application.Interface.IRepository.V1;
using Domain.Payload.Base;
using Domain.Payload.Request.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/products")]
    public class ProductController(IProductRepository _repository,
                                   ILogger<ProductController> _logger) : Controller
    {
        [HttpPost("create")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> CreateNewProduct([FromBody] CreateProductRequest request)
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

                var response = await _repository.CreateNewProduct(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.InnerException.Message
                };
                _logger.LogError(ex, "Error in CreateNewProduct: {Message}" + "\n", ex.InnerException.Message);
                return StatusCode(500, error);
            }
        }

        [HttpPut("update/{productId}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid productId, [FromBody] UpdateProductRequest request)
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
                var response = await _repository.UpdateProduct(productId, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError(ex, "Error in UpdateProduct: {Message}" + "\n", ex.StackTrace);
                return StatusCode(500, error);
            }
        }

        [HttpDelete("delete/{productId}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
        {
            try
            {
                var response = await _repository.DeleteProduct(productId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError(ex, "Error in DeleteProduct: {Message}" + "\n", ex.StackTrace);
                return StatusCode(500, error);
            }
        }

        [HttpGet("get-products")]
        public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1,
                                                     [FromQuery] int pageSize = 10,
                                                     [FromQuery] string? search = null,
                                                     [FromQuery] string? filter = null,
                                                     [FromQuery] bool? isActive = null,
                                                     [FromQuery] Guid? ShopID = null)
        {
            try
            {

                var response = await _repository.GetProducts(pageNumber, pageSize, search, filter, isActive, ShopID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError(ex, "Error in GetProducts: {Message}" + "\n", ex.StackTrace);
                return StatusCode(500, error);
            }
        }

        [HttpGet("get-detail")]
        public async Task<IActionResult> GetDetail([FromQuery] Guid productId)
        {
            try
            {
                var response = await _repository.GetDetail(productId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Detail API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("get-products-by-shop")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> GetProductByShop(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? filter = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] Guid? ShopID = null)
        {
            try
            {
                var response = await _repository.GetProductsByShop(
                    pageNumber,
                    pageSize,
                    search,
                    filter,
                    isActive,
                    ShopID
                );

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                _logger.LogError(ex, "Error in GetProductByShop: {Message}", ex.StackTrace);
                return StatusCode(500, error);
            }
        }

        [HttpPost("favorite-disfavorite")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> FavoriteOrDisFavorite([FromBody] FavoriteProductRequest request)
        {
            try
            {
                var response = await _repository.FavoriteOrDisFavoriteProduct(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Favoriete Product API]" + "\n" + ex.Message);

                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("get-favorite-product")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetFavoriteProduct([FromQuery]int pageNumber = 1, [FromQuery]int pageSize = 10)
        {
            try
            {
                var response = await _repository.GetFavoriteProduct(pageNumber, pageSize);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Favorite Product]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

    }
}
