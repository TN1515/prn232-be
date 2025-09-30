using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.BankSettings;
using Domain.Payload.Response.BankSettings;
using Domain.Share.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class BankSettingsRepository(GenericCacheInvalidator<BankSettings> _bankCache,
                                        GenericCacheInvalidator<Shop> _shopCache,
                                        GenericCacheInvalidator<Order> _orderCache,
                                        IMemoryCache _cache,
                                        DBContext _context) : IBankSettingRepository
    {
        public async Task<ApiResponse<string>> AddNewBank(AddBankRequest reqeuest)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);
                var getShopByUser = await _context.Shops.FirstOrDefaultAsync(x => x.OwnerId == userId);

                if (getShopByUser == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.OK,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                var checkAlready = await _context.BankSettings.FirstOrDefaultAsync(x => x.BankName.Equals(reqeuest.BankName)
                                                                                   && x.BankNo.Equals(reqeuest.BankNo)
                                                                                   && x.ShopID == getShopByUser.Id);
                if (checkAlready != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "Bank no is already exist",
                        Data = null
                    };
                }

                var newBankSetting = new BankSettings
                {
                    ID = Guid.NewGuid(),
                    BankName = reqeuest.BankName,
                    BankNo = reqeuest.BankNo,
                    IsUse = true,
                    ShopID = getShopByUser.Id,
                };


                await _context.BankSettings.AddAsync(newBankSetting);
                await _context.SaveChangesAsync();

                var getBanks = await _context.BankSettings
                                             .Where(x => x.ShopID == getShopByUser.Id && x.BankNo != reqeuest.BankNo)
                                             .ToListAsync();

                foreach (var bank in getBanks)
                {
                    bank.IsUse = false;
                }

                _context.BankSettings.UpdateRange(getBanks);
                await _context.SaveChangesAsync();

                _bankCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();
                _orderCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Add bank success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public async Task<ApiResponse<string>> DeleteBank(Guid bankId)
        {
            try
            {
                var getBank = await _context.BankSettings.FirstOrDefaultAsync(x => x.ID == bankId);
                if (getBank == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Bank not found",
                        Data = null
                    };
                }

                _context.BankSettings.Remove(getBank);
                await _context.SaveChangesAsync();
                _bankCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();
                _orderCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Delete bank success",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public async Task<ApiResponse<string>> EditBank(Guid bankId, EditBankRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);
                var getShopByUser = await _context.Shops.FirstOrDefaultAsync(x => x.OwnerId == userId);

                if (getShopByUser == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.OK,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                // tìm bank cần edit
                var bankSetting = await _context.BankSettings
                    .FirstOrDefaultAsync(x => x.ID == bankId && x.ShopID == getShopByUser.Id);

                if (bankSetting == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Bank not found",
                        Data = null
                    };
                }

                // check trùng BankName + BankNo trong cùng shop
                var checkAlready = await _context.BankSettings
                    .FirstOrDefaultAsync(x => x.ID != bankId
                                           && x.BankName.Equals(request.BankName)
                                           && x.BankNo.Equals(request.BankNo)
                                           && x.ShopID == getShopByUser.Id);

                if (checkAlready != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "Bank no is already exist",
                        Data = null
                    };
                }

                // update
                bankSetting.BankName = request.BankName;
                bankSetting.BankNo = request.BankNo;

                _context.BankSettings.Update(bankSetting);
                await _context.SaveChangesAsync();

                // clear cache
                _bankCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();
                _orderCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Edit bank success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetBanksResponse>>> GetBanks(int pageNumber, int pageSize, string? search, string? filter)
        {
            Guid userId = Guid.Parse(JWTUtil.GetUser());
            var paramters = new ListParameters<BankSettings>(pageNumber, pageSize);
            paramters.AddFilter("search", search);
            paramters.AddFilter("filter", filter);

            //var cacheKey = _bankCache.GetCacheKeyForList(paramters);
            //if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetBanksResponse> cacheData))
            //{
            //    return new ApiResponse<ProcedurePagingResponse<GetBanksResponse>>
            //    {
            //        StatusCode = StatusCode.OK,
            //        Message = "Get Banks Success(cache)",
            //        Data = cacheData
            //    };
            //}

            var getShop = await _context.Shops.FirstOrDefaultAsync(x => x.OwnerId == userId);

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }


            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetBanks",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    search = search,
                    filter = filter,
                    shopID = getShop.Id
                },
                commandType: System.Data.CommandType.StoredProcedure
                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetBanksResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Not Found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetBanksResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetBanksResponse
                {
                    ID = x.ID,
                    BankName = x.BankName,
                    BankNo = x.BankNo,
                    IsUse = x.IsUse
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            //_cache.Set(cacheKey, response, options);
            //_bankCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetBanksResponse>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get Banks Success",
                Data = response
            };
        }

        public async Task<ApiResponse<string>> SetUseBank(Guid bankId)
        {
            try
            {
                var getBank = await _context.BankSettings.FirstOrDefaultAsync(x => x.ID == bankId);
                if (getBank == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Bank not found",
                        Data = null
                    };
                }

                getBank.IsUse = true;

                var getBanks = await _context.BankSettings.Where(x => x.ShopID == getBank.ShopID && x.ID != bankId).ToListAsync();
                foreach (var bank in getBanks)
                {
                    bank.IsUse = false;
                }

                _context.BankSettings.UpdateRange(getBanks);
                await _context.SaveChangesAsync();

                _bankCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();
                _orderCache.InvalidateEntityList();

                return new ApiResponse<string>
                {

                    StatusCode = StatusCode.OK,
                    Message = "Set bank use success",
                    Data = null

                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message ?? ex.StackTrace);
            }
        }

    }
}
