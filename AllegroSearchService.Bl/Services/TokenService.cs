using AllegroSearchService.Bl.ServiceInterfaces;
using AllegroSearchService.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.Services
{
    public class TokenService : ITokenService
    {
        private IReadOnlyRepository _readOnlyRepository;
        private IWriteOnlyRepository _writeOnlyRepository;

        public TokenService(IReadOnlyRepository ro, IWriteOnlyRepository wo)
        {
            _readOnlyRepository = ro;
            _writeOnlyRepository = wo;
        }

        public async Task<TokenInfo> GetToken(TokenType tokenType)
        {
            var res = await _readOnlyRepository.Get<TokenInfo>(filter: x => x.TokenType == tokenType);
            return res.FirstOrDefault();
        }      

        public async Task SetToken(string token, TokenType tokenType, Guid by)
        {
            var coll = await _readOnlyRepository.Get<TokenInfo>(filter: x => x.TokenType == tokenType);
            var old = coll.FirstOrDefault();
            var item = new TokenInfo() { Token = token, TokenType = tokenType };
            if (old == null)
            {                
                _writeOnlyRepository.Add(item, by);
                _writeOnlyRepository.Commit();
            }
            else
            {
                item.ModifiedDate = DateTime.Now;
                item.Id = old.Id;
                _writeOnlyRepository.Update<TokenInfo>(item, by);
                await _writeOnlyRepository.Save();
            }
        }
    }
}
