using Application.Interface.IRepository.V1;
using Domain.Payload.Base;
using Domain.Payload.Request.Advertisement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/advertisements")]
    public class AdvertisementController(IAdvertisementRepository _repository,
                                         ILogger<AdvertisementController> _logger) : Controller
    {
        // Admin APIs
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdvertisement([FromBody] CreateAdvertisementRequest request)
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

                var response = await _repository.CreateAdvertisement(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var error = new ApiResponse<string>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ex.InnerException?.Message
                };
                _logger.LogError(ex, "Error in CreateAdvertisement: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdvertisements(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var response = await _repository.GetAdvertisements(pageNumber, pageSize, isActive);
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
                _logger.LogError(ex, "Error in GetAdvertisements: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpGet("detail/{advertisementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdvertisementDetail([FromRoute] Guid advertisementId)
        {
            try
            {
                var response = await _repository.GetAdvertisementDetail(advertisementId);
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
                _logger.LogError(ex, "Error in GetAdvertisementDetail: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpPut("update/{advertisementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAdvertisement(
            [FromRoute] Guid advertisementId,
            [FromBody] UpdateAdvertisementRequest request)
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

                var response = await _repository.UpdateAdvertisement(advertisementId, request);
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
                _logger.LogError(ex, "Error in UpdateAdvertisement: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpDelete("delete/{advertisementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAdvertisement([FromRoute] Guid advertisementId)
        {
            try
            {
                var response = await _repository.DeleteAdvertisement(advertisementId);
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
                _logger.LogError(ex, "Error in DeleteAdvertisement: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpPut("toggle-status/{advertisementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleAdvertisementStatus(
            [FromRoute] Guid advertisementId,
            [FromBody] ToggleAdvertisementStatusRequest request)
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

                var response = await _repository.ToggleAdvertisementStatus(advertisementId, request);
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
                _logger.LogError(ex, "Error in ToggleAdvertisementStatus: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpGet("tracking/{advertisementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdvertisementTracking(
            [FromRoute] Guid advertisementId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var response = await _repository.GetAdvertisementTracking(advertisementId, pageNumber, pageSize);
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
                _logger.LogError(ex, "Error in GetAdvertisementTracking: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        // Customer APIs
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAdvertisements()
        {
            try
            {
                var response = await _repository.GetActiveAdvertisements();
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
                _logger.LogError(ex, "Error in GetActiveAdvertisements: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpPost("record-impression")]
        public async Task<IActionResult> RecordImpression([FromBody] RecordImpressionRequest request)
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

                var response = await _repository.RecordImpression(request);
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
                _logger.LogError(ex, "Error in RecordImpression: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }

        [HttpPost("record-click")]
        public async Task<IActionResult> RecordClick([FromBody] RecordClickRequest request)
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

                var response = await _repository.RecordClick(request);
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
                _logger.LogError(ex, "Error in RecordClick: {Message}", ex.Message);
                return StatusCode(500, error);
            }
        }
    }
}
