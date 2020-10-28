using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AllegroSearchService.Common;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.AllegroPl.Rest;
using KioskBrains.Clients.YandexTranslate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoapService;

namespace KioskBrains.Clients.AllegroPl
{
    /// <summary>
    /// Keep in mind that it's a singleton.
    /// </summary>
    public class AllegroPlClient
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly AllegroPlClientSettings _settings;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private readonly YandexTranslateClient _yandexTranslateClient;

        private readonly ILogger<AllegroPlClient> _logger;

        private readonly RestClient _restClient;

        public AllegroPlClient(
            IOptions<AllegroPlClientSettings> settings,
            YandexTranslateClient yandexTranslateClient,
            ILogger<AllegroPlClient> logger)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));

            _yandexTranslateClient = yandexTranslateClient;
            _logger = logger;

            _restClient = new RestClient(_settings.ApiClientId, _settings.ApiClientSecret);
        }

        #region Search

        private const int MaxPageSize = 10;

        public async Task<SearchOffersResponse> SearchOffersAsync(
            string phrase,
            string translatedPhrase,
            string categoryId,
            OfferStateEnum state,
            OfferSortingEnum sorting,
            int offset,
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit > MaxPageSize)
            {
                throw new NotSupportedException($"Max '{MaxPageSize}' page size is supported.");
            }

            if (string.IsNullOrEmpty(translatedPhrase)
                && !string.IsNullOrEmpty(phrase))
            {
                if (_settings.IsTranslationEnabled)
                {
                    translatedPhrase = await _yandexTranslateClient.TranslateAsync(
                        phrase,
                        Languages.RussianCode,
                        Languages.PolishCode,
                        cancellationToken);
                }
                else
                {
                    translatedPhrase = phrase;
                }
            }

            // WORKAROUND! since WebAPI with attributes was disabled:
            // - we can't use 'All States' filter at the moment
            // - 'Used' is set instead of 'All States'
            /*if (state == OfferStateEnum.All)
            {
                state = OfferStateEnum.Used;
            }*/

            // search for offers
            var apiResponse = await _restClient.SearchOffersAsync(translatedPhrase, categoryId, state, sorting, offset, limit, cancellationToken);
            var apiOffers = new List<Rest.Models.Offer>();
            if (apiResponse.Items.Promoted?.Length > 0)
            {
                apiOffers.AddRange(apiResponse.Items.Promoted);
            }

            if (apiResponse.Items.Regular?.Length > 0)
            {
                apiOffers.AddRange(apiResponse.Items.Regular);
            }

            if (apiOffers.Count > MaxPageSize)
            {
                apiOffers = apiOffers
                    .Take(MaxPageSize)
                    .ToList();
            }

            var offers = apiOffers
                .Select(x => new Offer()
                {
                    Id = x.Id,
                    CategoryId = x.Category?.Id,
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = x.Name ?? "",
                    },
                    Price = x.SellingMode?.GetMaxPrice()?.Amount ?? 0,
                    PriceCurrencyCode = x.SellingMode?.GetMaxPrice()?.Currency ?? "PLN",
                    Images = x.Images,
                    // WORKAROUND! since WebAPI with attributes was disabled:
                    // - state should be non-All here
                    // - delivery options are set by REST API delivery/lowestPrice
                    State = state,
                    DeliveryOptions = new[]
                    {
                        new DeliveryOption()
                        {
                            Price = x.Delivery?.LowestPrice?.Amount ?? 0,
                        },
                    }
                })
                .ToArray();

            foreach(var o in offers)
            {
                Thread.Sleep(10);
                o.ExtraData = _restClient.GetExtraData(o.Id, cancellationToken);
            }

            var stateAndDeliveryOptionsTask = Task.Run(
                () => RequestOfferStatesAndDeliveryOptionsAsync(offers, state, cancellationToken),
                cancellationToken);

            var translateTask = Task.Run(async () =>
                {
                    if (_settings.IsTranslationEnabled)
                    {
                        // translate offer texts
                        await TranslateNamesAsync(offers, cancellationToken);
                    }
                },
                cancellationToken);

            await Task.WhenAll(stateAndDeliveryOptionsTask, translateTask);

            return new SearchOffersResponse()
            {
                Offers = offers,
                TranslatedPhrase = translatedPhrase,
                Total = apiResponse.SearchMeta?.TotalCount ?? 0,
            };
        }

        private const string StateAttributeName = "Stan";

        private async Task RequestOfferStatesAndDeliveryOptionsAsync(
            Offer[] offers,
            OfferStateEnum requestState,
            CancellationToken cancellationToken)
        {
            // disabled
            return;

            //Assure.ArgumentNotNull(offers, nameof(offers));

            //if (offers.Length == 0)
            //{
            //    return;
            //}

            //var offerIds = offers
            //    .Select(x => long.Parse(x.Id))
            //    .ToArray();

            //var isStateSpecifiedByRequest = requestState != OfferStateEnum.All;

            //var infoResponse = await _soapClient.GetItemsInfoAsync(
            //    offerIds,
            //    includeAttributes: !isStateSpecifiedByRequest,
            //    includeDeliveryOptions: true,
            //    includeDescription: false,
            //    cancellationToken: cancellationToken);

            //var infoRecords = infoResponse.arrayItemListInfo ?? new ItemInfoStruct[0];
            //var infoRecordsById = infoRecords
            //    .Where(x => x.itemInfo?.itId != null)
            //    .ToDictionary(x => x.itemInfo.itId.ToString());
            //foreach (var offer in offers)
            //{
            //    var infoRecord = infoRecordsById.GetValueOrDefault(offer.Id);

            //    // STATE
            //    if (isStateSpecifiedByRequest)
            //    {
            //        offer.State = requestState;
            //    }
            //    else
            //    {
            //        const OfferStateEnum StateDefaultValue = OfferStateEnum.New;

            //        if (infoRecord != null)
            //        {
            //            switch (infoRecord.itemInfo?.itIsNewUsed)
            //            {
            //                case 1:
            //                    offer.State = OfferStateEnum.New;
            //                    break;
            //                case 2:
            //                    offer.State = OfferStateEnum.Used;
            //                    break;
            //                default:
            //                    var stateValue = infoRecord.itemAttribs
            //                        ?.Where(x => x.attribName?.Equals(StateAttributeName, StringComparison.OrdinalIgnoreCase) == true)
            //                        .Select(x => x.attribValues?.FirstOrDefault())
            //                        .FirstOrDefault();
            //                    if (stateValue == null)
            //                    {
            //                        offer.State = StateDefaultValue;
            //                    }
            //                    else if (stateValue.StartsWith("Regen", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        offer.State = OfferStateEnum.Recovered;
            //                    }
            //                    else if (stateValue.StartsWith("Usz", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        offer.State = OfferStateEnum.Broken;
            //                    }
            //                    else
            //                    {
            //                        offer.State = StateDefaultValue;
            //                    }

            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            offer.State = StateDefaultValue;
            //        }
            //    }

            //    // DELIVERY OPTIONS
            //    offer.DeliveryOptions = infoRecord
            //        ?.itemPostageOptions
            //        ?.Where(x => IsPolandShipment(x))
            //        .Select(x => new DeliveryOption()
            //        {
            //            Price = (decimal)x.postageAmount,
            //        })
            //        .ToArray();
            //}
        }

        private bool IsPolandShipment(PostageStruct postageStruct)
        {
            var shipmentId = postageStruct.postageId;
            return shipmentId < 100 || shipmentId > 200;
        }

        private async Task TranslateNamesAsync(Offer[] offers, CancellationToken cancellationToken)
        {
            try
            {
                var texts = offers
                    .Select(x => x.Name.GetValue(Languages.PolishCode))
                    .ToArray();

                var translatedTexts = await _yandexTranslateClient.TranslateAsync(
                    texts,
                    Languages.PolishCode,
                    Languages.RussianCode,
                    cancellationToken);
               

                for (var i = 0; i < offers.Length; i++)
                {
                    var offer = offers[i];
                    offer.Name[Languages.RussianCode] = translatedTexts[i];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Offer names translation failed.", ex);
            }
        }


        
        

        #endregion

        #region Description

        public async Task<MultiLanguageString> GetOfferDescriptionAsync(string offerId, CancellationToken cancellationToken)
        {
            //if (string.IsNullOrEmpty(offerId)
            //    || !long.TryParse(offerId, out var offerIdValue))
            //{
            //    return null;
            //}

            //var infoResponse = await _soapClient.GetItemsInfoAsync(
            //    new[] { offerIdValue },
            //    includeAttributes: true,
            //    includeDeliveryOptions: false,
            //    includeDescription: true,
            //    cancellationToken: cancellationToken);

            //var infoRecord = infoResponse.arrayItemListInfo?.FirstOrDefault();
            //if (infoRecord == null)
            //{
            //    return null;
            //}

            //var descriptionBuilder = new StringBuilder();

            //// attributes first (except State)
            //if (infoRecord.itemAttribs?.Length > 0)
            //{
            //    var attributeDescriptions = new List<string>();
            //    foreach (var attributeRecord in infoRecord.itemAttribs)
            //    {
            //        if (attributeRecord.attribName.Equals(StateAttributeName, StringComparison.OrdinalIgnoreCase))
            //        {
            //            // state
            //            continue;
            //        }

            //        var attributeValues = attributeRecord.attribValues
            //            ?.Where(x => !string.IsNullOrEmpty(x))
            //            .ToArray();
            //        if (attributeValues?.Length > 0)
            //        {
            //            attributeDescriptions.Add($"{attributeRecord.attribName}: {string.Join(", ", attributeValues)}");
            //        }
            //    }

            //    if (attributeDescriptions.Count > 0)
            //    {
            //        descriptionBuilder.AppendLine(string.Join("; ", attributeDescriptions));
            //    }
            //}

            //// description text sections
            //var descriptionSectionsJson = infoRecord.itemInfo?.itStandardizedDescription;
            //if (!string.IsNullOrEmpty(descriptionSectionsJson))
            //{
            //    try
            //    {
            //        var descriptionSections = JsonConvert.DeserializeObject<DescriptionSections>(descriptionSectionsJson);
            //        var descriptionSectionItems = descriptionSections
            //                                          ?.Sections
            //                                          ?.SelectMany(x => x.Items ?? new DescriptionSectionItem[0])
            //                                          .ToArray()
            //                                      ?? new DescriptionSectionItem[0];
            //        foreach (var descriptionSectionItem in descriptionSectionItems)
            //        {
            //            if (descriptionSectionItem.Type?.Equals("TEXT", StringComparison.OrdinalIgnoreCase) != true)
            //            {
            //                // only text items
            //                continue;
            //            }

            //            if (string.IsNullOrEmpty(descriptionSectionItem.Content))
            //            {
            //                continue;
            //            }

            //            var contentText = ConvertDescriptionHtmlToText(descriptionSectionItem.Content);
            //            if (!string.IsNullOrEmpty(contentText))
            //            {
            //                descriptionBuilder.AppendLine(contentText);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(LoggingEvents.ResponseProcessingError, $"Offer '{offerId}' bad description format.", ex);
            //    }
            //}

            //var plDescription = descriptionBuilder
            //    .ToString()
            //    .Trim();

            var description = new MultiLanguageString()
            {
                // WORKAROUND! since WebAPI with attributes was disabled:
                // - description is disabled
                [Languages.PolishCode] = null,
                //[Languages.PolishCode] = plDescription,
            };

            // DISABLED TO SAVE MONEY
            //if (_settings.IsTranslationEnabled)
            //{
            //    try
            //    {
            //        var ruDescription = await _yandexTranslateClient.TranslateAsync(
            //            plDescription,
            //            Languages.PolishCode,
            //            Languages.RussianCode,
            //            cancellationToken);

            //        description[Languages.RussianCode] = ruDescription;
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(LoggingEvents.ExternalApiError, $"Offer '{offerId}' description translation failed.", ex);
            //    }
            //}

            return description;
        }

        #endregion

        #region HtmlToText

        // http://htmlbook.ru/samhtml/tekst/spetssimvoly
        private static readonly Dictionary<string, string> SpecialHtmlSymbolReplacements = new Dictionary<string, string>()
        {
            ["&nbsp;"] = " ",
            ["&amp;"] = "&",
            ["&quot;"] = "\"",
            ["&lt;"] = "<",
            ["&gt;"] = ">",
            ["&copy;"] = "©",
            ["&reg;"] = "®",
            ["&trade;"] = "™",
        };

        // http://www.beansoftware.com/ASP.NET-Tutorials/Convert-HTML-To-Plain-Text.aspx
        // https://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text-2
        private string ConvertDescriptionHtmlToText(string html)
        {
            Assure.ArgumentNotNull(html, nameof(html));

            // removal of head/script are not required since they are not presented in Allegro description HTML

            var htmlStringBuilder = new StringBuilder(html);

            // remove new lines since they are not visible in HTML
            htmlStringBuilder.Replace("\n", " ");
            htmlStringBuilder.Replace("\r", " ");

            // remove tab spaces
            htmlStringBuilder.Replace("\t", " ");

            // replace special characters like &, <, >, " etc.
            foreach (var (specialSymbol, replacement) in SpecialHtmlSymbolReplacements)
            {
                htmlStringBuilder.Replace(specialSymbol, replacement);
            }

            // insert line breaks, spaces, etc. (simple replace is used instead of regex since allegro description contains HTML tags without attributes)
            htmlStringBuilder.Replace("<p>", "\n<p>");
            htmlStringBuilder.Replace("<h1>", "\n<h1>");
            htmlStringBuilder.Replace("<h2>", "\n<h2>");
            htmlStringBuilder.Replace("<h3>", "\n<h3>");
            htmlStringBuilder.Replace("<tr>", "\n<tr>");
            htmlStringBuilder.Replace("<td>", " <td>");
            htmlStringBuilder.Replace("<li>", "\n- <li>");

            html = htmlStringBuilder.ToString();

            // remove others special symbols
            html = Regex.Replace(html, @"&(.{2,6});", "", RegexOptions.IgnoreCase);

            // remove all HTML tags
            html = Regex.Replace(html, "<[^>]*>", "");

            // remove multiple spaces
            html = Regex.Replace(html, " +", " ");

            // remove first space in line
            html = html
                .Replace("\n ", "\n")
                .Trim();

            return html;
        }

        #endregion
    }
}