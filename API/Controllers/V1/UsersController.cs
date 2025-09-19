using Application.Interface.IRepository.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController(IUserRepository _repository, ILogger<UsersController> _logger) : Controller
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery]int pageNumber = 1, [FromQuery]int pageSize = 10, 
                                                  [FromQuery]string? search = null, [FromQuery] string? filter = null)
        {
            try
            {
                var response = await _repository.GetUsers(pageNumber, pageSize, search, filter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Users API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPatch("block-unblock/{userID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUnBlock(Guid userID)
        {
            try
            {
                var response = await _repository.BlockOrUnBlock(userID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Block UnBlock Users API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
