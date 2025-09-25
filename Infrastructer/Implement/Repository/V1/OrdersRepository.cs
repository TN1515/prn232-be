using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Entities.Enum;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Orders;
using Domain.Payload.Response.Orders;
using Domain.Share.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SePaySerivce.PayLoad.Request;
using SePaySerivce.PayLoad.Response;
using SePaySerivce.Service;

namespace Infrastructer.Implement.Repository.V1
{
    public class OrdersRepository(DBContext _context,
                                  GenericCacheInvalidator<Order> _orderCache,
                                  GenericCacheInvalidator<OrderDetail> _detailCache,
                                  GenericCacheInvalidator<Cart> _cartCache,
                                  GenericCacheInvalidator<CartItem> _cartItemCache,
                                  IMemoryCache _cache,
                                  ISepayQRService _sepayService) : IOrdersRepository
    {
        public async Task<ApiResponse<string>> CancelOrder(Guid orderID)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderID);

                if (order == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Không tìm thấy đơn hàng",
                        Data = null
                    };
                }

                if (order.Status == OrderStatusEnum.Cancelled.ToString())
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Đơn hàng đã bị hủy trước đó",
                        Data = null
                    };
                }

                // Hoàn trả lại stock cho từng sản phẩm trong chi tiết đơn hàng
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Product != null)
                    {
                        detail.Product.Stock += detail.Quantity;
                        _context.Products.Update(detail.Product);
                    }
                }

                // Đổi trạng thái đơn hàng sang Canceled
                order.Status = OrderStatusEnum.Cancelled.ToString();
                order.ModifyDate = DateTime.Now;
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Invalidate cache
                _orderCache.InvalidateEntityList();
                _detailCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();
                _cartItemCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Hủy đơn hàng thành công",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi hủy đơn hàng: {ex.Message}",
                    Data = null
                };
            }
        }


        public async Task<ApiResponse<List<Guid>>> CreateOrder(CreateOrderRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = Guid.Parse(JWTUtil.GetUser()!);

                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .ThenInclude(s => s.Shop)
                    .Where(ci => request.CartItemIDs.Contains(ci.Id))
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return new ApiResponse<List<Guid>>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Không tìm thấy sản phẩm trong giỏ hàng"
                    };
                }

                var groupedByShop = cartItems.GroupBy(ci => ci.Product.ShopID);

                var createdOrderIds = new List<Guid>();

                foreach (var group in groupedByShop)
                {
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = CommonUtil.GenerateOrderNo(),
                        UserId = userId,
                        TotalAmounts = 0,
                        Status = OrderStatusEnum.WaitForPayment.ToString(),
                        DeliveryAddress = request.DeliveryAddress,
                        ReceiverName = request.ReceiverName,
                        ReceiverPhone = request.ReceiverPhone,
                        CreatedDate = DateTime.Now,
                        ModifyDate = DateTime.Now,
                        IsActive = true,
                        PaymentMethod = request.PaymentMethod,
                        OrderDetails = new List<OrderDetail>()
                    };

                    foreach (var cartItem in group)
                    {
                        if (cartItem.Quantity > cartItem.Product.Stock)
                        {
                            return new ApiResponse<List<Guid>>
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                Message = $"{cartItem.Product.Name} chỉ còn {cartItem.Product.Stock} sản phẩm trong kho"
                            };
                        }

                        var detail = new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId ?? Guid.Empty,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            TotalAmounts = cartItem.UnitPrice * cartItem.Quantity,
                            Note = HTMLUtil.Sanitize(cartItem.Note)
                        };

                        order.TotalAmounts += detail.TotalAmounts;

                        // Giảm stock
                        cartItem.Product.Stock -= cartItem.Quantity;
                        _context.Products.Update(cartItem.Product);

                        order.OrderDetails.Add(detail);

                        // Xóa cartItem
                        _context.CartItems.Remove(cartItem);
                    }

                    await _context.Orders.AddAsync(order);
                    createdOrderIds.Add(order.Id);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _orderCache.InvalidateEntityList();
                _cartItemCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();
                _detailCache.InvalidateEntityList();

                return new ApiResponse<List<Guid>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = $"Tạo {createdOrderIds.Count} đơn hàng thành công",
                    Data = createdOrderIds
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<List<Guid>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi tạo đơn hàng: {ex.ToString()}"
                };
            }
        }
        public async Task<ApiResponse<CreateQrResponse>> CreateQRPayment(Guid orderID)
        {
            try
            {
                var getOrder = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Shop)
                                .ThenInclude(s => s.BankSettings)
                    .FirstOrDefaultAsync(o => o.Id == orderID);

                if (getOrder == null)
                {
                    return new ApiResponse<CreateQrResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Order not found",
                        Data = null
                    };
                }

                var shop = getOrder.OrderDetails
                                   .Select(od => od.Product.Shop)
                                   .FirstOrDefault();

                if (shop == null)
                {
                    return new ApiResponse<CreateQrResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                var bank = shop.BankSettings.FirstOrDefault(bs => bs.IsUse);

                if (bank == null)
                {
                    return new ApiResponse<CreateQrResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Bank setting not found",
                        Data = null
                    };
                }

                var qrRequest = new CreateQRCode
                {
                    BankAccount = bank.BankNo,
                    BankCode = bank.BankName,
                    Amount = getOrder.TotalAmounts ?? 0,
                    Description = $"Thanh toan don hang {getOrder.OrderCode}"
                };

                var qrResponse = _sepayService.GenerateQR(qrRequest);

                return new ApiResponse<CreateQrResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = qrResponse
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> EditOrderStatus(Guid orderID, OrderStatusEnum status)
        {
            try
            {
                var getOrder = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderID);
                if (getOrder == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Order not found",
                        Data = null
                    };
                }

                getOrder.Status = status.ToString();
                _context.Orders.Update(getOrder);
                await _context.SaveChangesAsync();
                _orderCache.InvalidateEntityList();
                _detailCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();
                _cartItemCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Edit order status success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> FastOrder(CreateFastOrderRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);

                var getProduct = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductID);
                if (getProduct == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Product not found",
                        Data = null
                    };
                }

                if (getProduct.Stock < request.Quantity)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Số lượng sản phẩm trong kho không đủ",
                        Data = null
                    };
                }

                var newOrder = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = CommonUtil.GenerateOrderNo(),
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    ReceiverName = request.ReceiverName,
                    ReceiverPhone = request.ReceiverPhone,
                    DeliveryAddress = request.DeliveryAddress,
                    IsActive = true,
                    PaymentMethod = request.PaymentMethod,
                    Status = OrderStatusEnum.WaitForPayment.ToString(),
                    ModifyDate = DateTime.Now,
                    TotalAmounts = 0,
                    OrderDetails = new List<OrderDetail>()
                };

                var newOrderDetail = new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = newOrder.Id,
                    Note = HTMLUtil.Sanitize(request.Note),
                    ProductId = getProduct.Id,
                    UnitPrice = getProduct.Price,
                    Quantity = request.Quantity,
                    TotalAmounts = getProduct.Price * request.Quantity
                };

                // Cập nhật stock
                getProduct.Stock -= request.Quantity;
                _context.Products.Update(getProduct);

                // Cập nhật tổng tiền order
                newOrder.TotalAmounts = newOrderDetail.TotalAmounts;
                newOrder.OrderDetails.Add(newOrderDetail);

                await _context.Orders.AddAsync(newOrder);
                await _context.OrderDetails.AddAsync(newOrderDetail);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Invalidate cache
                _orderCache.InvalidateEntityList();
                _detailCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();
                _cartItemCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Tạo đơn hàng nhanh thành công",
                    Data = newOrder.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi tạo đơn hàng nhanh: {ex.Message}",
                    Data = null
                };
            }
        }


        public async Task<ApiResponse<ProcedurePagingResponse<GetOrderDetailsResponse>>> GetDetails(Guid orderId, int pageNumber, int pageSize)
        {
            var parameters = new ListParameters<OrderDetail>(pageNumber, pageSize);
            parameters.AddFilter("OrderId", orderId);

            var cacheKey = _detailCache.GetCacheKeyForList(parameters);
            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetOrderDetailsResponse> cacheResponse))
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrderDetailsResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get Details Success(cache)",
                    Data = cacheResponse
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetDetails",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    orderId = orderId
                },
                commandType: System.Data.CommandType.StoredProcedure
                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrderDetailsResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Details Not Found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetOrderDetailsResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetOrderDetailsResponse
                {
                    ID = x.ID,
                    OrderID = x.OrderID,
                    ProductID = x.ProductID,
                    ProductName = x.ProductName,
                    CommonImage = x.CommonImage,
                    Description = x.Description,
                    Category = x.Category,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalAmounts = x.TotalAmounts,
                    Note = x.Note
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, options);
            _detailCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetOrderDetailsResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Details Success",
                Data = response
            };
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>> GetOrderByShop(int pageNumber, int pageSize, string? Filter)
        {
            var userId = Guid.Parse(JWTUtil.GetUser()!);
            var getShop = await _context.Shops
                                        .Where(x => x.OwnerId == userId)
                                        .Select(x => x.Id)
                                        .FirstOrDefaultAsync();

            var parameters = new ListParameters<Order>(pageNumber, pageSize);
            parameters.AddFilter("Filter", Filter);
            parameters.AddFilter("ShopId", getShop);

            var cacheKey = _orderCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetOrdersResponse> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get orders success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetOrdersByShop",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    Filter = Filter,
                    ShopId = getShop
                },
                commandType: System.Data.CommandType.StoredProcedure
                );

            if (rows.Count() < 0)
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Orders not found",
                    Data = null

                };
            }

            var first = rows.First();

            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetOrdersResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetOrdersResponse
                {
                    OrderID = x.OrderID,
                    OrderCode = x.OrderCode,
                    TotalAmounts = x.TotalAmounts,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    DeliveryAddress = x.DeliveryAddress,
                    ReceiverName = x.ReceiverName,
                    ReceiverPhone = x.ReceiverPhone,
                    IsActive = x.IsActive,
                    PaymentMethod = x.PaymentMethod

                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, options);
            _orderCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get orders success",
                Data = response
            };
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>> GetOrdersByUser(int pageNumber, int pageSize, string? Filter)
        {
            var userId = Guid.Parse(JWTUtil.GetUser()!);

            var parameters = new ListParameters<Order>(pageNumber, pageSize);
            parameters.AddFilter("Filter", Filter);
            parameters.AddFilter("UserId", userId);

            var cacheKey = _orderCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetOrdersResponse> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Get orders success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetOrdersByUser",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    Filter = Filter,
                    UserId = userId
                },
                commandType: System.Data.CommandType.StoredProcedure
                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Orders not found",
                    Data = null
                };
            }

            var first = rows.First();

            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetOrdersResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetOrdersResponse
                {
                    OrderID = x.OrderID,
                    OrderCode = x.OrderCode,
                    TotalAmounts = x.TotalAmounts,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    DeliveryAddress = x.DeliveryAddress,
                    ReceiverName = x.ReceiverName,
                    ReceiverPhone = x.ReceiverPhone,
                    IsActive = x.IsActive,
                    PaymentMethod = x.PaymentMethod

                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, options);
            _orderCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetOrdersResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get orders success",
                Data = response
            };

        }
    }
}
