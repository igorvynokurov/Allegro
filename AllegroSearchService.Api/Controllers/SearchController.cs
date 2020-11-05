using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using AllegroSearchService.Bl.ServiceInterfaces;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.YandexTranslate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllegroSearchService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        private AllegroPlClient _client;
        private CancellationTokenSource _tokenSource;
        public SearchController(ILogger<AllegroPlClient> logger, 
            IOptions<AllegroPlClientSettings> settings, 
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            ITokenService tokenService,
            ITranslateService translateService)
        {
            _logger = logger;
            _settings = settings;
            _yandexSettings = yandexApiClientSettings;
            _client = new AllegroPlClient(
                settings, 
                new YandexTranslateClient(yandexApiClientSettings, tokenService), 
                logger,
                tokenService,
                translateService);
        }
        

        // GET: api/<SearchController>
        [HttpGet]
        public  SearchOffersResponse Get(string phrase,
            string translatedPhrase,
            string categoryId,
            OfferStateEnum state,
            OfferSortingEnum sorting,
            int offset,
            int limit,
            string searchToken)
        {
            _tokenSource = new CancellationTokenSource();
            try
            {
                if (searchToken != "sfdfjiojaa5345zfhsery7JHG")
                {
                    throw new AuthenticationException("Invalid token");
                }               
                var token = _tokenSource.Token;
                var res = _client.SearchOffersAsync(phrase, translatedPhrase, categoryId, state, sorting, offset, limit, token).Result;
                return res;
            }
            catch(Exception er)
            {
                throw;
            }
            finally
            {
                _tokenSource.Dispose();
            }            
        }

        // GET api/<SearchController>/Cancel
        [HttpGet("Cancel")]
        public string Cancel()
        {
            return "value";
        }

        // POST api/<SearchController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SearchController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SearchController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
