using Microsoft.AspNetCore.DataProtection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Web.MVC.Constants;
using Web.MVC.Models.Api_responses.account;

namespace Web.MVC.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IDataProtector dataProtector;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string url;

        public AuthMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider, IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            this.next = next;
            this.httpClientFactory = httpClientFactory;
            dataProtector = dataProtectionProvider.CreateProtector("JWT");
            url = $"{configuration["ApiGateway:Protocol"]}://{configuration["ApiGateway:Domain"]}";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out string? protectedAccessToken);

            if (protectedAccessToken != null)
            {
                string accessToken = dataProtector.Unprotect(protectedAccessToken);
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);

                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    context.Response.Cookies.Delete(CookieNames.AccessToken);

                    string userIdStr = jwt.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    Guid userId = new Guid(userIdStr);

                    HttpClient httpClient = httpClientFactory.CreateClient();

                    var userResponse = await httpClient.GetAsync($"{url}/api/User/get-user-by-id/{userId}");
                    userResponse.EnsureSuccessStatusCode();
                    var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                    if (user.RefreshToken != null && user.RefreshTokenExpiryTime > DateTime.UtcNow)
                    {
                        using StringContent jsonContent = new(JsonSerializer.Serialize(new
                        {
                            AccessToken = accessToken,
                            user.RefreshToken
                        }), Encoding.UTF8, "application/json");
                        var refreshResponse = await httpClient.PostAsync($"{url}/api/Token/refresh", jsonContent);
                        refreshResponse.EnsureSuccessStatusCode();

                        string newAccessToken = await refreshResponse.Content.ReadAsStringAsync();

                        string protectedNewAccessToken = dataProtector.Protect(newAccessToken);
                        context.Response.Cookies.Append(CookieNames.AccessToken, protectedNewAccessToken, new CookieOptions
                            { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });

                        context.Request.Headers.Append("Authorization", "Bearer " + newAccessToken);
                    }
                }
                else
                {
                    context.Request.Headers.Append("Authorization", "Bearer " + accessToken);
                }
            }

            await next.Invoke(context);
        }
    }
}
