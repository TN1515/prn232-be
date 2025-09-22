using Application.Interface.IRepository.V1;
using Domain.Payload.Request.Blogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/blogs")]
    public class BlogsController(IBlogRepository _repository,
                                ILogger<BlogsController> _logger) : Controller
    {
        [HttpPost("create")]
        [Authorize(Roles = "User, Shop")]
        public async Task<IActionResult> CreateBlogs([FromBody] CreateBlogRequest request)
        {
            try
            {
                var response = await _repository.CreateBlog(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Create Blog API]" + "\n" + ex.StackTrace);
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPatch("edit/{blogId}")]
        [Authorize(Roles = "User, Shop")]
        public async Task<IActionResult> EditBlog([FromRoute] Guid BlogId, [FromBody] EditBlogRequest request)
        {
            try
            {
                var response = await _repository.EditBlog(BlogId, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Blog API]" + "\n" + ex.StackTrace);
                return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpDelete("delete/{blogId}")]
        [Authorize(Roles = "User, Shop,Admin")]
        public async Task<IActionResult> DeleteBlog([FromRoute] Guid blogId)
        {
            try
            {
                var response = await _repository.DeleteBlog(blogId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Delete Blog API]" + "\n" + ex.StackTrace);
                return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBlog([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? filter = null)
        {
            try
            {
                var response = await _repository.GetBlogs(pageNumber, pageSize, search, filter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Blogs API]" + "\n" + ex.StackTrace);
                return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpGet("get-by-author")]
        [Authorize]
        public async Task<IActionResult> GetBlogByAuthor([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? filter = null)
        {
            try
            {
                var response = await _repository.GetBlogsByAuthor(pageNumber, pageSize, search, filter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Blogs By Author API]" + "\n" + ex.StackTrace);
                return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpGet("get-detail")]
        public async Task<IActionResult> GetDetail([FromQuery] Guid blogID)
        {
            try
            {
                var response = await _repository.GetDetail(blogID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Detail API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.InnerException.Message);
            }
        }

    }
}
