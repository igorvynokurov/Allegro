using System;

namespace KioskBrains.Clients.AllegroPl
{
    public class AllegroPlRequestException : Exception
    {
        public AllegroPlRequestException(string message)
            : base(message)
        {
        }

        public AllegroPlRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}