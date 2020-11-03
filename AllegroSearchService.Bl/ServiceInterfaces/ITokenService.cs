using AllegroSearchService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.ServiceInterfaces
{
    public interface ITokenService
    {
        Task<TokenInfo> GetToken(TokenType tokenType);
        Task SetToken(string token, TokenType tokenType, Guid by);
    }
}
