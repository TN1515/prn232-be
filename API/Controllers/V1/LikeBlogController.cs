using Application.Interface.IRepository.V1;
using Domain.Payload.Request.Like;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/likes")]
    public class LikeBlogController(ILikeRepository _repository,
                                    ILogger<LikeBlogController> _logger) : Controller
    {
        [HttpPost("like-dislike")]
        [Authorize]
        public async Task<IActionResult> LikeBlog([FromBody] LikeRequest request)
        {

            try
            {
                var response = await _repository.LikeOrDislike(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Like API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException?.Message);
            }

        }
    }
}
