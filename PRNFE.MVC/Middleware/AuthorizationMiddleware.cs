using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PRNFE.MVC.Models;
using System.Text;

namespace PRNFE.MVC.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Cookies["AccessToken"];

			var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

			if (path.StartsWith("/tenant/managepayment") )
			{
				await _next(context);
				return;
			}



			if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenModel = ParseJwtToken(token);
                    if (tokenModel != null)
                    {
                        // Store user info in HttpContext for use in controllers
                        context.Items["UserInfo"] = tokenModel;
                        
                        // Check if token is expired
                        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        if (currentTime > tokenModel.exp)
                        {
                            // Token expired, clear cookies and redirect to login
                            context.Response.Cookies.Delete("AccessToken");
                            context.Response.Cookies.Delete("RefreshToken");
                            context.Response.Redirect("/Auth/Login");
                            return;
                        }
                    }
                }
                catch
                {
                    // Invalid token, clear cookies and redirect to login
                    context.Response.Cookies.Delete("AccessToken");
                    context.Response.Cookies.Delete("RefreshToken");
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }

            await _next(context);
        }

        private JwtTokenModel ParseJwtToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;
                
                var payload = parts[1];
                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                var json = Encoding.UTF8.GetString(bytes);
                
                return JsonConvert.DeserializeObject<JwtTokenModel>(json);
            }
            catch
            {
                return null;
            }
        }
    }
} 