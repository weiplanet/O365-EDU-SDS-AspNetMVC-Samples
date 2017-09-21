using Microsoft.AspNetCore.Builder;
using OneRosterProviderDemo.Middlewares;

namespace OneRosterProviderDemo
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseOauthMessageSigning(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OAuth1>();
        }
    }
}
