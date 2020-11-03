using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.YandexTranslate.Models;
using Microsoft.Extensions.Options;
using AllegroSearchService.Common;
using Newtonsoft.Json;
using AllegroSearchService.Bl.ServiceInterfaces;
using AllegroSearchService.Data.Entity;
using Newtonsoft.Json.Linq;
using System.Text;

namespace KioskBrains.Clients.YandexTranslate
{
    public class YandexTranslateClient
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly YandexTranslateClientSettings _settings;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        private string _accessToken;
        private static readonly TimeSpan AccessTokenDuration = TimeSpan.FromHours(6);

        private readonly object _authRequestLocker = new object();

        private bool _isAuthRequestInProgress;
        private ITokenService _tokenService;
        private DateTime? _accessTokenTime;

        public YandexTranslateClient(IOptions<YandexTranslateClientSettings> settings, ITokenService tokenService)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));
            _tokenService = tokenService;
        }

        private bool IsAuthSessionExpired(DateTime now)
        {
            var accessTokenTime = _accessTokenTime;
            return accessTokenTime == null
                   || accessTokenTime.Value + AccessTokenDuration < now;
        }

        private async Task EnsureAuthSessionAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            if (_accessToken == null)
            {
                var t = await _tokenService.GetToken(TokenType.Yandex);
                _accessTokenTime = t == null ? null : t.ModifiedDate ?? null;
                _accessToken = t == null ? null : t.Token;
            }

            if (!IsAuthSessionExpired(now))
            {
                return;
            }

            lock (_authRequestLocker)
            {
                if (_isAuthRequestInProgress)
                {
                    if (_accessToken == null)
                    {
                        // possible for parallel requests after app started
                        throw new AllegroPlRequestException("Auth session is being initialized...");
                    }

                    // AccessTokenDuration is much lower than actual one, so old access token can be used even while new one is requested
                    return;
                }

                _isAuthRequestInProgress = true;
            }

            try
            {
                string responseBody;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
                        var httpResponse = await httpClient.PostAsync(
                            "https://iam.api.cloud.yandex.net/iam/v1/tokens",
                            new StringContent($"{{ \"yandexPassportOauthToken\":\"{ _settings.ApiKey }\" }}", Encoding.UTF8, "application/json"),
                                                    cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync();
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new AllegroPlRequestException($"Request to auth yandex API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AllegroPlRequestException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Request to auth API failed, no response.", ex);
                }

                string accessToken;
                try
                {
                    const string AccessTokenProperty = "iamToken";
                    var response = JsonConvert.DeserializeObject<JObject>(responseBody);
                    accessToken = response[AccessTokenProperty].Value<string>();
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        throw new Exception($"'{AccessTokenProperty}' is null or empty.");
                    }
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Bad format of auth API response.", ex);
                }
                await _tokenService.SetToken(accessToken, TokenType.Yandex, Guid.NewGuid());
                _accessToken = accessToken;
                _accessTokenTime = now;
            }
            finally
            {
                lock (_authRequestLocker)
                {
                    _isAuthRequestInProgress = false;
                }
            }
        }

        public async Task<string> TranslateAsync(
            string text,
            string fromLanguageCode,
            string toLanguageCode,
            CancellationToken cancellationToken)
        {                        
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // if digits only, return immediately
            if (text.All(x => char.IsDigit(x)
                              || char.IsWhiteSpace(x)
                              || char.IsSeparator(x)))
            {
                return text;
            }

            Assure.ArgumentNotNull(fromLanguageCode, nameof(fromLanguageCode));
            Assure.ArgumentNotNull(toLanguageCode, nameof(toLanguageCode));

            string responseBody;
            try
            {
                var apiKey = _settings.ApiKey;
                var requestUrl = $"https://translate.yandex.net/api/v1.5/tr.json/translate?key={apiKey}&lang={fromLanguageCode}-{toLanguageCode}";
                var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        ["text"] = text.Length > MaxTextLength
                            ? text.Substring(0, MaxTextLength - 3) + "..."
                            : text
                    });

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                    var httpResponse = await httpClient.PostAsync(
                        requestUrl,
                        httpContent,
                        cancellationToken);
                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new YandexTranslateRequestException($"Request to API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (YandexTranslateRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new YandexTranslateRequestException("Request to API failed, no response.", ex);
            }

            Response response;
            try
            {
                response = JsonConvert.DeserializeObject<Response>(responseBody);
                // todo: fix this in all clients - move out of try-catch
                if (response == null)
                {
                    throw new YandexTranslateRequestException("API response is null.");
                }
            }
            catch (Exception ex)
            {
                throw new YandexTranslateRequestException("Bad format of API response.", ex);
            }

            if (response.Text?.Length > 0)
            {
                return string.Join("\n", response.Text);
            }

            throw new YandexTranslateRequestException("API response doesn't contain any text.");
        }

        private const int MaxTextLength = 10_000;

        public async Task<string[]> TranslateAsync(
            string[] texts,
            string fromLanguageCode,
            string toLanguageCode,
            CancellationToken cancellationToken)
        {
            await EnsureAuthSessionAsync(cancellationToken);
            if (texts == null
                || texts.All(x => string.IsNullOrWhiteSpace(x)))
            {
                return texts;
            }

            Assure.ArgumentNotNull(fromLanguageCode, nameof(fromLanguageCode));
            Assure.ArgumentNotNull(toLanguageCode, nameof(toLanguageCode));



            string responseBody;
            try
            {
                var apiKey = _settings.ApiKey;
                var requestUrl = $"https://translate.api.cloud.yandex.net/translate/v2/translate";
                /*var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["sourceLanguageCode"] = Languages.PolishCode,
                    ["targetLanguageCode"] = Languages.RussianCode,                   
                    ["folderId"] = "b1gr992001hh7bimbr42",
                    ["format"] = "PLAIN_TEXT",
                    ["texts"] = JsonConvert.SerializeObject(texts)    
                });*/

                var str= $"{{ \"sourceLanguageCode\":\"{ fromLanguageCode.ToLower() }\", \"targetLanguageCode\": \"{toLanguageCode.ToLower()}\", \"format\": \"PLAIN_TEXT\", \"folderId\": \"b1gr992001hh7bimbr42\", \"texts\": {JsonConvert.SerializeObject(texts)} }}";
                var httpContent = new StringContent(str, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    //httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                    var httpResponse = await httpClient.PostAsync(
                        requestUrl,
                        httpContent,
                        cancellationToken);
                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new YandexTranslateRequestException($"Request to API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (YandexTranslateRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new YandexTranslateRequestException("Request to API failed, no response.", ex);
            }

            JObject response;
            string[] res;
            try
            {                
                response = JsonConvert.DeserializeObject<JObject>(responseBody);
                // todo: fix this in all clients - move out of try-catch
                if (response == null)
                {
                    throw new YandexTranslateRequestException("API response is null.");
                }

                res = response["translations"].ToList().Select(x => x["text"].ToString()).ToArray();
            }
            catch (Exception ex)
            {
                throw new YandexTranslateRequestException("Bad format of API response.", ex);
            }

            if (res.Any())
            {
                return res;
            }

            throw new YandexTranslateRequestException("API response doesn't contain any text.");

            //return translatedTexts;
        }
    }
}