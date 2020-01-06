using Microsoft.AspNetCore.Http;
using System;

namespace TelegramGlobalException.MiddleWare
{
    public interface IGlobalExceptionHandlerMiddleware
    {
         void SendErrorToTelegram(HttpContext context, Exception exception, int statusCode);
    }
}
