namespace KioskBrains.Clients.YandexTranslate.Models
{
    internal class Response
    {
        public int Code { get; set; }

        public string Lang { get; set; }

        public string[] Text { get; set; }
    }
}