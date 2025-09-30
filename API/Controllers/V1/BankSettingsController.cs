using Application.Interface.IRepository.V1;
using Domain.Payload.Request.BankSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/bank-settings")]
    public class BankSettingsController(IBankSettingRepository _repository,
                                        ILogger<BankSettingsController> _logger) : Controller
    {
        [HttpPost("add-bank")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> AddNewBank([FromBody] AddBankRequest request)
        {
            try
            {
                var response = await _repository.AddNewBank(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Add New Bank API]" + ex.InnerException.Message);
                return StatusCode(500, ex.InnerException.Message);
            }
        }

        [HttpDelete("detele/{bankID}")]
        public async Task<IActionResult> DeleteBank(Guid bankId)
        {
            try
            {
                var response = await _repository.DeleteBank(bankId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Delete Bank]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPatch("edit/{bankID}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> EditBank(Guid bankID, [FromBody] EditBankRequest request)
        {
            try
            {
                var response = await _repository.EditBank(bankID, request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Edit Bank]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPatch("set-use/{bankID}")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> SetUse(Guid bankID)
        {
            try
            {
                var response = await _repository.SetUseBank(bankID);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Set use API]" + "\n" + ex.InnerException.Message);
                return StatusCode(500, ex.ToString());
            }
        }


        [HttpGet("get-banks")]
        [Authorize(Roles = "Shop")]
        public async Task<IActionResult> GetBanks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? filter = null)
        {
            try
            {
                var response = await _repository.GetBanks(pageNumber, pageSize, search, filter);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Get Banks API]" + "\n" + ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
