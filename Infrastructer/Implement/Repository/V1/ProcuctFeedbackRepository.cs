using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.ProductFeedbacks;
using Domain.Payload.Response.ProductFeedbaks;
using Domain.Share.Common;
using Domain.Share.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class ProcuctFeedbackRepository(GenericCacheInvalidator<Product> _procuctCache,
                                           GenericCacheInvalidator<ProductFeedback> _feedbackCache,
                                           IMemoryCache _cache,
                                           DBContext _context) : IProductFeedbacksRepository
    {
        public async Task<ApiResponse<string>> DeleteFeedback(Guid feebackID)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);
                var getFeedback = await _context.ProductFeedbacks.FirstOrDefaultAsync(x => x.Id == feebackID);
                if (getFeedback == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Feedback not found",
                        Data = null
                    };
                }

                else if (getFeedback != null && getFeedback.UserId != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You don't have permission",
                        Data = null
                    };
                }

                _context.ProductFeedbacks.Remove(getFeedback);
                await _context.SaveChangesAsync();

                _feedbackCache.InvalidateEntityList();
                _procuctCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Delete Feedback Success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> EditFeedback(Guid feedbackID, EditFeedbackRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);

                var getFeedback = await _context.ProductFeedbacks.FirstOrDefaultAsync(x => x.Id == feedbackID);
                if (getFeedback == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Feedback not found",
                        Data = null
                    };
                }

                else if (getFeedback != null && getFeedback.UserId != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You don't have permission",
                        Data = null
                    };
                }

                getFeedback.Content = HTMLUtil.Sanitize(getFeedback.Content) ?? getFeedback.Content;
                getFeedback.Rating = request.Rating ?? getFeedback.Rating;
                getFeedback.ModifyDate = DateTime.Now;


                _context.ProductFeedbacks.Update(getFeedback);
                await _context.SaveChangesAsync();

                _feedbackCache.InvalidateEntityList();
                _procuctCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Edit Feedbacks success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> FeedBacks(FeedbackRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser());
                var checkFeedbacks = await _context.ProductFeedbacks.FirstOrDefaultAsync(x => x.ProductId == request.ProductID && x.UserId == userId);

                if (checkFeedbacks != null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Conflict,
                        Message = "You feedback already exsit",
                        Data = null

                    };
                }

                var checkProduct = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductID);
                if (checkProduct == null)
                {

                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Product not found",
                        Data = null
                    };
                }

                var feedbacks = new ProductFeedback
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductID,
                    UserId = userId,
                    Content = HTMLUtil.Sanitize(request.Content),
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    ModifyDate = DateTime.Now,
                    Rating = request.Rating
                };

                await _context.ProductFeedbacks.AddAsync(feedbacks);
                await _context.SaveChangesAsync();

                _procuctCache.InvalidateEntityList();
                _feedbackCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Feedback Success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetFeedbacksResponse>>> GetFeedbacks(Guid productID, int pageNumber, int pageSize, decimal? ratingFilter)
        {
            var parameters = new ListParameters<GetFeedbacksResponse>(pageNumber, pageSize);
            parameters.AddFilter("ratingFilter", ratingFilter);

            var cacheKey = _feedbackCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetFeedbacksResponse> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetFeedbacksResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get Feedbacks Success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetProductFeedbacks",
                new
                {
                    ProductID = productID,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    RatingFilter = ratingFilter
                },
                commandType: System.Data.CommandType.StoredProcedure
                );
            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetFeedbacksResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Not Found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetFeedbacksResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetFeedbacksResponse
                {
                    ID = x.ID,
                    Content = x.Content,
                    Rating = x.Rating,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    UserID = x.UserID,
                    FullName = x.FullName ?? "Chưa cập nhật",
                    Avatar = x.Avatar ?? " Chưa cập nhật",
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
            };

            _cache.Set(cacheKey, response, options);


            return new ApiResponse<ProcedurePagingResponse<GetFeedbacksResponse>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get Product Feedbacks Success",
                Data = response
            };
        }
    }
}
