using AllegroSearchService.Data.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllegroSearchService.Data.Entity
{
    public class TokenInfo : BaseEntity<int>
    {        
        public string Token { get; set; }
        public TokenType TokenType { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public enum TokenType
    {
        Allegro, Yandex
    }
}
