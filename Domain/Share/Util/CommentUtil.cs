using Domain.Domain.Entities;
using Domain.Payload.Response.Blog;

namespace Domain.Shares.Util
{
    public static class CommentUtil
    {
        /// <summary>
        /// Phân trang các comment gốc (ParentId == null).
        /// </summary>
        public static List<Comment> GetPagedRootComments(List<Comment> allComments, int pageNumber, int pageSize)
        {
            return allComments
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Duyệt và xây cây bình luận nhiều cấp (đệ quy).
        /// </summary>
        public static List<CommentResponse> BuildCommentTree(List<Comment> flatComments, Guid? parentId = null)
        {
            return flatComments
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new CommentResponse
                {
                    ID = c.Id,
                    Content = c.Content,
                    UserID = c.User.Id,
                    Avatar = c.User?.Avatar,
                    DisplayName = c.User.FullName,
                    CreatedDate = c.CreatedDate,
                    HasReplies = flatComments.Any(x => x.ParentId == c.Id),
                })
                .ToList();
        }

        /// <summary>
        /// Kiểm tra xem comment có nằm trong chuỗi reply của bình luận gốc hay không.
        /// </summary>
        public static bool IsDescendantOf(HashSet<Guid> rootIds, Comment comment, List<Comment> allComments)
        {
            if (comment.ParentId == null) return false;
            if (rootIds.Contains(comment.ParentId.Value)) return true;

            var parent = allComments.FirstOrDefault(c => c.Id == comment.ParentId.Value);
            return parent != null && IsDescendantOf(rootIds, parent, allComments);
        }
    }
}
