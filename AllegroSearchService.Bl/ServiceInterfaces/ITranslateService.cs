using AllegroSearchService.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.ServiceInterfaces
{
    public interface ITranslateService
    {
        Task<string> GetTranslatedText(string text, string from, string to);
        Task AddRecord(string textInit, string textResult, string fromCode, string toCode, Guid createdById);
        Task AddRecords(IDictionary<string, string> texts,  string fromCode, string toCode, Guid createdById);
        IDictionary<string, string> GetDictionary(IEnumerable<string> values);
    }
}
