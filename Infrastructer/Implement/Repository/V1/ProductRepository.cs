using Application.Interface.IRepository.V1;
using Dapper;
using Domain.Context;
using Domain.Domain.Entities;
using Domain.Entities;
using Domain.Payload.Base;
using Domain.Payload.Base.Paginate;
using Domain.Payload.Request.Product;
using Domain.Payload.Response.Product;
using Domain.Payload.Response.ProductImage;
using Domain.Payload.Response.Shop;
using Domain.Share.Common;
using Domain.Share.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructer.Implement.Repository.V1
{
    public class ProductRepository(DBContext _context,
                                   GenericCacheInvalidator<Product> _productCache,
                                   GenericCacheInvalidator<Shop> _shopCache,
                                   GenericCacheInvalidator<FavoriteProduct> _favoriteProductCache,
                                   IMemoryCache _cache) : IProductRepository
    {
        public async Task<ApiResponse<string>> CreateNewProduct(CreateProductRequest request)
        {
            try
            {
                var newProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = HTMLUtil.Sanitize(request.Description),
                    CostPrice = request.CostPrice,
                    Price = request.Price,
                    Stock = request.Stock,
                    Material = request.Material,
                    CommonImage = request.CommonImage,
                    CreatedDate = DateTime.Now,
                    ModifyDate = null,
                    IsActive = true,
                    Status = "Available",
                    ShopID = _context.Shops
                                     .Where(x => x.OwnerId == Guid.Parse(JWTUtil.GetUser()))
                                     .Select(x => x.Id)
                                     .FirstOrDefault()

                };

                var checkCategory = await _context.Products
                                                  .Where(x => x.Category.Equals(request.Category))
                                                  .Select(x => x.Category)
                                                  .FirstOrDefaultAsync();
                if (checkCategory == null)
                {
                    newProduct.Category = request.Category;
                }
                else
                {
                    newProduct.Category = checkCategory;
                }

                await _context.Products.AddAsync(newProduct);

                if (request.MoreImage != null && request.MoreImage.Any())
                {
                    foreach (var image in request.MoreImage)
                    {
                        if (image == null || string.IsNullOrEmpty(image.Url))
                            continue;

                        var newImage = new ProductImages
                        {
                            ID = Guid.NewGuid(),
                            Url = image.Url,
                            CreatedDate = DateTime.Now,
                            ProductID = newProduct.Id
                        };

                        await _context.ProductImages.AddAsync(newImage);
                    }
                }

                await _context.SaveChangesAsync();
                _productCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.Created,
                    Message = "Create new product successfully",
                    Data = newProduct.Id.ToString()
                };
            }


            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> DeleteProduct(Guid productId)
        {
            try
            {
                var ownerId = Guid.Parse(JWTUtil.GetUser());
                var product = _context.Products
                                      .Include(p => p.Shop)
                                      .FirstOrDefault(p => p.Id == productId && p.IsActive == true);
                if (product == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Product not found Or has been Deleted",
                        Data = null
                    };
                }
                if (product.Shop.OwnerId != ownerId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You do not have permission to delete this product",
                        Data = null
                    };
                }

                product.IsActive = false;
                product.ModifyDate = DateTime.Now;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                _productCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Delete product successfully",
                    Data = product.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<GetDetail>> GetDetail(Guid productId)
        {
            var getProduct = await _context.Products
                                           .Include(x => x.ProductImages)
                                           .Include(x => x.Shop)
                                           .Include(x => x.ProductFeedbacks)
                                           .FirstOrDefaultAsync(x => x.Id == productId && x.IsActive == true);
            if (getProduct == null)
            {
                return new ApiResponse<GetDetail>
                {
                    StatusCode = StatusCode.NotFound,
                    Message = "Product not found",
                    Data = null
                };
            }

            var response = new GetDetail
            {
                Id = getProduct.Id,
                Name = getProduct.Name,
                Description = getProduct.Description,
                CostPrice = getProduct.CostPrice,
                Price = getProduct.Price,
                Stock = getProduct.Stock,
                Material = getProduct.Material,
                CommonImage = getProduct.CommonImage,
                CreatedDate = getProduct.CreatedDate,
                ModifyDate = getProduct.ModifyDate,
                Status = getProduct.Status,
                IsActive = getProduct.IsActive,
                Category = getProduct.Category,
                ShopID = getProduct.Shop.Id.ToString(),
                ShopName = getProduct.Shop.Name,
                Address = getProduct.Shop.Address,
                ShopPhone = getProduct.Shop.Phone,
                MoreImages = getProduct.ProductImages.Select(x => new ProductImagesResponse
                {
                    ID = x.ID,
                    Url = x.Url,
                    CreatedDate = x.CreatedDate
                }).OrderByDescending(x => x.CreatedDate).ToList(),
                Rating = getProduct.ProductFeedbacks.Any()
                            ? getProduct.ProductFeedbacks.Average(x => x.Rating)
                            : 0
            };

            return new ApiResponse<GetDetail>
            {
                StatusCode = StatusCode.OK,
                Message = "Get Product Success",
                Data = response
            };
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetProductsResponse>>> GetProducts(int pageNumber, int pageSize, string? search, string? filter, bool? isActive, Guid? ShopID)
        {

            //var paramters = new ListParameters<Product>(pageNumber, pageSize);
            //paramters.AddFilter("search", search);
            //paramters.AddFilter("filter", filter);
            //paramters.AddFilter("isActive", isActive);
            //paramters.AddFilter("ShopID", ShopID);
            //var cacheKey = _productCache.GetCacheKeyForList(paramters);

            //if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetProductsResponse> cachedResponse))
            //{
            //    return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
            //    {
            //        StatusCode = StatusCode.OK,
            //        Message = "Get products successfully (from cache)",
            //        Data = cachedResponse
            //    };
            //}

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetProducts",
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Search = search,
                    Filter = filter,
                    IsActive = isActive,
                    ShopID = ShopID
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (!rows.Any())
            {
                return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "No products found",
                    Data = null
                };
            }

            var first = rows.First();
            var totalRecords = (int)first.TotalRecords;

            var response = new ProcedurePagingResponse<GetProductsResponse>
            {
                TotalRecord = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = rows.Select(row => new GetProductsResponse
                {
                    Id = row.ID,
                    Name = row.Name,
                    Description = row.Description,
                    CostPrice = row.CostPrice,
                    Price = row.Price,
                    Stock = row.Stock,
                    Material = row.Material,
                    CommonImage = row.CommonImage,
                    CreatedDate = row.CreatedDate,
                    ModifyDate = row.ModifyDate,
                    IsActive = row.IsActive,
                    Status = row.Status,
                    Category = row.Category,
                    ShopID = row.ShopID,
                    ShopName = row.ShopName,
                    ShopAddress = row.ShopAddress,
                    ShopPhone = row.ShopPhone,
                    Rating = row.Rating
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            //_cache.Set(cacheKey, response, options);
            //_productCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get products successfully",
                Data = response
            };
        }

        public async Task<ApiResponse<string>> UpdateProduct(Guid productId, UpdateProductRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ownerId = Guid.Parse(JWTUtil.GetUser());
                var product = await _context.Products
                                            .Include(p => p.Shop)
                                            .Include(p => p.ProductImages)
                                            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive == true);

                if (product == null)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.NotFound,
                        Message = "Product not found Or has been Deleted",
                        Data = null
                    };
                }

                if (product.Shop.OwnerId != ownerId)
                {
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.Forbidden,
                        Message = "You do not have permission to update this product",
                        Data = null
                    };
                }

                // Update basic fields
                product.Name = request.Name ?? product.Name;
                product.Description = HTMLUtil.Sanitize(request.Description) ?? product.Description;
                product.CostPrice = request.CostPrice ?? product.CostPrice;
                product.Price = request.Price ?? product.Price;
                product.Stock = request.Stock ?? product.Stock;
                product.Material = request.Material ?? product.Material;
                product.CommonImage = request.CommonImage ?? product.CommonImage;
                product.ModifyDate = DateTime.Now;

                if (!string.IsNullOrEmpty(request.Category))
                {
                    var checkCategory = await _context.Products
                                                      .Where(x => x.Category.Equals(request.Category))
                                                      .Select(x => x.Category)
                                                      .FirstOrDefaultAsync();
                    product.Category = checkCategory ?? request.Category;
                }

                // --- Update MoreImages ---
                if (request.MoreImages != null && request.MoreImages.Any())
                {
                    // Xóa ảnh cũ
                    _context.ProductImages.RemoveRange(product.ProductImages);

                    // Thêm ảnh mới
                    foreach (var img in request.MoreImages)
                    {
                        var newImg = new ProductImages
                        {
                            ID = Guid.NewGuid(),
                            Url = img.Url,
                            CreatedDate = DateTime.Now,
                            ProductID = product.Id
                        };
                        await _context.ProductImages.AddAsync(newImg);
                    }
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _productCache.InvalidateEntity(product.Id);
                _shopCache.InvalidateEntity(product.ShopID.Value);

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Update product successfully",
                    Data = product.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.ToString());
            }
        }
        public async Task<ApiResponse<ProcedurePagingResponse<GetProductsResponse>>> GetProductsByShop(
    int pageNumber,
    int pageSize,
    string? search = null,
    string? filter = null,
    bool? isActive = null,
    Guid? ShopIDFromRequest = null)
        {
            Guid? shopID = ShopIDFromRequest;
            if (shopID == null)
            {
                var userId = JWTUtil.GetUser();
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
                    if (user != null && user.Role.ToString() == "Shop")
                    {
                        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == user.Id);
                        shopID = shop?.Id;
                    }
                }
            }

            if (shopID == null)
            {
                return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
                {
                    StatusCode = StatusCode.BadRequest,
                    Message = "ShopID không xác định",
                    Data = null
                };
            }

            //var parameters = new ListParameters<Product>(pageNumber, pageSize);
            //parameters.AddFilter("search", search);
            //parameters.AddFilter("filter", filter);
            //parameters.AddFilter("isActive", isActive);
            //parameters.AddFilter("ShopID", shopID);
            //var cacheKey = _productCache.GetCacheKeyForList(parameters);

            //if (_cache.TryGetValue(cacheKey, out ProcedurePagingResponse<GetProductsResponse> cachedResponse))
            //{
            //    return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
            //    {
            //        StatusCode = StatusCode.OK,
            //        Message = "Get products successfully (from cache)",
            //        Data = cachedResponse
            //    };
            //}

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetProducts",
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Search = search,
                    Filter = filter,
                    IsActive = isActive,
                    ShopID = shopID
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (!rows.Any())
            {
                return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "No products found",
                    Data = null
                };
            }

            var first = rows.First();
            var totalRecords = (int)first.TotalRecords;

            var response = new ProcedurePagingResponse<GetProductsResponse>
            {
                TotalRecord = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = rows.Select(row => new GetProductsResponse
                {
                    Id = row.ID,
                    Name = row.Name,
                    Description = row.Description,
                    CostPrice = row.CostPrice,
                    Price = row.Price,
                    Stock = row.Stock,
                    Material = row.Material,
                    CommonImage = row.CommonImage,
                    CreatedDate = row.CreatedDate,
                    ModifyDate = row.ModifyDate,
                    IsActive = row.IsActive,
                    Status = row.Status,
                    Category = row.Category,
                    ShopID = row.ShopID,
                    ShopName = row.ShopName,
                    ShopAddress = row.ShopAddress,
                    ShopPhone = row.ShopPhone
                }).ToList()
            };

            //var options = new MemoryCacheEntryOptions
            //{
            //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            //    SlidingExpiration = TimeSpan.FromMinutes(2)
            //};
            //_cache.Set(cacheKey, response, options);
            //_productCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetProductsResponse>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get products successfully",
                Data = response
            };
        }

        public async Task<ApiResponse<string>> FavoriteOrDisFavoriteProduct(FavoriteProductRequest request)
        {
            try
            {
                Guid userId = Guid.Parse(JWTUtil.GetUser()!);

                var getFavorite = await _context.FavoriteProducts.FirstOrDefaultAsync(x => x.ProductId == request.ProductID && x.UserId == userId);
                if (getFavorite != null)
                {
                    _context.FavoriteProducts.Remove(getFavorite);
                    await _context.SaveChangesAsync();


                    _productCache.InvalidateEntityList();
                    _favoriteProductCache.InvalidateEntityList();
                    _shopCache.InvalidateEntityList();
                    return new ApiResponse<string>
                    {
                        StatusCode = StatusCode.OK,
                        Message = "Remove Favorite Success",
                        Data = null
                    };
                }

                var newFavorite = new FavoriteProduct
                {
                    ProductId = request.ProductID,
                    UserId = userId
                };

                await _context.FavoriteProducts.AddAsync(newFavorite);
                await _context.SaveChangesAsync();

                _productCache.InvalidateEntityList();
                _favoriteProductCache.InvalidateEntityList();
                _shopCache.InvalidateEntityList();

                return new ApiResponse<string>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Favorite Product Success",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<ApiResponse<ProcedurePagingResponse<GetFavoriteProductResponse>>> GetFavoriteProduct(int pageNumber, int pageSize)
        {
            Guid userId = Guid.Parse(JWTUtil.GetUser()!);

            var parameters = new ListParameters<FavoriteProduct>(pageNumber, pageSize);
            parameters.AddFilter("User", userId);

            var cacheKey = _favoriteProductCache.GetCacheKeyForList(parameters);

            if (_cache.TryGetValue(cacheKey,out ProcedurePagingResponse<GetFavoriteProductResponse> cacheData))
            {
                return new ApiResponse<ProcedurePagingResponse<GetFavoriteProductResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Get favorite product success(cache)",
                    Data = cacheData
                };
            }

            using var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var rows = await connection.QueryAsync<dynamic>(
                "dbo.sp_GetFavoriteProduct",
                new
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    userId = userId
                },

                commandType: System.Data.CommandType.StoredProcedure

                );

            if (rows.Count() < 1)
            {
                return new ApiResponse<ProcedurePagingResponse<GetFavoriteProductResponse>>
                {
                    StatusCode = StatusCode.OK,
                    Message = "Not Found",
                    Data = null
                };
            }

            var first = rows.First();
            int totalRecords = first.TotalRecords;

            var response = new ProcedurePagingResponse<GetFavoriteProductResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecord = totalRecords,
                Items = rows.Select(x => new GetFavoriteProductResponse
                {
                    ProductID = x.ProductID,
                    ProductName = x.ProductName,
                    Description = x.Description,
                    Image = x.Image,
                    Price = x.Price,
                    ShopID = x.ShopID,
                    ShopName = x.ShopName,
                    ShopAddress = x.ShopAddress,
                }).ToList()
            };

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey,response, options);
            _favoriteProductCache.AddToListCacheKeys(cacheKey);

            return new ApiResponse<ProcedurePagingResponse<GetFavoriteProductResponse>>
            {
                StatusCode = StatusCode.OK,
                Message = "Get Favorite Product Success",
                Data = response
            };
        }
    }
}
