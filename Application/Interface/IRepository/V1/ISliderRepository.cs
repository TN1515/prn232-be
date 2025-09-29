using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Request.Slider;
using Domain.Payload.Response.Slider;

namespace Application.Interface.IRepository.V1
{
    public interface ISliderRepository
    {
        /// <summary>
        /// Lấy danh sách tất cả slider (Admin)
        /// </summary>
        Task<ApiResponse<List<GetSliderResponse>>> GetSliders(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Xem chi tiết 1 slider (Admin)
        /// </summary>
        Task<ApiResponse<SliderDetailResponse>> GetSliderDetail(Guid sliderID);

        /// <summary>
        /// Thêm slider mới (Admin)
        /// </summary>
        Task<ApiResponse<GetSliderResponse>> CreateSlider(CreateSliderRequest request);

        /// <summary>
        /// Cập nhật slider (Admin)
        /// </summary>
        Task<ApiResponse<GetSliderResponse>> UpdateSlider(UpdateSliderRequest request);

        /// <summary>
        /// Xóa slider (Admin)
        /// </summary>
        Task<ApiResponse<bool>> DeleteSlider(Guid sliderID);

        /// <summary>
        /// Đổi thứ tự hiển thị slider (Admin)
        /// </summary>
        Task<ApiResponse<bool>> UpdateSliderOrder(UpdateSliderOrderRequest request);

        /// <summary>
        /// Kích hoạt/Ẩn slider (Admin)
        /// </summary>
        Task<ApiResponse<bool>> ToggleSliderStatus(ToggleSliderStatusRequest request);

        /// <summary>
        /// Lấy danh sách slider đang hiển thị (Customer)
        /// </summary>
        Task<ApiResponse<List<ActiveSliderResponse>>> GetActiveSliders();

        /// <summary>
        /// Lấy slider theo thời gian (Customer)
        /// </summary>
        Task<ApiResponse<List<ActiveSliderResponse>>> GetSlidersByTime(DateTime? startDate = null, DateTime? endDate = null);
    }
}
