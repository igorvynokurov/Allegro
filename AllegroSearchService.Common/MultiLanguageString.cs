using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroSearchService.Common
{
    public class MultiLanguageString : Dictionary<string, string>
    {
        public string GetValue(string key)
        {
            return this[key];
        }
    }
}
