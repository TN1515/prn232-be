using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Response.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructer.Implement.Repository.V1
{
    public class UserRepository(GenericCacheInvalidator<User> _userCache,
                                GenericCacheInvalidator<Shop> _shopCache,
                                IMemoryCache _cache,
                                DBContext _context) : IUserRepository
    {
        public async Task<ApiResponse<string>> BlockOrUnBlock(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "User not found",
                        Data = null
                    };
                }

                user.IsActive = !user.IsActive;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _userCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = user.IsActive ? "Unblock User Success" : "Block User Success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetUsersResponse>>> GetUsers(int pageNumber, int pageSize, string? search, string? filter)
        {
            var parameters = new ListParameters<User>(pageNumber, pageSize);
            parameters.AddFilter("search", search);
            parameters.AddFilter("filter", filter);

            var cacheKey = _userCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey,out ProcedurePagingResponse<GetUsersResponse> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetUsersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get Users Success(Cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetUsers",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    search = search,
                    filter = filter
                },

                commandType: CommandType.StoredProcedure
                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetUsersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Not found",
                    Data = null
                };
            }
            
            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetUsersResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetUsersResponse
                {
                    Id = x.Id,
                    UserName = x.UserName ?? string.Empty,
                    Email = x.Email ?? string.Empty,
                    FullName = x.FullName ?? string.Empty,
                    Phone = x.Phone ?? string.Empty,
                    Gender = x.Gender,                 
                    DBO = x.DBO,                       
                    Address = x.Address,
                    Avatar = x.Avatar,
                    CreatedDate = x.CreatedDate,
                    ModifyDate = x.ModifyDate,
                    Role = x.Role ?? string.Empty,
                    IsActive = x.IsActive
                }).Where(x => !x.Role.Equals("Admin")).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };  

            _cache.Set(cacheKey, response,options);
            _userCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetUsersResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Users Success",
                Data = response
            };
        }
    }
}
