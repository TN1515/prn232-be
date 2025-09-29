using Application.Interface.IRepository.V1;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Request.Slider;
using Domain.Payload.Response.Slider;
using Domain.Share.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructer.Implement.Repository.V1
{
    public class SliderRepository : ISliderRepository
    {
        private readonly DBContext _context;

        public SliderRepository(DBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả slider (Admin)
        /// </summary>
        public async Task<ApiResponse<List<GetSliderResponse>>> GetSliders(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var sliders = await _context.Sliders
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new GetSliderResponse
                    {
                        ID = s.ID,
                        Description = s.Description,
                        ImageUrl = s.ImageUrl,
                        LinkUrl = s.LinkUrl,
                        OrderIndex = s.OrderIndex,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        IsActive = s.IsActive,
                        CreatedDate = s.CreatedDate,
                        ModifyDate = s.ModifyDate
                    })
                    .ToListAsync();

                return new ApiResponse<List<GetSliderResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Lấy danh sách slider thành công",
                    Data = sliders
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GetSliderResponse>>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi lấy danh sách slider: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Xem chi tiết 1 slider (Admin)
        /// </summary>
        public async Task<ApiResponse<SliderDetailResponse>> GetSliderDetail(Guid sliderID)
        {
            try
            {
                var slider = await _context.Sliders
                    .FirstOrDefaultAsync(x => x.ID == sliderID);

                if (slider == null)
                {
                    return new ApiResponse<SliderDetailResponse>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Slider không tồn tại",
                        Data = null
                    };
                }

                var response = new SliderDetailResponse
                {
                    ID = slider.ID,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    LinkUrl = slider.LinkUrl,
                    OrderIndex = slider.OrderIndex,
                    StartDate = slider.StartDate,
                    EndDate = slider.EndDate,
                    IsActive = slider.IsActive,
                    CreatedDate = slider.CreatedDate,
                    ModifyDate = slider.ModifyDate
                };

                return new ApiResponse<SliderDetailResponse>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Lấy chi tiết slider thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SliderDetailResponse>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi lấy chi tiết slider: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Thêm slider mới (Admin)
        /// </summary>
        public async Task<ApiResponse<GetSliderResponse>> CreateSlider(CreateSliderRequest request)
        {
            try
            {
                var slider = new Slider
                {
                    ID = Guid.NewGuid(),
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    LinkUrl = request.LinkUrl,
                    OrderIndex = request.OrderIndex,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = request.IsActive,
                    CreatedDate = DateTime.Now,
                    ModifyDate = null
                };

                _context.Sliders.Add(slider);
                await _context.SaveChangesAsync();

                var response = new GetSliderResponse
                {
                    ID = slider.ID,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    LinkUrl = slider.LinkUrl,
                    OrderIndex = slider.OrderIndex,
                    StartDate = slider.StartDate,
                    EndDate = slider.EndDate,
                    IsActive = slider.IsActive,
                    CreatedDate = slider.CreatedDate,
                    ModifyDate = slider.ModifyDate
                };

                return new ApiResponse<GetSliderResponse>
                {
                    StatusCode = StatusCode.Created,
                    Message = "Tạo slider thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<GetSliderResponse>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi tạo slider: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Cập nhật slider (Admin)
        /// </summary>
        public async Task<ApiResponse<GetSliderResponse>> UpdateSlider(UpdateSliderRequest request)
        {
            try
            {
                var slider = await _context.Sliders
                    .FirstOrDefaultAsync(x => x.ID == request.ID);

                if (slider == null)
                {
                    return new ApiResponse<GetSliderResponse>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Slider không tồn tại",
                        Data = null
                    };
                }

                slider.Description = request.Description;
                slider.ImageUrl = request.ImageUrl;
                slider.LinkUrl = request.LinkUrl;
                slider.OrderIndex = request.OrderIndex;
                slider.StartDate = request.StartDate;
                slider.EndDate = request.EndDate;
                slider.IsActive = request.IsActive;
                slider.ModifyDate = DateTime.Now;

                _context.Sliders.Update(slider);
                await _context.SaveChangesAsync();

                var response = new GetSliderResponse
                {
                    ID = slider.ID,
                    Description = slider.Description,
                    ImageUrl = slider.ImageUrl,
                    LinkUrl = slider.LinkUrl,
                    OrderIndex = slider.OrderIndex,
                    StartDate = slider.StartDate,
                    EndDate = slider.EndDate,
                    IsActive = slider.IsActive,
                    CreatedDate = slider.CreatedDate,
                    ModifyDate = slider.ModifyDate
                };

                return new ApiResponse<GetSliderResponse>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Cập nhật slider thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<GetSliderResponse>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi cập nhật slider: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Xóa slider (Admin)
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteSlider(Guid sliderID)
        {
            try
            {
                var slider = await _context.Sliders
                    .FirstOrDefaultAsync(x => x.ID == sliderID);

                if (slider == null)
                {
                    return new ApiResponse<bool>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Slider không tồn tại",
                        Data = false
                    };
                }

                _context.Sliders.Remove(slider);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Xóa slider thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi xóa slider: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Đổi thứ tự hiển thị slider (Admin)
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateSliderOrder(UpdateSliderOrderRequest request)
        {
            try
            {
                var slider = await _context.Sliders
                    .FirstOrDefaultAsync(x => x.ID == request.SliderID);

                if (slider == null)
                {
                    return new ApiResponse<bool>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Slider không tồn tại",
                        Data = false
                    };
                }

                slider.OrderIndex = request.NewOrderIndex;
                slider.ModifyDate = DateTime.Now;

                _context.Sliders.Update(slider);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Cập nhật thứ tự slider thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi cập nhật thứ tự slider: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Kích hoạt/Ẩn slider (Admin)
        /// </summary>
        public async Task<ApiResponse<bool>> ToggleSliderStatus(ToggleSliderStatusRequest request)
        {
            try
            {
                var slider = await _context.Sliders
                    .FirstOrDefaultAsync(x => x.ID == request.ID);

                if (slider == null)
                {
                    return new ApiResponse<bool>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Slider không tồn tại",
                        Data = false
                    };
                }

                slider.IsActive = request.IsActive;
                slider.ModifyDate = DateTime.Now;

                _context.Sliders.Update(slider);
                await _context.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.OK,
                    Message = request.IsActive ? "Kích hoạt slider thành công" : "Ẩn slider thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi cập nhật trạng thái slider: {ex.Message}",
                    Data = false
                };
            }
        }

        /// <summary>
        /// Lấy danh sách slider đang hiển thị (Customer)
        /// </summary>
        public async Task<ApiResponse<List<ActiveSliderResponse>>> GetActiveSliders()
        {
            try
            {
                var now = DateTime.Now;

                var sliders = await _context.Sliders
                    .Where(x => x.IsActive &&
                                (x.StartDate == null || x.StartDate <= now) &&
                                (x.EndDate == null || x.EndDate >= now))
                    .OrderBy(x => x.OrderIndex)
                    .ThenByDescending(x => x.CreatedDate)
                    .Select(s => new ActiveSliderResponse
                    {
                        ID = s.ID,
                        Description = s.Description,
                        ImageUrl = s.ImageUrl,
                        LinkUrl = s.LinkUrl,
                        OrderIndex = s.OrderIndex,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate
                    })
                    .ToListAsync();

                return new ApiResponse<List<ActiveSliderResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Lấy danh sách slider đang hiển thị thành công",
                    Data = sliders
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ActiveSliderResponse>>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi lấy danh sách slider đang hiển thị: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Lấy slider theo thời gian (Customer)
        /// </summary>
        public async Task<ApiResponse<List<ActiveSliderResponse>>> GetSlidersByTime(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Sliders
                    .Where(x => x.IsActive)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(x => x.StartDate == null || x.StartDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(x => x.EndDate == null || x.EndDate <= endDate.Value);
                }

                var sliders = await query
                    .OrderBy(x => x.OrderIndex)
                    .ThenByDescending(x => x.CreatedDate)
                    .Select(s => new ActiveSliderResponse
                    {
                        ID = s.ID,
                        Description = s.Description,
                        ImageUrl = s.ImageUrl,
                        LinkUrl = s.LinkUrl,
                        OrderIndex = s.OrderIndex,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate
                    })
                    .ToListAsync();

                return new ApiResponse<List<ActiveSliderResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Lấy slider theo thời gian thành công",
                    Data = sliders
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ActiveSliderResponse>>
                {
                    StatusCode = StatusCode.InternalServerError,
                    Message = $"Lỗi khi lấy slider theo thời gian: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
