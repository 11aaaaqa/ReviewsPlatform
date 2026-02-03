using System.Net.Http.Headers;
using Web.MVC.Constants;

namespace Web.MVC.Services.DelegatingHandlers
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor contextAccessor;
        public AuthHandler(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = contextAccessor.HttpContext;

            if (httpContext?.Items.TryGetValue(CookieNames.AccessToken, out var accessTokenObj) == true)
            {
                if (accessTokenObj != null)
                {
                    string accessToken = accessTokenObj.ToString()!;
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
