using Application.Interface.IRepository.V1;
using Domain.Payload.Request.ProductFeedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/product-feedbacks")]
    public class ProductFeedbackController(IProductFeedbacksRepository _repository,
                                           ILogger<ProductFeedbackController> _logger) : Controller
    {
        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> Feedback([FromBody] FeedbackRequest request)
        {
            try
            {
                var response = await _repository.FeedBacks(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Feedback API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPatch("edit/{feedbackID}")]
        [Authorize]
        public async Task<IActionResult> EditFeedback(Guid feedbackID, [FromBody] EditFeedbackRequest request)
        {
            try
            {
                var response = await _repository.EditFeedback(feedbackID, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Feedback API]" + "\n" + ex.InnerException.Message);
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("get-feedbacks")]
        public async Task<IActionResult> GetFeeedBacks([FromQuery] Guid productID, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] decimal? ratingFilter = 0)
        {
            try
            {
                var response = await _repository.GetFeedbacks(productID, pageNumber, pageSize, ratingFilter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Feedbacks]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpDelete("delete/{feedbackID}")]
        [Authorize]
        public async Task<IActionResult> DeleteFeedback(Guid feedbackID)
        {
            try
            {
                var response = await _repository.DeleteFeedback(feedbackID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Delete Feedback API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
