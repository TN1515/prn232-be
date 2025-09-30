using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.BankSettings;
using Domain.Payload.Response.BankSettings;

namespace Application.Interface.IRepository.V1
{
    public interface IBankSettingRepository
    {
        Task<ApiResponse<string>> AddNewBank(AddBankRequest reqeuest);
        Task<ApiResponse<string>> DeleteBank(Guid bankId);
        Task<ApiResponse<string>> EditBank(Guid bankId, EditBankRequest request);
        Task<ApiResponse<string>> SetUseBank(Guid bankId);
        Task<ApiResponse<ProcedurePagingResponse<GetBanksResponse>>> GetBanks(int pageNumber, int pageSize, string? search, string? filter);
    }
}
