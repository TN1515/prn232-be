using Domain.Payload.Base;
using Domain.Payload.Request.Auth;
using Domain.Payload.Request.Users;
using Domain.Payload.Response.Auth;
using Domain.Payload.Response.Users;

namespace Application.Interface.IRepository.V1
{
    public interface IAuthenticationRepository
    {
        Task<ApiResponse<string>> UserRegister(RegisterRequest request);
        Task<ApiResponse<LoginResponse>> Login(LoginRequest request);
        Task<ApiResponse<string>> ChangePassword(ChangePasswordRequest request);
        Task<ApiResponse<string>> SendKeyForgotPassword(SendKeyEmailRequest request);
        Task<ApiResponse<string>> SetNewPassword(SetNewPasswordRequest request);
        Task<ApiResponse<GetUserResponse>> GetUser();
        Task<ApiResponse<string>> EditProfile(EditUserRequest request);
    }
}
