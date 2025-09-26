using Application.Interface.IRepository.V1;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Request.Like;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class LikeRepository(GenericCacheInvalidator<LikeBlog> _likeCache,
                                GenericCacheInvalidator<Blog> _blogCache,
                                GenericCacheInvalidator<Comment> _commentCache,
                                IMemoryCache _cache,
                                DBContext _context) : ILikeRepository
    {
        public async Task<ApiResponse<string>> LikeOrDislike(LikeRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser());

                var getBlog = await _context.Blogs.FirstOrDefaultAsync(x => x.Id == request.BlogID);
                if (getBlog == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Blog not found",
                        Data = null
                    };
                }

                var getLike = await _context.LikeBlogs.FirstOrDefaultAsync(x => x.BlogId == request.BlogID && x.UserId == userId);
                if (getLike == null)
                {
                    var newLike = new LikeBlog
                    {
                        Id = Guid.NewGuid(),
                        BlogId = request.BlogID,
                        UserId = userId,
                        CreatedDate = DateTime.Now,
                    };

                    await _context.LikeBlogs.AddAsync(newLike);
                    await _context.SaveChangesAsync();

                    _blogCache.InvalidateEntityList();
                    _commentCache.InvalidateEntityList();
                    _likeCache.InvalidateEntityList();

                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Like Blog Success",
                        Data = null
                    };
                }

                _context.LikeBlogs.Remove(getLike);
                await _context.SaveChangesAsync();

                _likeCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _blogCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Dislike blog success",
                    Data = null
                };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.InnerException.Message);
            }
        }
    }
}
