using System;

namespace KioskBrains.Clients.YandexTranslate
{
    public class YandexTranslateRequestException : Exception
    {
        public YandexTranslateRequestException(string message)
            : base(message)
        {
        }

        public YandexTranslateRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}