using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Request.Auth;
using Domain.Payload.Request.Users;
using Domain.Payload.Response.Auth;
using Domain.Payload.Response.Users;
using Domain.Share.Common;
using Domain.Share.Util;
using Domain.Shares.Util;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RabbitMQContract.Consumer.Email;
using System.Net.Mail;

namespace Infrastructure.Implement.Repository.V1
{
    public class AuthenticationRepository(DBContext _context,
                                          GenericCacheInvalidator<User> _userCache,
                                          IMemoryCache _cache,
                                          IBus _bus) : IAuthenticationRepository
    {
        public async Task<ApiResponse<string>> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                var getUser = await _context.Users
                                            .FirstOrDefaultAsync(x => x.Id == Guid.Parse(JWTUtil.GetUser()));
                if (getUser == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "User not found",
                        Data = null
                    };
                }

                string currentPasswordHash = PasswordUtil.HashPassword(request.CurrentPassword);
                if (!getUser.Password.Equals(currentPasswordHash))
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Unauthorized,
                        Message = "Current password is incorrect",
                        Data = null
                    };
                }

                getUser.Password = PasswordUtil.HashPassword(request.NewPassword);
                getUser.ModifyDate = DateTime.Now;
                _context.Users.Update(getUser);
                await _context.SaveChangesAsync();
                _userCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Password changed successfully",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<GetUserResponse>> GetUser()
        {
            var userId = Guid.Parse(JWTUtil.GetUser()!);

            using var connection = _context.Database.GetDbConnection();
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }

            var response = await connection.QueryAsync<GetUserResponse>(
                "dbo.sp_GetUser",
                new
                {
                    UserId = userId
                },

                commandType: System.Data.CommandType.StoredProcedure

                );

            var user = response.FirstOrDefault();
            if (user == null)
            {
                return new ApiResponse<GetUserResponse>
                {
                    StatusCode = StatusCode.NotFound,
                    Message = "User not found",
                    Data = null
                };
            }

            return new ApiResponse<GetUserResponse>
            {
                StatusCode = StatusCode.OK,
                Message = "Get user successfully",
                Data = user
            };
        }
        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                var checkEmailAndUserName = await _context.Users
                                               .Include(x => x.Shops)
                                               .FirstOrDefaultAsync(x => x.Email.Equals(request.UserNameOrEmail)
                                                                    || x.UserName.Equals(request.UserNameOrEmail));

                if (checkEmailAndUserName == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Email or UserName does not exist",
                        Data = null
                    };
                }

                string passwordHash = PasswordUtil.HashPassword(request.Password);
                var checkLogin = await _context.Users
                                                     .FirstOrDefaultAsync(x => (x.Email.Equals(request.UserNameOrEmail)
                                                                             || x.UserName.Equals(request.UserNameOrEmail))
                                                                             && x.Password.Equals(passwordHash));
                if (checkLogin == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        StatusCode = StatusCode.Unauthorized,
                        Message = "Password is incorrect",
                        Data = null
                    };
                }

                if (checkLogin != null && checkLogin.Role == Domain.Entities.Enum.RoleEnum.Shop && !checkLogin.Shops.Any())
                {
                    var responseBad = new LoginResponse
                    {
                        userId = checkLogin.Id,  // thêm UserId
                        Username = checkLogin.UserName,
                        Role = checkLogin.Role.ToString(),
                        Token = null  // chưa login thành công nên token để null
                    };

                    return new ApiResponse<LoginResponse>
                    {
                        StatusCode = StatusCode.BadRequest,
                        Message = "Bạn chưa đăng ký thông tin shop vui lòng đăng ký",
                        Data = responseBad
                    };
                }

                if (checkLogin != null && checkLogin.IsActive == false)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        StatusCode = StatusCode.Unauthorized,
                        Message = "Account has been blocked",
                        Data = null
                    };
                }
                var token = JWTUtil.GenerateToken(checkLogin);
                var response = new LoginResponse
                {
                    Token = token,
                    Role = checkLogin.Role.ToString(),
                    Username = checkLogin.UserName,
                };
                return new ApiResponse<LoginResponse>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Login successful",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<string>> EditProfile(EditUserRequest request)
        {
            try
            {
                var userId = Guid.Parse(JWTUtil.GetUser()!);
                var getUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

                if (getUser == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "User not found",
                        Data = null
                    };
                }

                // ✅ Check UserName duplicate
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    var existsUserName = await _context.Users
                        .AnyAsync(x => x.UserName == request.UserName && x.Id != userId);
                    if (existsUserName)
                    {
                        return new ApiResponse<string>
                        {
                            StatusCode = StatusCode.BadRequest,
                            Message = "Username already exists",
                            Data = null
                        };
                    }
                }

                // ✅ Check Email duplicate
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var existsEmail = await _context.Users
                        .AnyAsync(x => x.Email == request.Email && x.Id != userId);
                    if (existsEmail)
                    {
                        return new ApiResponse<string>
                        {
                            StatusCode = StatusCode.BadRequest,
                            Message = "Email already exists",
                            Data = null
                        };
                    }
                }

                // ✅ Check Phone duplicate
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    var existsPhone = await _context.Users
                        .AnyAsync(x => x.Phone == request.PhoneNumber && x.Id != userId);
                    if (existsPhone)
                    {
                        return new ApiResponse<string>
                        {
                            StatusCode = StatusCode.BadRequest,
                            Message = "Phone number already exists",
                            Data = null
                        };
                    }
                }

                // ✅ Update profile
                getUser.UserName = request.UserName ?? getUser.UserName;
                getUser.Email = request.Email ?? getUser.Email;
                getUser.Phone = request.PhoneNumber ?? getUser.Phone;
                getUser.FullName = request.FullName ?? getUser.FullName;
                getUser.Gender = request.Gender ?? getUser.Gender;
                getUser.Dbo = request.DBO ?? getUser.Dbo;
                getUser.Address = request.Address ?? getUser.Address;
                getUser.Avatar = request.Avatar ?? getUser.Avatar;
                getUser.ModifyDate = DateTime.Now;

                _context.Users.Update(getUser);
                await _context.SaveChangesAsync();
                _userCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Profile updated successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + "\n" + ex.StackTrace);
            }
        }
        public async Task<ApiResponse<string>> SendKeyForgotPassword(SendKeyEmailRequest request)
        {
            try
            {
                string code = CommonUtil.GenerateUniqueCode(8);

                var getUser = await _context.Users.FirstOrDefaultAsync(x => x.Email.Equals(request.Email));
                if (getUser == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Không tìm thấy thông tin",
                        Data = null
                    };
                }
                string cacheKey = $"ForgotPassword:{request.Email}";
                _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));
                getUser.Password = code;
                _context.Users.Update(getUser);
                await _context.SaveChangesAsync();

                var emailMessage = new EmailMessage
                {
                    To = request.Email,
                    Subject = "[Mã Khôi Phục Mật Khẩu]",
                    Body = MessageUtil.GenerateForgotPasswordEmail(code)
                };

                await _bus.Publish(emailMessage);

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Đã gửi mã khôi phục mật khẩu.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "Có lỗi xảy ra khi gửi mã khôi phục.",
                    Data = ex.Message
                };
            }
        }
        public async Task<ApiResponse<string>> SetNewPassword(SetNewPasswordRequest request)
        {
            try
            {
                var cacheKey = $"ForgotPassword:{request.Email}";

                if (!_cache.TryGetValue(cacheKey, out string cachedCode) || cachedCode != request.Code)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Mã khôi phục không hợp lệ hoặc đã hết hạn",
                        Data = null
                    };
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Không tìm thấy người dùng",
                        Data = null
                    };
                }

                user.Password = PasswordUtil.HashPassword(request.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _cache.Remove(cacheKey);

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Đặt lại mật khẩu thành công",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "Có lỗi xảy ra khi đặt lại mật khẩu",
                    Data = ex.Message
                };
            }
        }
        public async Task<ApiResponse<string>> UserRegister(RegisterRequest request)
        {
            try
            {
                var checkEmail = await _context.Users
                                               .FirstOrDefaultAsync(x => x.Email.Equals(request.Email));
                if (checkEmail != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "Email already exists",
                        Data = null
                    };
                }

                var checkUserName = await _context.Users
                                                  .FirstOrDefaultAsync(x => x.UserName.Equals(request.UserName));
                if (checkUserName != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "User name already exists",
                        Data = null
                    };
                }

                var checkPhone = await _context.Users
                                               .FirstOrDefaultAsync(x => x.Phone.Equals(request.PhoneNumber));
                if (checkPhone != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "Phone number already exists",
                        Data = null
                    };
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = request.UserName,
                    Password = PasswordUtil.HashPassword(request.Password),
                    Email = request.Email,
                    Phone = request.PhoneNumber,
                    FullName = "Chưa cập nhật",
                    Gender = "Chưa cập nhật",
                    Dbo = default,
                    Address = "Chưa cập nhật",
                    Avatar = "Chưa cập nhật",
                    CreatedDate = DateTime.Now,
                    Role = request.Role,
                    ModifyDate = null,
                    IsActive = true
                };



                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                if (newUser.Role == Domain.Entities.Enum.RoleEnum.User)
                {
                    var newCart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = newUser.Id,
                        TotalAmounts = 0
                    };

                    await _context.Carts.AddAsync(newCart);
                    await _context.SaveChangesAsync();
                }

                if (newUser.Role == Domain.Entities.Enum.RoleEnum.Shop)
                {
                    DateTime deadline = newUser.CreatedDate.AddHours(1);

                    var emailMessage = new EmailMessage
                    {
                        To = newUser.Email,
                        Subject = "[Cảnh báo] Thiết lập thông tin Shop trong vòng 1 giờ",
                        Body = MessageUtil.GenerateInactiveUserWarningEmail(
                            userName: newUser.UserName,
                            email: newUser.Email,
                            fullName: newUser.FullName,
                            phone: newUser.Phone,
                            gender: newUser.Gender,
                            dob: newUser.Dbo,
                            address: newUser.Address,
                            avatarUrl: "https://hmall-s5bv.vercel.app/images/HMallLogo.jpg",
                            createdDate: newUser.CreatedDate,
                            modifyDate: newUser.ModifyDate,
                            role: newUser.Role.ToString(),
                            deadline: deadline
                        )
                    };

                    await _bus.Publish(emailMessage);
                }


                _userCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.Created,
                    Message = "User registered successfully",
                    Data = newUser.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
