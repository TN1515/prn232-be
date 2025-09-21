using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Shop;
using Domain.Payload.Response.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class ShopRepository(DBContext _context,
                                GenericCacheInvalidator<Shop> _shopCache,
                                GenericCacheInvalidator<Product> _productCache,
                                GenericCacheInvalidator<User> _userCache,
                                IMemoryCache _cache) : IShopRepository
    {
        public async Task<ApiResponse<string>> BlockAndUnBlockShop(Guid shopID)
        {
            try
            {
                var getShop = await _context.Shops
                                            .Include(x => x.Owner)
                                            .FirstOrDefaultAsync(x => x.Id == shopID);
                if (getShop == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                // Toggle trạng thái
                getShop.IsActive = !getShop.IsActive;
                getShop.Owner.IsActive = !getShop.Owner.IsActive;

                _context.Shops.Update(getShop);
                await _context.SaveChangesAsync();

                // Xóa cache liên quan
                _shopCache.InvalidateEntityList();
                _userCache.InvalidateEntityList();
                _productCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = getShop.IsActive ? "Shop has been unblocked successfully"
                                               : "Shop has been blocked successfully",
                    Data = getShop.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<string>> DeleteShop(Guid shopId)
        {
            try
            {
                var checkDelete = _context.Shops
                                                .FirstOrDefault(x => x.Id == shopId && x.IsActive == true);
                if (checkDelete == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Shop not found or has been deleted",
                        Data = null
                    };
                }

                var getOwnerByShop = await _context.Users
                                                   .FirstOrDefaultAsync(x => x.Id == checkDelete.OwnerId);

                getOwnerByShop.IsActive = false;
                _context.Users.Update(getOwnerByShop);

                checkDelete.IsActive = false;
                checkDelete.ModifyDate = DateTime.Now;

                _context.Shops.Update(checkDelete);
                await _context.SaveChangesAsync();

                _shopCache.InvalidateEntity(shopId);
                _productCache.InvalidateEntityList();
                _userCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Shop deleted successfully",
                    Data = checkDelete.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<string>> EditShopInfo(Guid shopId, EditShopRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var getShop = await _context.Shops
                                            .FirstOrDefaultAsync(x => x.Id == shopId && x.IsActive == true);
                if (getShop == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Shop not found Or has been blocked",
                        Data = null
                    };
                }

                var ownerId = Guid.Parse(JWTUtil.GetUser()!);
                if (getShop.OwnerId != ownerId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to edit this shop",
                        Data = null
                    };
                }

                var checkEmail = await _context.Shops
                                                .FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != shopId);
                if (checkEmail != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = "Shop Email already exists",
                        Data = null
                    };

                }
                var checkShopPhone = await _context.Shops
                                                    .FirstOrDefaultAsync(x => x.Phone == request.Phone && x.Id != shopId);
                if (checkShopPhone != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = "Shop Phone already exists",
                        Data = null
                    };
                }
                getShop.Name = request.Name ?? getShop.Name;
                getShop.Address = request.Address ?? getShop.Address;
                getShop.Phone = request.Phone ?? getShop.Phone;
                getShop.Email = request.Email ?? getShop.Email;
                getShop.City = request.City ?? getShop.City;
                getShop.Province = request.Province ?? getShop.Province;
                getShop.LogoUrl = request.LogoUrl ?? getShop.LogoUrl;
                getShop.CoverImageUrl = request.CoverImageUrl ?? getShop.CoverImageUrl;
                getShop.Qrbanking = request.QRBanking ?? getShop.Qrbanking;
                getShop.ModifyDate = DateTime.Now;
                _context.Shops.Update(getShop);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _shopCache.InvalidateEntity(shopId);
                _productCache.InvalidateEntityList();
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Shop information updated successfully",
                    Data = getShop.Id.ToString()
                };
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<GetShopByOwner>> GetShopByOwner()
        {
            var ownerId = Guid.Parse(JWTUtil.GetUser()!);


            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            var result = connection.QueryFirstOrDefault<GetShopByOwner>(
                "dbo.sp_GetShopByOwner",
                new
                {
                    OwnerID = ownerId
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (result == null)
            {
                return new ApiResponse<GetShopByOwner>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Shop not found",
                    Data = null
                };
            }

            return new ApiResponse<GetShopByOwner>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Shop detail retrieved successfully",
                Data = result
            };
        }
        public async Task<ApiResponse<GetShopDetail>> GetShopDetail(Guid shopId)
        {
            try
            {
                using var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                var result = connection.QueryFirstOrDefault<GetShopDetail>(
                    "dbo.sp_GetShopDetail",
                    new
                    {
                        ShopID = shopId
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                if (result == null)
                {
                    return new ApiResponse<GetShopDetail>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                return new ApiResponse<GetShopDetail>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Shop detail retrieved successfully",
                    Data = result
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<ProcedurePagingResponse<GetShopsResponse>>> GetShops(int pageNumber, int pageSize, string? search, string? Filter, bool? IsActive)
        {
            var parameters = new ListParameters<Shop>(pageNumber, pageSize);
            parameters.AddFilter("Search", search);
            parameters.AddFilter("Filter", Filter);
            parameters.AddFilter("IsActive", IsActive);
            var cacheKey = _shopCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetShopsResponse> cachedResponse))
            {
                return new ApiResponse<ProcedurePagingResponse<GetShopsResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Shops retrieved successfully (from cache)",
                    Data = cachedResponse
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetShops",
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Search = search,
                    Filter = Filter,
                    IsActive = IsActive
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (!rows.Any())
            {
                return new ApiResponse<ProcedurePagingResponse<GetShopsResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "No shops found",
                    Data = new ProcedurePagingResponse<GetShopsResponse>
                    {
                        TotalRecord = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Items = new List<GetShopsResponse>()
                    }
                };
            }
            var first = rows.First();
            var totalRecords = (int)first.TotalRecord;

            var response = new ProcedurePagingResponse<GetShopsResponse>
            {
                TotalRecord = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = rows.Select(row => new GetShopsResponse
                {
                    ShopID = row.ShopID,
                    Name = row.Name,
                    Address = row.Address,
                    City = row.City,
                    Province = row.Province,
                    Logo = row.LogoUrl,
                    IsActive = row.IsActive,
                    RatingAverage = row.RatingAverage
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _cache.Set(cacheKey, response, options);
            _shopCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetShopsResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Shops retrieved successfully",
                Data = response
            };
        }
        public async Task<ApiResponse<string>> ShopRegister(ShopRegisterRequest request)
        {
            try
            {
                var checkOwner = await _context.Users
                                               .FirstOrDefaultAsync(x => x.Id == request.UserID);
                if (checkOwner == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Owner not found",
                        Data = null
                    };
                }

                var checkEmail = await _context.Shops
                                                .FirstOrDefaultAsync(x => x.Email == request.Email);
                if (checkEmail != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = "Shop Email already exists",
                        Data = null
                    };
                }

                var checkShopPhone = await _context.Shops
                                                    .FirstOrDefaultAsync(x => x.Phone == request.Phone);
                if (checkShopPhone != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = "Shop Phone already exists",
                        Data = null
                    };
                }

                var newShop = new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Address = request.Address,
                    Phone = request.Phone,
                    Email = request.Email,
                    City = request.City,
                    Province = request.Province,
                    LogoUrl = request.LogoUrl,
                    CoverImageUrl = request.CoverImageUrl,
                    Qrbanking = request.QRBanking,
                    OwnerId = request.UserID,
                    CreatedDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    IsActive = true,
                    RatingAverage = 0
                };

                await _context.Shops.AddAsync(newShop);
                await _context.SaveChangesAsync();
                _shopCache.InvalidateEntityList();
                _productCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Shop registered successfully",
                    Data = newShop.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
