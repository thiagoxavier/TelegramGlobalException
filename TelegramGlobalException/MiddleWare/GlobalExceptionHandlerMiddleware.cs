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
    public class GlobalExceptionHandlerMiddleware : IMiddleware, IGlobalExceptionHandlerMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private static NotificationService _notificationService;
        private static bool reportMessage;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, IOptions<AddConfig> options)
        {
            _logger = logger;
            _notificationService = new NotificationService(options.Value.BotId, options.Value.ReceiveId);
            reportMessage = options.Value.ReportMessageError;
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

            var mensagem = Message(context, exception, statusCode);
            _ = _notificationService.Notify("", mensagem);

            string messageError = reportMessage ? exception.Message : "";
            var json = JsonConvert.SerializeObject(new
            {
                statusCode,
                message = "An error occurred whilst processing your request",
                detailed = messageError
            }, SerializerSettings.JsonSerializerSettings);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";


            return context.Response.WriteAsync(json);
        }

        protected static string Message(HttpContext context, Exception exception, int statusCode)
        {
            StringBuilder telegramMessage = new StringBuilder();
            telegramMessage.AppendLine($"%0A*Status code:* {statusCode}");
            telegramMessage.AppendLine($"%0A*TraceId:* {context.TraceIdentifier}");
            telegramMessage.AppendLine($"%0A*Action:* {context.Request.Path}");
            telegramMessage.AppendLine($"%0A*Exception:* {exception.Message.ToString()}");
            telegramMessage.AppendLine($"%0A*Date:* {DateTime.Now.ToString()}");
            return telegramMessage.ToString();
        }

        public void SendErrorToTelegram(HttpContext context, Exception exception, int statusCode)
        {
            var mensagem = Message(context, exception, statusCode);
            _ = _notificationService.Notify("", mensagem);
        }
    }
}
