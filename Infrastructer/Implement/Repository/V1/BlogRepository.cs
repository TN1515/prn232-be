using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Blogs;
using Domain.Payload.Response.Blogs;
using Domain.Share.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class BlogRepository(DBContext _context,
                                GenericCacheInvalidator<Blog> _blogCache,
                                GenericCacheInvalidator<Comment> _commentCache,
                                GenericCacheInvalidator<LikeBlog> _likeCache,
                                IMemoryCache _cache) : IBlogRepository
    {
        public async Task<ApiResponse<string>> CreateBlog(CreateBlogRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(JWTUtil.GetUser()));
                var newblog = new Blog
                {
                    Id = Guid.NewGuid(),
                    Title = HTMLUtil.Sanitize(request.Title),
                    Content = HTMLUtil.Sanitize(request.Content),
                    Image = request.Image,
                    Author = user.Id,
                    PublishDate = DateTime.Now,
                    ModifyDate = null,
                    IsActive = true
                };

                await _context.Blogs.AddAsync(newblog);
                await _context.SaveChangesAsync();

                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Create Blog Success",
                    Data = newblog.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.InnerException.Message

                };
            }
        }

        public async Task<ApiResponse<string>> DeleteBlog(Guid blogID)
        {
            try
            {
                var getBlog = await _context.Blogs.FirstOrDefaultAsync(x => x.Id == blogID);
                if (getBlog == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Blog not found",
                        Data = null
                    };
                }

                getBlog.IsActive = false;

                _context.Blogs.Update(getBlog);
                await _context.SaveChangesAsync();

                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache?.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Delete Blog Success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
            }
        }

        public async Task<ApiResponse<string>> EditBlog(Guid blogId, EditBlogRequest request)
        {
            try
            {
                var userId = Guid.Parse(JWTUtil.GetUser());

                var getBlog = await _context.Blogs.FirstOrDefaultAsync(x => x.Id == blogId);
                if (getBlog == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Blog not found",
                        Data = null
                    };
                }

                if (getBlog != null && getBlog.Author != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You don't have permission",
                        Data = null
                    };
                }

                getBlog.Title = HTMLUtil.Sanitize(request.Content) ?? getBlog.Content;
                getBlog.Content = HTMLUtil.Sanitize(request.Content) ?? getBlog.Content;
                getBlog.Image = request.Image ?? getBlog.Image;
                getBlog.ModifyDate = DateTime.Now;
                getBlog.Author = getBlog.Author;
                getBlog.IsActive = true;


                _context.Blogs.Update(getBlog);
                await _context.SaveChangesAsync();

                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Edit blog success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = ex.StackTrace
                };
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetBlogs>>> GetBlogs(int pageNumber, int pageSize, string? search, string? filter)
        {
            var parameters = new ListParameters<Blog>(pageNumber, pageSize);
            parameters.AddFilter("search", search);
            parameters.AddFilter("filter", filter);

            var cacheKey = _blogCache.GetCacheKeyForList(parameters);
            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetBlogs> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get Blogs Success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetBlogs",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    search = search,
                    filter = filter
                },

                commandType: System.Data.CommandType.StoredProcedure
                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data not found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetBlogs>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetBlogs
                {
                    ID = x.ID,
                    Title = x.Title,
                    Image = x.Image,
                    Author = x.Author,
                    PublishDate = x.PublishDate,
                    TotalLike = x.TotalLike,
                    TotalComment = x.TotalComment

                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, options);
            _blogCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get blogs success",
                Data = response
            };

        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetBlogs>>> GetBlogsByAuthor(
         int pageNumber, int pageSize, string? search, string? filter)
        {
            var authorId = Guid.Parse(JWTUtil.GetUser()!);

            var parameters = new ListParameters<Blog>(pageNumber, pageSize);
            parameters.AddFilter("authorId", authorId.ToString());
            parameters.AddFilter("search", search);
            parameters.AddFilter("filter", filter);

            var cacheKey = _blogCache.GetCacheKeyForList(parameters);
            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetBlogs> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get Blogs By Author Success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetBlogsByAuthor",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    authorId = authorId,
                    search = search,
                    filter = filter
                },
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (!rows.Any())
            {
                return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Data not found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetBlogs>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetBlogs
                {
                    ID = x.ID,
                    Title = x.Title,
                    Image = x.Image,
                    Author = x.Author,
                    PublishDate = x.PublishDate,
                    TotalLike = x.TotalLike,
                    TotalComment = x.TotalComment
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, options);
            _blogCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetBlogs>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get blogs by author success",
                Data = response
            };
        }

        public async Task<ApiResponse<GetDetail>> GetDetail(Guid blogID)
        {
            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var result = await connection.QueryFirstOrDefaultAsync<GetDetail>(
                "dbo.sp_GetBlogDetail",
                new
                {
                    BlogID = blogID
                },
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == null)
            {
                return new ApiResponse<GetDetail>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Blog not found",
                    Data = null
                };
            }

            return new ApiResponse<GetDetail>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Detail Success",
                Data = result
            };
        }

    }
}
