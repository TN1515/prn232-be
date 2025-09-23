using System.Data;
using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.CartItem;
using Domain.Payload.Response.Cart;
using Domain.Share.Common;
using Domain.Share.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class CartRepository(DBContext _context,
                                GenericCacheInvalidator<Cart> _cartCache,
                                GenericCacheInvalidator<CartItem> _cartItemCache,
                                IMemoryCache _cache) : ICartRepository
    {
        public async Task<ApiResponse<string>> AddCartItem(AddCartItemRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = Guid.Parse(JWTUtil.GetUser()!);
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Cart not found",
                        Data = null
                    };
                }

                var checkExist = await _context.CartItems
                                               .Include(x => x.Product)
                                               .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);

                if (checkExist != null)
                {
                    // Cộng dồn số lượng
                    checkExist.Quantity += request.Quantity;
                    checkExist.UnitPrice = checkExist.Product.Price;
                    checkExist.TotalAmounts = checkExist.Quantity * checkExist.UnitPrice;

                    _context.CartItems.Update(checkExist);
                    await _context.SaveChangesAsync();

                    // Cập nhật tổng tiền giỏ hàng
                    cart.TotalAmounts = await _context.CartItems
                                                .Where(ci => ci.CartId == cart.Id)
                                                .SumAsync(ci => ci.TotalAmounts);

                    _context.Carts.Update(cart);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    _cartItemCache.InvalidateEntityList();
                    _cartCache.InvalidateEntityList();

                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.OK,
                        Message = "Update cart item successfully",
                        Data = null
                    };
                }

                // Thêm sản phẩm mới vào giỏ
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Product not found",
                        Data = null
                    };
                }

                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price,
                    TotalAmounts = request.Quantity * product.Price,
                    Note = HTMLUtil.Sanitize(request.Note)
                };

                await _context.CartItems.AddAsync(cartItem);
                await _context.SaveChangesAsync();

                cart.TotalAmounts = await _context.CartItems
                                            .Where(ci => ci.CartId == cart.Id)
                                            .SumAsync(ci => ci.TotalAmounts);

                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _cartItemCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Add cart item successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<ApiResponse<string>> DeleteCartItem(Guid cartItemId)
        {
            try
            {
                var checkDelete = await _context.CartItems
                                                .Include(x => x.Cart)
                                                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
                if (checkDelete == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Cart item not found",
                        Data = null
                    };
                }

                if (checkDelete != null && checkDelete.Cart.UserId != Guid.Parse(JWTUtil.GetUser()!))
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You are not allowed to delete this cart item",
                        Data = null
                    };
                }

                _context.CartItems.Remove(checkDelete);
                await _context.SaveChangesAsync();
                checkDelete.Cart.TotalAmounts = _context.CartItems
                                                        .Where(ci => ci.CartId == checkDelete.CartId)
                                                        .Sum(ci => ci.TotalAmounts);
                _context.Carts.Update(checkDelete.Cart);
                await _context.SaveChangesAsync();

                _cartItemCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Delete cart item successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> EditCartItemQuantity(EditCartItemQuantityRequest request)
        {
            try
            {
                var userId = Guid.Parse(JWTUtil.GetUser()!);
                var cartItem = await _context.CartItems
                                       .Include(ci => ci.Cart)
                                       .FirstOrDefaultAsync(ci => ci.Id == request.CartItemID);
                if (cartItem == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Cart item not found",
                        Data = null
                    };
                }

                if (cartItem != null && cartItem.Cart.UserId != userId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You are not allowed to edit this cart item",
                        Data = null
                    };
                }

                cartItem.Quantity = request.Quantity;
                cartItem.TotalAmounts = cartItem.Quantity * cartItem.UnitPrice;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
                cartItem.Cart.TotalAmounts = _context.CartItems
                                                    .Where(ci => ci.CartId == cartItem.CartId)
                                                    .Sum(ci => ci.TotalAmounts);
                _context.Carts.Update(cartItem.Cart);
                await _context.SaveChangesAsync();
                _cartItemCache.InvalidateEntityList();
                _cartCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Edit cart item quantity successfully",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetCarts>>> GetCartsItem(int pageNumber, int pageSize)
        {
            var parameters = new ListParameters<CartItem>(pageNumber, pageSize);
            parameters.AddFilter("UserId", Guid.Parse(JWTUtil.GetUser()!));
            var cacheKey = _cartItemCache.GetCacheKeyForList(parameters);

            // Check cache
            if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetCarts> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetCarts>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get cart items successfully",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var userId = Guid.Parse(JWTUtil.GetUser()!);
            var cartId = await _context.Carts
                                       .Where(c => c.UserId == userId)
                                       .Select(x => x.Id)
                                       .FirstOrDefaultAsync();

            if (cartId == Guid.Empty)
            {
                return new ApiResponse<ProcedurePagingResponse<GetCarts>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "No cart found",
                    Data = null
                };
            }

            using var multi = await connection.QueryMultipleAsync(
                "dbo.sp_GetCartItems",
                new
                {
                    CartID = cartId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure);

            // 1. Lấy tổng tiền giỏ hàng
            var totalAmounts = await multi.ReadFirstOrDefaultAsync<decimal>();

            // 2. Lấy danh sách cart items + TotalRecords
            var cartItemRows = (await multi.ReadAsync<dynamic>()).ToList();

            if (!cartItemRows.Any())
            {
                return new ApiResponse<ProcedurePagingResponse<GetCarts>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "No cart items found",
                    Data = null
                };
            }

            int totalRecords = cartItemRows.First().TotalRecords;

            var cartItems = cartItemRows.Select(row => new GetCartItemsResponse
            {
                CartItemID = row.CartItemID,
                ProductID = row.ProductID,
                ProductName = row.ProductName,
                ProductImage = row.Image,
                UnitPrice = row.UnitPrice,
                Quantity = row.Quantity,
                TotalAmounts = row.TotalAmounts
            }).ToList();

            var response = new ProcedurePagingResponse<GetCarts>
            {
                TotalRecord = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = new List<GetCarts>
                {
                    new GetCarts
                    {
                        TotalAmounts = totalAmounts,
                        CartItems = cartItems
                    }
                }
            };

            // Save cache
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            _cache.Set(cacheKey, response, cacheEntryOptions);
            _cartItemCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetCarts>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get cart items successfully",
                Data = response
            };
        }
    }
}
