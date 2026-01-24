using System.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection;
using Web.MVC.Constants;

namespace Web.MVC.Services.DelegatingHandlers
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly IDataProtector dataProtector;
        private readonly IHttpContextAccessor contextAccessor;
        public AuthHandler(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor)
        {
            dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposeConstants.Jwt);
            this.contextAccessor = contextAccessor;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = contextAccessor.HttpContext;

            if (httpContext?.Request.Cookies.TryGetValue(CookieNames.AccessToken, out string? protectedAccessToken) == true)
            {
                if (!string.IsNullOrEmpty(protectedAccessToken))
                {
                    string accessToken = dataProtector.Unprotect(protectedAccessToken);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
