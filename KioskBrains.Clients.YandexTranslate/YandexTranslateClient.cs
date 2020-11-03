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

namespace KioskBrains.Clients.YandexTranslate
{
    public class YandexTranslateClient
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly YandexTranslateClientSettings _settings;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public YandexTranslateClient(IOptions<YandexTranslateClientSettings> settings)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));
        }

        public async Task<string> TranslateAsync(
            string text,
            string fromLanguageCode,
            string toLanguageCode,
            CancellationToken cancellationToken)
        {
            return "RUS";
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
            if (texts == null
                || texts.All(x => string.IsNullOrWhiteSpace(x)))
            {
                return texts;
            }

            Assure.ArgumentNotNull(fromLanguageCode, nameof(fromLanguageCode));
            Assure.ArgumentNotNull(toLanguageCode, nameof(toLanguageCode));

            // creates tons of threads but 2-3 times faster
            var tasks = texts.Select(
                text => Task.Run(
                    () => TranslateAsync(text, fromLanguageCode, toLanguageCode, cancellationToken),
                    cancellationToken));

            var translatedTexts = await Task.WhenAll(tasks);

            return translatedTexts;
        }
    }
}