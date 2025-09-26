using Domain.Payload.Base;
using Domain.Payload.Request.Like;

namespace Application.Interface.IRepository.V1
{
    public interface ILikeRepository
    {
        Task<ApiResponse<string>> LikeOrDislike(LikeRequest request);
    }
}
