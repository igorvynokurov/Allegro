using AllegroSearchService.Data.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroSearchService.Data.Entity
{
    public class TranslateItem : BaseEntityManualId<string>
    {
        public string TextRu { get; set; }
        public bool Translated { get; set; }
    }
}
