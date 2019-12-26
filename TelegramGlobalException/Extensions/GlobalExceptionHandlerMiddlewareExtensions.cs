using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using TelegramGlobalException.MiddleWare;

namespace TelegramGlobalException.Extensions
{
    public static class TelegramExceptionHandlerMiddlewareExtensions
    {

        public static IServiceCollection AddGlobalExceptionHandlerMiddleware(this IServiceCollection services, Action<AddConfig> configuration)
        {
            services.Configure(configuration);
            return services.AddTransient<GlobalExceptionHandlerMiddleware>();
        }

        public static void UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
