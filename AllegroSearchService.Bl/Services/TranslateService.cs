using AllegroSearchService.Bl.ServiceInterfaces;
using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using AllegroSearchService.Common;
using AllegroSearchService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AllegroSearchService.Bl.Services
{
    public class TranslateService : ITranslateService
    {
        private IReadOnlyRepository _readOnlyRepository;
        private IWriteOnlyRepository _writeOnlyRepository;


        public TranslateService(IReadOnlyRepository ro, IWriteOnlyRepository wo)
        {
            _readOnlyRepository = ro;
            _writeOnlyRepository = wo;
        }

        public async Task AddRecord(string textInit, string textResult, string from, string to, Guid createdById)
        {
            textInit = textInit.ToLower();
            var old = await _readOnlyRepository.GetById<TranslateItem>(textInit);

            var item = new TranslateItem() { Id = textInit, TextRu = textResult, Translated = textInit.ToLower() != textResult.ToLower() };
            if (old == null)
            {
                _writeOnlyRepository.Add(item, createdById);
                _writeOnlyRepository.Commit();
            }
            else
            {
                if (textInit.ToLower() == textResult.ToLower() || old.TextRu.ToLower() == textResult.ToLower())
                {
                    return;
                }
                _writeOnlyRepository.Update<TranslateItem>(item, createdById);
                await _writeOnlyRepository.Save();
            }
        }

        public async Task AddRecords(IDictionary<string, string> texts, string fromCode, string toCode, Guid createdById)
        {
            foreach (var textInit in texts)
            {
                var text = textInit.Key.ToLower();
                var old = await _readOnlyRepository.GetById<TranslateItem>(textInit.Key);

                var item = new TranslateItem() { Id = text, TextRu = textInit.Value, Translated = text != textInit.Value.ToLower() };
                if (old == null)
                {
                    _writeOnlyRepository.Add(item, createdById);                    
                }
                
            }
            _writeOnlyRepository.Commit();
        }

        public IDictionary<string, string> GetDictionary(IEnumerable<string> values)
        {
            var texts = values.Select(x => x.ToLower()).ToList();
            var translations =  _readOnlyRepository.GetAllSync<TranslateItem>().Where(x => texts.Contains(x.Id)).ToList();
            return translations.ToDictionary(x => x.Id, x => x.TextRu);
        }

        public async Task<string> GetTranslatedText(string text, string from, string to)
        {
            var task = _readOnlyRepository.GetById<TranslateItem>(text);
            var res = await task;
            return res == null ? "" : res.TextRu;

            /*public  Task<string> GetTranslatedText(string text, Languages from, Languages to)
            {
                var task = _readOnlyRepository.GetById<TranslateItem>(text);

                return new Task<string>(() => task.Result.TextRu);
                var res = await task; 
                return res.TextRu;
            }*/
        }        
    }
}
