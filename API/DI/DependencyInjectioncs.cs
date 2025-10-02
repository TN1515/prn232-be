using API.BackgroundService;
using Application.Interface.IRepository.V1;
using Domain.Context;
using EmailService.Config;
using EmailService.Implement;
using EmailService.Interface;
using Infrastructer.Implement.Repository.V1;
using Infrastructure.Implement.Repository.V1;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQContract.Config;
using RabbitMQContract.Consumer;
using RabbitMQContract.Consumer.Email;
using SePaySerivce.Service;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Discord;

namespace API.DI
{   
    public class DependencyInjections
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            #region Database Configuration
            services.AddDbContext<DBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            #endregion

            #region Repository Configuration
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IShopRepository, ShopRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IBankSettingRepository, BankSettingsRepository>();
            services.AddScoped<ISepayQRService, SePayService>();
            services.AddScoped<IProductFeedbacksRepository, ProcuctFeedbackRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
            services.AddScoped<ISliderRepository, SliderRepository>();
            #endregion

            #region Common Configuration

            #endregion
            #region Cache Configuration

            services.AddMemoryCache();
            services.AddScoped(typeof(GenericCacheInvalidator<>));

            #endregion

            #region Serilog Config
            services.AddSerilog();

            var webHookId = configuration["Discord:WebHookId"];
            var webHookToken = configuration["Discord:WebHookToken"];

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console();

            if (ulong.TryParse(webHookId, out var parsedWebHookId) && !string.IsNullOrWhiteSpace(webHookToken))
            {
                loggerConfig = loggerConfig.WriteTo.Discord(
                    webhookId: parsedWebHookId,
                    webhookToken: webHookToken,
                    restrictedToMinimumLevel: LogEventLevel.Error);
            }

            Log.Logger = loggerConfig.CreateLogger();

            #endregion

            #region RabbitMQ
            services.Configure<RabbitMQConfig>(configuration.GetSection("RabbitMQ"));
            var rabbitSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<TestMessageConsumer>();
                x.AddConsumer<EmailConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitSettings.HostName, rabbitSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitSettings.UserName);
                        h.Password(rabbitSettings.Password);
                    });

                    cfg.ReceiveEndpoint("test-queue", e =>
                    {
                        e.ConfigureConsumer<TestMessageConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("email-queue", e =>
                    {
                        e.ConfigureConsumer<EmailConsumer>(context);
                    });

                });
            });


            #endregion

            #region Email Settings
            services.Configure<SendMailConfig>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailSender, EmailSender>();

            #endregion

            #region Background Serivce
            services.AddHostedService<ShopRegisterService>();
            services.AddHostedService<ProductService>();
            #endregion

        }
    }
}
