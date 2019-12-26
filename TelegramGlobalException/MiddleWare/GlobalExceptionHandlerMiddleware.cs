using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using TelegramGlobalException.Extensions;

namespace TelegramGlobalException.MiddleWare
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private static NotificationService _notificationService;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, IOptions<AddConfig> options)
        {
            _logger = logger;
            _notificationService = new NotificationService(options.Value.BotId, options.Value.ReceiveId);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            const int statusCode = StatusCodes.Status500InternalServerError;

            StringBuilder telegramMessage = new StringBuilder();
            telegramMessage.AppendLine($"%0A*Status code:* {statusCode}");
            telegramMessage.AppendLine($"%0A*TraceId:* {context.TraceIdentifier}");
            telegramMessage.AppendLine($"%0A*Action:* {context.Request.Path}");
            telegramMessage.AppendLine($"%0A*Exception:* {exception.Message.ToString()}");
            telegramMessage.AppendLine($"%0A*Date:* {DateTime.Now.ToString()}");


            _ = _notificationService.Notify("", telegramMessage.ToString());

            var json = JsonConvert.SerializeObject(new
            {
                statusCode,
                message = "An error occurred whilst processing your request",
                detailed = exception.Message
            }, SerializerSettings.JsonSerializerSettings);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";


            return context.Response.WriteAsync(json);
        }


    }
}
