using EventTicket.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VNPAY.NET;

namespace EventTicket.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            builder.Services.AddDbContext<EventTicketContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<EventTicket.Service.Service.EventService>(); // Đăng ký EventService trước khi Build

            builder.Services.AddSingleton<IVnpay, Vnpay>(); // Đăng ký IVnpay thủ công

            // Add IHttpClientFactory with named client
            builder.Services.AddHttpClient("VnpayClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7134"); // Khớp với cổng trong launchSettings.json
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    // Bỏ qua kiểm tra chứng chỉ (chỉ dùng trong môi trường phát triển)
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllers();
            app.Run();
        }
    }
}