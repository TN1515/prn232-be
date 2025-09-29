using Application.Interface.IRepository.V1;
using Domain.Payload.Request.Slider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/sliders")]
    public class SliderController : ControllerBase
    {
        private readonly ISliderRepository _sliderRepository;

        public SliderController(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository;
        }

        #region Admin Endpoints

        /// <summary>
        /// Lấy danh sách slider (Admin)
        /// </summary>
        [HttpGet("list")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetSliders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _sliderRepository.GetSliders(pageNumber, pageSize);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Xem chi tiết slider (Admin)
        /// </summary>
        [HttpGet("detail/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSliderDetail(Guid id)
        {
            var result = await _sliderRepository.GetSliderDetail(id);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Tạo slider mới (Admin)
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSlider([FromBody] CreateSliderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.ImageUrl))
                return BadRequest("ImageUrl không được để trống");

            var result = await _sliderRepository.CreateSlider(request);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật slider (Admin)
        /// </summary>
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSlider(Guid id, [FromBody] UpdateSliderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.ID)
                return BadRequest("ID không khớp");

            if (string.IsNullOrWhiteSpace(request.ImageUrl))
                return BadRequest("ImageUrl không được để trống");

            var result = await _sliderRepository.UpdateSlider(request);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Xóa slider (Admin)
        /// </summary>
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSlider(Guid id)
        {
            var result = await _sliderRepository.DeleteSlider(id);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Đổi thứ tự hiển thị slider (Admin)
        /// </summary>
        [HttpPut("update-order")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSliderOrder([FromBody] UpdateSliderOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _sliderRepository.UpdateSliderOrder(request);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Kích hoạt / Ẩn slider (Admin)
        /// </summary>
        [HttpPut("toggle-status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleSliderStatus([FromBody] ToggleSliderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _sliderRepository.ToggleSliderStatus(request);
            return StatusCode((int)result.StatusCode, result);
        }

        #endregion

        #region Customer Endpoints

        /// <summary>
        /// Lấy danh sách slider đang hiển thị (Customer)
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetActiveSliders()
        {
            var result = await _sliderRepository.GetActiveSliders();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Lấy slider theo thời gian (Customer)
        /// </summary>
        [HttpGet("by-time")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetSlidersByTime([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _sliderRepository.GetSlidersByTime(startDate, endDate);
            return StatusCode((int)result.StatusCode, result);
        }

        #endregion
    }
}
