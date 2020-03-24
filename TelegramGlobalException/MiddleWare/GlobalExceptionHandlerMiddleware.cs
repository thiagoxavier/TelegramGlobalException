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
        private static string _addressExtraInformation;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, IOptions<AddConfig> options)
        {
            _addressExtraInformation = options.Value.AddressExtraInformation;
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
            var telegramMessage = new StringBuilder();
            telegramMessage.AppendLine($"*Status code:* {statusCode}");
            telegramMessage.AppendLine($"*TraceId:* {context.TraceIdentifier}");
            telegramMessage.AppendLine($"*Action:* {context.Request.Path}");
            telegramMessage.AppendLine($"*Exception:* {exception.Message.ToString()}");

            if (!string.IsNullOrWhiteSpace(_addressExtraInformation))
            {
                telegramMessage.AppendLine($"*Extra information:* {_addressExtraInformation}{context.TraceIdentifier}");
            }

            telegramMessage.AppendLine($"*Date:* {DateTime.Now.ToString()}");
            string stackTrace = exception.StackTrace.Length > 2100 ? exception.StackTrace.Substring(0, 2100) : exception.StackTrace;

            telegramMessage.AppendLine($"*Stack Trace:* {stackTrace}");

            return telegramMessage.ToString();
        }

        public void SendErrorToTelegram(HttpContext context, Exception exception, int statusCode)
        {
            var mensagem = Message(context, exception, statusCode);
            _ = _notificationService.Notify("", mensagem);
        }
    }
}
