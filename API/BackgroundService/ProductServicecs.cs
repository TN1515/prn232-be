using Domain.Context;
using Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API.BackgroundService
{
    public class ProductService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductService> _logger;
        private Timer _timer;

        public ProductService(IServiceScopeFactory scopeFactory, ILogger<ProductService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ ProductService started.");

            // 🔁 Chạy ngay lập tức, sau đó lặp lại mỗi 5 phút (có thể chỉnh lại tùy ý)
            _timer = new Timer(async _ => await DoWorkAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DBContext>();
                var cacheInvalidator = scope.ServiceProvider.GetRequiredService<GenericCacheInvalidator<Product>>();
                var products = await context.Products.ToListAsync();

                if (products.Count == 0)
                {
                    _logger.LogInformation("📦 No products found to update.");
                    return;
                }

                int updatedCount = 0;

                foreach (var product in products)
                {
                    if (product.Stock == 0 && product.Status != "Out Of Stock")
                    {
                        product.Status = "Out Of Stock";
                        updatedCount++;
                    }
                    else if (product.Stock > 0 && product.Status == "Out Of Stock")
                    {
                        product.Status = "Available";
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"🛒 Updated {updatedCount} product statuses at {DateTime.Now}.");
                }
                else
                {
                    _logger.LogInformation("✅ No product status changes needed.");
                }

                cacheInvalidator.InvalidateEntityList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error occurred in ProductService.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 ProductService stopped.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
