using Application.Interface.IRepository.V1;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Comments;
using Domain.Payload.Response.Blog;
using Domain.Share.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class CommentRepository(GenericCacheInvalidator<Blog> _blogCache,
                                   GenericCacheInvalidator<Comment> _commentCache,
                                   GenericCacheInvalidator<LikeBlog> _likeCache,
                                   IMemoryCache _cache,
                                   DBContext _context) : ICommentRepository
    {
        public async Task<ApiResponse<string>> Comment(CommentRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser());


                var checkBlog = await _context.Blogs.FirstOrDefaultAsync(x => x.Id == request.BlogID && x.IsActive == true);
                if (checkBlog == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Blog not found",
                        Data = null
                    };
                }

                var newComment = new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = HTMLUtil.Sanitize(request.Content),
                    CreatedDate = DateTime.Now,
                    ParentId = null,
                    ModifyDate = DateTime.Now,
                    BlogId = request.BlogID,
                    UserId = userId
                };

                await _context.Comments.AddAsync(newComment);
                await _context.SaveChangesAsync();

                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Comment Blog Success",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }
        private async Task<List<Comment>> GetAllRepliesAsync(Guid commentId)
        {
            var replies = await _context.Comments
                .Where(c => c.ParentId == commentId)
                .ToListAsync();

            var allReplies = new List<Comment>(replies);

            foreach (var reply in replies)
            {
                var subReplies = await GetAllRepliesAsync(reply.Id);
                allReplies.AddRange(subReplies);
            }

            return allReplies;
        }
        public async Task<ApiResponse<string>> DeleteComment(Guid commentID)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser());

                var getComment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentID);
                if (getComment == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Comment Not Found",
                        Data = null
                    };
                }

                if (getComment.UserId != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You don't have permission",
                        Data = null
                    };
                }

                // --- Lấy toàn bộ comment con liên quan ---
                var allReplies = await GetAllRepliesAsync(commentID);

                // --- Xóa toàn bộ replies trước ---
                _context.Comments.RemoveRange(allReplies);

                // --- Xóa comment gốc ---
                _context.Comments.Remove(getComment);

                await _context.SaveChangesAsync();

                _commentCache.InvalidateEntityList();
                _blogCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Delete comment and all related replies successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
        public async Task<ApiResponse<string>> EditComment(Guid commentID, EditCommentRequest request)
        {
            Guid userId = Guid.Parse(JWTUtil.GetUser());
            try
            {
                var getComment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentID);
                if (getComment == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Comment Not Found",
                        Data = null
                    };
                }

                if (getComment.UserId != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You don't have permission",
                        Data = null
                    };
                }

                getComment.Content = HTMLUtil.Sanitize(request.Content) ?? getComment.Content;
                getComment.ModifyDate = DateTime.Now;
                _context.Comments.Update(getComment);
                await _context.SaveChangesAsync();
                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Edit comment success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public async Task<ApiResponse<string>> RepliesComment(RepliesCommentRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser());

                var checkComment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == request.CommentID);
                if (checkComment == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Comment not found",
                        Data = null
                    };
                }

                var replies = new Comment
                {
                    Id = Guid.NewGuid(),
                    ParentId = request.CommentID,
                    Content = HTMLUtil.Sanitize(request.Content),
                    CreatedDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    UserId = userId,
                    BlogId = checkComment.BlogId
                };

                await _context.Comments.AddAsync(replies);
                await _context.SaveChangesAsync();

                _blogCache.InvalidateEntityList();
                _commentCache.InvalidateEntityList();
                _likeCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Replies comment success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }
        }

        public async Task<ApiResponse<PagingResponse<CommentResponse>>> GetComments(Guid blogId, int pageNumber, int pageSize, int repliesPage, int repliesSize)
        {
            var parameters = new ListParameters<Comment>(pageNumber, pageSize);
            parameters.AddFilter("blogId", blogId);
            parameters.AddFilter("repliesPage", repliesPage);
            parameters.AddFilter("repliesSize", repliesSize);

            var cacheKey = _commentCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey, out PagingResponse<CommentResponse> comments))
            {
                return new ApiResponse<PagingResponse<CommentResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Lấy bình luận thành công(cache)",
                    Data = comments
                };
            }

            // --- Lấy comment gốc ---
            var rootComment = _context.Comments
                                      .Include(x => x.User)
                                      .Where(c => c.BlogId == blogId && c.ParentId == null);

            var totalRoots = await rootComment.CountAsync();

            // --- Đếm toàn bộ comment (gồm cả reply) ---
            var totalComments = await _context.Comments
                                              .Where(c => c.BlogId == blogId)
                                              .CountAsync();

            var rootComments = await rootComment
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var rootIds = rootComments.Select(c => c.Id).ToList();

            var replies = await _context.Comments
                                        .Where(c => c.ParentId != null && rootIds.Contains(c.ParentId.Value))
                                        .Include(c => c.User)
                                        .ToListAsync();

            var responseItems = rootComments.Select(root =>
            {
                var repliesOfRoot = replies
                    .Where(r => r.ParentId == root.Id)
                    .OrderBy(r => r.CreatedDate);

                var pagedReplies = repliesOfRoot
                    .Skip((repliesPage - 1) * repliesSize)
                    .Take(repliesSize)
                    .Select(r => new RepliesResponse
                    {
                        ID = r.Id,
                        Content = r.Content,
                        Avatar = r.User?.Avatar,
                        UserID = r.User.Id,
                        DisplayName = r.User.FullName ?? "Chưa cập nhật",
                        CreatedDate = r.CreatedDate,
                        HasReplies = replies.Any(x => x.ParentId == r.Id)
                    })
                    .ToList();

                return new CommentResponse
                {
                    ID = root.Id,
                    Content = root.Content,
                    UserID = root.User.Id,
                    Avatar = root.User?.Avatar,
                    DisplayName = root.User.FullName,
                    CreatedDate = root.CreatedDate,
                    HasReplies = repliesOfRoot.Any(),
                    TotalReplies = repliesOfRoot.Count(),
                    Replies = pagedReplies
                };
            }).ToList();

            var pagedResult = new PagingResponse<CommentResponse>(responseItems, pageNumber, pageSize, totalRoots);

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            _cache.Set(cacheKey, pagedResult, options);
            _commentCache.AddToListCacheKeys(cacheKey);
            _blogCache.InvalidateEntityList();
            _likeCache.InvalidateEntityList();

            return new ApiResponse<PagingResponse<CommentResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"Lấy bình luận thành công (Tổng cộng {totalComments} bình luận, gồm cả phản hồi)",
                Data = pagedResult
            };
        }

    }
}
