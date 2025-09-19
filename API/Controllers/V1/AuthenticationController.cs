using Application.Interface.IRepository.V1;
using Domain.Payload.Base;
using Domain.Payload.Request.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthenticationController(IAuthenticationRepository _repository,
                                          ILogger<AuthenticationController> _logger) : Controller
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Domain.Payload.Request.Auth.RegisterRequest request)
        {
            try
            {
                var response = await _repository.UserRegister(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Register API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };

                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Domain.Payload.Request.Auth.LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", ModelState.Values
                                                        .SelectMany(x => x.Errors)
                                                        .Select(x => x.ErrorMessage)),
                        Data = null
                    };

                    return StatusCode(badRequest.StatusCode, badRequest);
                }
                var response = await _repository.Login(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Login API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] Domain.Payload.Request.Auth.ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", ModelState.Values
                                                        .SelectMany(x => x.Errors)
                                                        .Select(x => x.ErrorMessage)),
                        Data = null
                    };
                    return StatusCode(badRequest.StatusCode, badRequest);
                }
                var response = await _repository.ChangePassword(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Change Password API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPost("send-key-forgot-password")]
        public async Task<IActionResult> SendKeyForgotPassword([FromBody] SendKeyEmailRequest request)
        {
            try
            {
                var response = await _repository.SendKeyForgotPassword(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Send Key Forgot Password API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPatch("set-new-password")]
        public async Task<IActionResult> SetNewPassword([FromBody] SetNewPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", ModelState.Values
                                                        .SelectMany(x => x.Errors)
                                                        .Select(x => x.ErrorMessage)),
                        Data = null
                    };
                    return StatusCode(badRequest.StatusCode, badRequest);
                }
                var response = await _repository.SetNewPassword(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Set New Password API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpGet("get-user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var response = await _repository.GetUser();
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get User API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }

        [HttpPut("edit-profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] Domain.Payload.Request.Users.EditUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var badRequest = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = string.Join("; ", ModelState.Values
                                                        .SelectMany(x => x.Errors)
                                                        .Select(x => x.ErrorMessage)),
                        Data = null
                    };
                    return StatusCode(badRequest.StatusCode, badRequest);
                }
                var response = await _repository.EditProfile(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Profile API]" + "\n" + ex.Message + "\n" + ex.StackTrace);
                var error = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
                return StatusCode(error.StatusCode, error);
            }
        }
    }
}
