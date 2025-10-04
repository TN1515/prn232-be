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
    public class ShopRegisterService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShopRegisterService> _logger;
        private Timer _timer;

        public ShopRegisterService(IServiceScopeFactory scopeFactory, ILogger<ShopRegisterService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ ShopRegisterService started.");
            // Mỗi 5 phút chạy 1 lần
            _timer = new Timer(async _ => await DoWorkAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async Task DoWorkAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DBContext>();

                DateTime cutoff = DateTime.Now.AddHours(-1);

                var shopUsers = await context.Users
                    .Where(u => u.Role == Domain.Entities.Enum.RoleEnum.Shop
                                && u.CreatedDate <= cutoff)
                    .ToListAsync();

                var usersToDelete = new List<User>();

                foreach (var user in shopUsers)
                {
                    bool hasShop = await context.Shops.AnyAsync(s => s.OwnerId == user.Id);
                    if (!hasShop)
                    {
                        usersToDelete.Add(user);
                        _logger.LogWarning($"🗑️ User {user.Email} (ID: {user.Id}) - created at {user.CreatedDate}, no shop info found.");
                    }
                }

                if (usersToDelete.Any())
                {
                    context.Users.RemoveRange(usersToDelete);
                    int deleted = await context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Deleted {deleted} invalid shop accounts at {DateTime.Now}.");
                }
                else
                {
                    _logger.LogInformation("No invalid shop accounts found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error occurred in ShopRegisterService.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 ShopRegisterService stopped.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
