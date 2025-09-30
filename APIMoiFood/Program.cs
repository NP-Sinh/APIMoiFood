using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using APIMoiFood.Services.AuthService;
using APIMoiFood.Services.CartService;
using APIMoiFood.Services.CategoryService;
using APIMoiFood.Services.EmailService;
using APIMoiFood.Services.FoodService;
using APIMoiFood.Services.JwtService;
using APIMoiFood.Services.NotificationService;
using APIMoiFood.Services.OrderService;
using APIMoiFood.Services.PaymentMethodService;
using APIMoiFood.Services.PaymentService;
using APIMoiFood.Services.PaymentService.MomoService;
using APIMoiFood.Services.PaymentService.VnpayService;
using APIMoiFood.Services.ProfileService;
using APIMoiFood.Services.ReviewService;
using APIMoiFood.Services.Statistics.StatisticsService;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MoiFoodDBContext>(c =>
        c.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("Momo"));
builder.Services.Configure<VnpaySettings>(builder.Configuration.GetSection("VnPay"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5046); // HTTP
    options.ListenAnyIP(7128, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// JWT config
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// RateLimit config
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


// Add services to the container.
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<MomoService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IVnpayService, VnpayService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Cho phép truy cập file tĩnh (wwwroot/images/...)
app.UseStaticFiles();
app.MapHub<NotificationHub>("/hubs/notification");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting();

app.MapControllers();

app.Run();
