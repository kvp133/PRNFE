using Microsoft.AspNetCore.Http;
using PRNFE.MVC.Helper;
using PRNFE.MVC.Middleware;

namespace PRNFE.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddTransient<LoggingHandler>();
            builder.Services.AddHttpClient("AuthorizedApiClient")
                .AddHttpMessageHandler<AuthHeaderHandler>()
                .AddHttpMessageHandler<LoggingHandler>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Cho phép HTTP (qua gateway)
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            // Add custom authorization middleware
            app.UseMiddleware<AuthorizationMiddleware>();

            app.UseAuthorization();

            app.MapRazorPages();
            
            // Add specific routes for different controllers
            app.MapControllerRoute(
                name: "admin",
                pattern: "Admin/{action=UserManagement}/{id?}",
                defaults: new { controller = "Admin" });

            app.MapControllerRoute(
                name: "landlord",
                pattern: "Landlord/{action=DormitoryManagement}/{id?}",
                defaults: new { controller = "Landlord" });

            app.MapControllerRoute(
                name: "tenant",
                pattern: "Tenant/{action=InvoiceInfo}/{id?}",
                defaults: new { controller = "Tenant" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}