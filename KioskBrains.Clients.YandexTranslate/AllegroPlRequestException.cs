using System;
using System.Runtime.Serialization;

namespace KioskBrains.Clients.YandexTranslate
{
    [Serializable]
    internal class AllegroPlRequestException : Exception
    {
        public AllegroPlRequestException()
        {
        }

        public AllegroPlRequestException(string message) : base(message)
        {
        }

        public AllegroPlRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AllegroPlRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}