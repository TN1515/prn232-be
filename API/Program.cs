using System.Text.Json.Serialization;
using API.DI;
using Domain.Config;
using Domain.Payload.Base;
using Domain.Share.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
#pragma warning disable

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
                .AddJsonOptions(optins =>
                {
                    optins.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.UseAllOfToExtendReferenceSchemas();
});
builder.Services.AddHttpContextAccessor();
var httpContextAccessor = builder.Services.BuildServiceProvider()
    .GetRequiredService<IHttpContextAccessor>();

#region CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
#endregion

DependencyInjections.Register(builder.Services, builder.Configuration);
#region JWT & SWAGGER Configuration

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer {token}' vào đây"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();
JWTUtil.Configure(jwtSettings, httpContextAccessor);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"error\":\"Token không hợp lệ hoặc đã hết hạn.\"}");
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiResponse<string>
            {
                StatusCode = StatusCode.Unauthorized,
                Message = "Bạn chưa đăng nhập hoặc token không hợp lệ.",
                Data = null
            }));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var response = new ApiResponse<string>
            {
                StatusCode = StatusCode.Forbidden,
                Message = "Bạn không có quyền truy cập vào tài nguyên này.",
                Data = null
            };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    };
});

#endregion
var app = builder.Build();

app.MapGet("Get-LTT BOT", () =>
{
    app.Logger.LogError("Tôi Lê Thanh Truyền Đây Thưa Ngài!");
    return Results.Ok("Get-LTT BOT is running");
}
);

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
