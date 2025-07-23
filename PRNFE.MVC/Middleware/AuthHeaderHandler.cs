using System.Net.Http.Headers;

namespace PRNFE.MVC.Middleware
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            // ✅ Add Bearer Token
            var token = context.Request.Cookies["AccessToken"]; // Or read from session/cookie/claims
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // ✅ Forward Cookies from browser request (optional)
            if (context.Request.Headers.TryGetValue("Cookie", out var cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader.ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
