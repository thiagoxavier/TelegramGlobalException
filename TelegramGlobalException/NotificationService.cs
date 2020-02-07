using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TelegramGlobalException
{
    public class NotificationService
    {
        private readonly string _botId;
        private readonly string _botReceiveId;
        public NotificationService(string botId, string botReceiveId)
        {
            _botId = botId;
            _botReceiveId = botReceiveId;
        }

        public async Task Notify(string title, string message)
        {
            var text = new StringBuilder($"*{title}*");
            text.Append($"%0A{Uri.EscapeDataString(message)}");
            text.Append($"%0A*MachineName: {Environment.MachineName}*");
            text.Append($"%0A*Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}*");

            using (var httpClient = new HttpClient())
            {
                var url = $"https://api.telegram.org/{_botId}/sendMessage?chat_id={_botReceiveId}&parse_mode=markdown&text={text}";
                var response = await httpClient.GetAsync(url);
            }
        }
    }
}
