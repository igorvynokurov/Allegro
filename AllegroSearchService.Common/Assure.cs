using System;

namespace AllegroSearchService.Common
{
    public class Assure
    {
        public static void ArgumentNotNull(object argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ApplicationException($"{ argName } cannot be null.");
            }
        }
    }
}
