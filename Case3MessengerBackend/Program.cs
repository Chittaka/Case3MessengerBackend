using Case3MessengerBackend;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Case3MessengerBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
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

            builder.Services.Configure<IpRateLimitOptions>(options =>
            {
                        options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit = 1000,
                    Period = "1m"
                }
            };
            });
            builder.Services.AddSwaggerGen();

            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddControllers();
            var app = builder.Build();
            app.UseIpRateLimiting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerUI();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();  // Подключаем все контроллеры
                endpoints.MapHub<ChatHub>("/chat").RequireAuthorization();
            });

            app.UseHttpsRedirection();
            app.UseStatusCodePages();
            app.UseExceptionHandler("/Error");
            app.UseHsts(); // Включает HSTS для браузеров
            app.Run();
        }
    }
}
