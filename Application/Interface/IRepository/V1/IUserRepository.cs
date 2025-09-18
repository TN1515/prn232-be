using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Response.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepository.V1
{
    public interface IUserRepository
    {
        Task<ApiResponse<string>> BlockOrUnBlock(Guid userId);
        Task<ApiResponse<ProcedurePagingResponse<GetUsersResponse>>> GetUsers(int pageNumber,int pageSize, string?search, string?filter);
    }
}
