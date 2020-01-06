namespace TelegramGlobalException.Extensions
{
    public class AddConfig
    {
        public string BotId { get; set; }
        public string ReceiveId { get; set; }

        /// <summary>
        /// Report message
        /// </summary>
        public bool ReportMessageError { get; set; } = true;
    }
}
