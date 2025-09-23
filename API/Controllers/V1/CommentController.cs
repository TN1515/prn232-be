using Application.Interface.IRepository.V1;
using Domain.Payload.Request.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/comments")]
    public class CommentController(ICommentRepository _repository,
                                   ILogger<CommentController> _logger) : Controller
    {

        [HttpPost("comment")]
        [Authorize]
        public async Task<IActionResult> Comment([FromBody] CommentRequest request)
        {
            try
            {
                var response = await _repository.Comment(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Create Comment API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException.Message);
            }
        }


        [HttpPatch("edit/{commentID}")]
        [Authorize]

        public async Task<IActionResult> EditComment(Guid commentID, [FromBody] EditCommentRequest request)
        {
            try
            {
                var response = await _repository.EditComment(commentID, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Comment API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException.Message);
            }
        }

        [HttpDelete("delete/{commentID}")]
        [Authorize]

        public async Task<IActionResult> DeleteComment(Guid commentID)
        {
            try
            {
                var response = await _repository.DeleteComment(commentID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Delete Comment API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException.Message);
            }
        }


        [HttpPost("replies")]
        [Authorize]
        public async Task<IActionResult> Replies([FromBody] RepliesCommentRequest request)
        {
            try
            {
                var response = await _repository.RepliesComment(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Replies comment API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException.Message);
            }
        }

        [HttpGet("get-comments")]
        public async Task<IActionResult> GetComments([FromQuery] Guid blogId, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] int repliesPage, [FromQuery] int repliesSize)
        {

            if (pageNumber < 1 || pageSize < 1 || repliesPage < 1 || repliesSize < 1)
            {
                return BadRequest(new { StatusCode = 400, Message = "Page number and size must be greater than 0." });
            }
            var response = await _repository.GetComments(blogId, pageNumber, pageSize, repliesPage, repliesSize);
            return StatusCode(response.StatusCode, response);
        }


    }
}
