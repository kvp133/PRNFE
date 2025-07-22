
using Microsoft.AspNetCore.Http;
using PRNFE.MVC.Middleware;
﻿using Microsoft.AspNetCore.Http;


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

            // Cấu hình HttpClient với BaseUrl từ appsettings.json
            builder.Services.AddHttpClient<Controllers.ResidentController>(client =>
            {
                var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.Timeout = TimeSpan.FromSeconds(30); // Set timeout
                }
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
