using Application.Interface.IRepository.V1;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Advertisement;
using Domain.Payload.Response.Advertisement;
using Domain.Share.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructer.Implement.Repository.V1
{
    public class AdvertisementRepository(DBContext _context) : IAdvertisementRepository
    {
        // Admin APIs
        public async Task<ApiResponse<string>> CreateAdvertisement(CreateAdvertisementRequest request)
        {
            try
            {
                var newAdvertisement = new Advertisement
                {
                    ID = Guid.NewGuid(),
                    MediaUrl = request.MediaUrl,
                    RedirectUrl = request.RedirectUrl,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = request.IsActive,
                    Views = 0,
                    Clicks = 0,
                    CreatedDate = DateTime.Now,
                    ModifyDate = null
                };

                await _context.Advertisements.AddAsync(newAdvertisement);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.Created,
                    Message = "Advertisement created successfully",
                    Data = newAdvertisement.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating advertisement: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<PagingResponse<GetAdvertisementResponse>>> GetAdvertisements(int pageNumber = 1, int pageSize = 10, bool? isActive = null)
        {
            try
            {
                var query = _context.Advertisements.AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new GetAdvertisementResponse
                    {
                        ID = x.ID,
                        MediaUrl = x.MediaUrl,
                        RedirectUrl = x.RedirectUrl,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        IsActive = x.IsActive,
                        Views = x.Views,
                        Clicks = x.Clicks,
                        CreatedDate = x.CreatedDate,
                        ModifyDate = x.ModifyDate
                    })
                    .ToListAsync();

                var pagingResponse = new PagingResponse<GetAdvertisementResponse>(items, pageNumber, pageSize, totalCount);

                return new ApiResponse<PagingResponse<GetAdvertisementResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get advertisements successfully",
                    Data = pagingResponse
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting advertisements: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<AdvertisementDetailResponse>> GetAdvertisementDetail(Guid advertisementId)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == advertisementId);

                if (advertisement == null)
                {
                    return new ApiResponse<AdvertisementDetailResponse>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                var response = new AdvertisementDetailResponse
                {
                    ID = advertisement.ID,
                    MediaUrl = advertisement.MediaUrl,
                    RedirectUrl = advertisement.RedirectUrl,
                    StartDate = advertisement.StartDate,
                    EndDate = advertisement.EndDate,
                    IsActive = advertisement.IsActive,
                    Views = advertisement.Views,
                    Clicks = advertisement.Clicks,
                    CreatedDate = advertisement.CreatedDate,
                    ModifyDate = advertisement.ModifyDate
                };

                return new ApiResponse<AdvertisementDetailResponse>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get advertisement detail successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting advertisement detail: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<string>> UpdateAdvertisement(Guid advertisementId, UpdateAdvertisementRequest request)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == advertisementId);

                if (advertisement == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                if (!string.IsNullOrEmpty(request.MediaUrl))
                    advertisement.MediaUrl = request.MediaUrl;

                if (!string.IsNullOrEmpty(request.RedirectUrl))
                    advertisement.RedirectUrl = request.RedirectUrl;

                if (request.StartDate.HasValue)
                    advertisement.StartDate = request.StartDate.Value;

                if (request.EndDate.HasValue)
                    advertisement.EndDate = request.EndDate.Value;

                advertisement.ModifyDate = DateTime.Now;

                _context.Advertisements.Update(advertisement);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Advertisement updated successfully",
                    Data = advertisement.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating advertisement: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<string>> DeleteAdvertisement(Guid advertisementId)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == advertisementId);

                if (advertisement == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                // Delete associated tracking records first
                var trackings = await _context.AdvertisementTrackings
                    .Where(x => x.AdvertisementID == advertisementId)
                    .ToListAsync();

                _context.AdvertisementTrackings.RemoveRange(trackings);

                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Advertisement deleted successfully",
                    Data = advertisement.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting advertisement: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<string>> ToggleAdvertisementStatus(Guid advertisementId, ToggleAdvertisementStatusRequest request)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == advertisementId);

                if (advertisement == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                advertisement.IsActive = request.IsActive;
                advertisement.ModifyDate = DateTime.Now;

                _context.Advertisements.Update(advertisement);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = $"Advertisement {(request.IsActive ? "activated" : "deactivated")} successfully",
                    Data = advertisement.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling advertisement status: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<PagingResponse<AdvertisementTrackingResponse>>> GetAdvertisementTracking(Guid advertisementId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == advertisementId);

                if (advertisement == null)
                {
                    return new ApiResponse<PagingResponse<AdvertisementTrackingResponse>>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                var totalCount = await _context.AdvertisementTrackings
                    .Where(x => x.AdvertisementID == advertisementId)
                    .CountAsync();

                var items = await _context.AdvertisementTrackings
                    .Where(x => x.AdvertisementID == advertisementId)
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new AdvertisementTrackingResponse
                    {
                        ID = x.ID,
                        AdvertisementID = x.AdvertisementID,
                        TrackingType = x.TrackingType,
                        CreatedDate = x.CreatedDate
                    })
                    .ToListAsync();

                var pagingResponse = new PagingResponse<AdvertisementTrackingResponse>(items, pageNumber, pageSize, totalCount);

                return new ApiResponse<PagingResponse<AdvertisementTrackingResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get advertisement tracking successfully",
                    Data = pagingResponse
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting advertisement tracking: {ex.Message}", ex);
            }
        }

        // Customer APIs
        public async Task<ApiResponse<List<ActiveAdvertisementResponse>>> GetActiveAdvertisements()
        {
            try
            {
                var now = DateTime.Now;

                var items = await _context.Advertisements
                    .Where(x => x.IsActive && (x.StartDate == null || x.StartDate <= now) && (x.EndDate == null || x.EndDate >= now))
                    .Select(x => new ActiveAdvertisementResponse
                    {
                        ID = x.ID,
                        MediaUrl = x.MediaUrl,
                        RedirectUrl = x.RedirectUrl,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Views = x.Views,
                        Clicks = x.Clicks
                    })
                    .ToListAsync();

                return new ApiResponse<List<ActiveAdvertisementResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get active advertisements successfully",
                    Data = items
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting active advertisements: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<string>> RecordImpression(RecordImpressionRequest request)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == request.AdvertisementID);

                if (advertisement == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                // Increment views count
                advertisement.Views = (advertisement.Views ?? 0) + 1;
                _context.Advertisements.Update(advertisement);

                // Record tracking
                var tracking = new AdvertisementTracking
                {
                    ID = Guid.NewGuid(),
                    AdvertisementID = request.AdvertisementID,
                    TrackingType = "View",
                    CreatedDate = DateTime.Now
                };

                await _context.AdvertisementTrackings.AddAsync(tracking);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Impression recorded successfully",
                    Data = tracking.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error recording impression: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse<string>> RecordClick(RecordClickRequest request)
        {
            try
            {
                var advertisement = await _context.Advertisements
                    .FirstOrDefaultAsync(x => x.ID == request.AdvertisementID);

                if (advertisement == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Advertisement not found",
                        Data = null
                    };
                }

                // Increment clicks count
                advertisement.Clicks = (advertisement.Clicks ?? 0) + 1;
                _context.Advertisements.Update(advertisement);

                // Record tracking
                var tracking = new AdvertisementTracking
                {
                    ID = Guid.NewGuid(),
                    AdvertisementID = request.AdvertisementID,
                    TrackingType = "Click",
                    CreatedDate = DateTime.Now
                };

                await _context.AdvertisementTrackings.AddAsync(tracking);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Click recorded successfully",
                    Data = tracking.ID.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error recording click: {ex.Message}", ex);
            }
        }
    }
}
