using System;
using System.Threading;
using System.Threading.Tasks;
using AllegroSearchService.Common;
using SoapService;

namespace KioskBrains.Clients.AllegroPl.Soap
{
    /// <summary>
    /// TODO: replace with REST API when analog of doGetItemsInfo is added to REST API.
    /// </summary>
    internal class SoapClient_WebApiDisabled
    {
        private readonly string _webApiLogin;
        private readonly string _webApiPassword;
        private readonly string _webApiKey;

        public SoapClient_WebApiDisabled(
            string webApiLogin,
            string webApiPassword,
            string webApiKey)
        {
            Assure.ArgumentNotNull(webApiLogin, nameof(webApiLogin));
            Assure.ArgumentNotNull(webApiPassword, nameof(webApiPassword));
            Assure.ArgumentNotNull(webApiKey, nameof(webApiKey));

            _webApiLogin = webApiLogin;
            _webApiPassword = webApiPassword;
            _webApiKey = webApiKey;
        }

        private const int CountryId = 1; // Poland

        private readonly object _versionKeyLocker = new object();

        private bool _versionRequestInProgress;

        private class VersionKeyInfo
        {
            public long VersionKey { get; }

            public DateTime IssuedOn { get; }

            public VersionKeyInfo(long versionKey, DateTime issuedOn)
            {
                VersionKey = versionKey;
                IssuedOn = issuedOn;
            }
        }

        private VersionKeyInfo _versionKeyInfo;

        private async Task<VersionKeyInfo> GetVersionKeyAsync(CancellationToken cancellationToken)
        {
            lock (_versionKeyLocker)
            {
                // version key is obtained only once (but it's sometimes invalidated by Allegro - see GetItemsInfoAsync exception handling)
                if (_versionKeyInfo != null)
                {
                    return _versionKeyInfo;
                }

                if (_versionRequestInProgress)
                {
                    // possible for parallel requests after app started
                    throw new AllegroPlRequestException("Version key is being obtained...");
                }

                _versionRequestInProgress = true;
            }

            try
            {
                var client = new servicePortClient();
                var sysStatus = await client.doQuerySysStatusAsync(new doQuerySysStatusRequest(1, CountryId, _webApiKey));

                lock (_versionKeyLocker)
                {
                    _versionKeyInfo = new VersionKeyInfo(sysStatus.verKey, DateTime.Now);

                    cancellationToken.ThrowIfCancellationRequested();

                    return _versionKeyInfo;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException($"Request {nameof(servicePortClient.doQuerySysStatusAsync)} failed.", ex);
            }
            finally
            {
                lock (_versionKeyLocker)
                {
                    _versionRequestInProgress = false;
                }
            }
        }

        public async Task<doGetItemsInfoResponse> GetItemsInfoAsync(
            long[] offerIds,
            bool includeAttributes,
            bool includeDeliveryOptions,
            bool includeDescription,
            CancellationToken cancellationToken)
        {
            Assure.ArgumentNotNull(offerIds, nameof(offerIds));
            const int MaxItemsPerRequest = 10;
            if (offerIds.Length > MaxItemsPerRequest)
            {
                throw new NotSupportedException($"Max '{MaxItemsPerRequest}' items per request is supported.");
            }

            var requestVersionKeyInfo = await GetVersionKeyAsync(cancellationToken);

            try
            {
                var client = new servicePortClient();

                // session id can't be cached in cloud since WebApi bind session id to IP
                // todo: refactor after static IP for all external requests
                var loginResponse = await client.doLoginAsync(
                    new doLoginRequest(
                        _webApiLogin,
                        _webApiPassword,
                        CountryId,
                        _webApiKey,
                        requestVersionKeyInfo.VersionKey));
                cancellationToken.ThrowIfCancellationRequested();

                var response = await client.doGetItemsInfoAsync(
                    new doGetItemsInfoRequest(
                        loginResponse.sessionHandlePart,
                        offerIds,
                        getDesc: includeDescription ? 1 : 0, // description
                        getImageUrl: 0,
                        getAttribs: includeAttributes ? 1 : 0, // attributes (including state)
                        getPostageOptions: includeDeliveryOptions ? 1 : 0, // delivery options
                        getCompanyInfo: 0,
                        getProductInfo: 0,
                        getAfterSalesServiceConditions: 0,
                        getEan: 0,
                        getAdditionalServicesGroup: 0));
                cancellationToken.ThrowIfCancellationRequested();

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex.Message?.StartsWith("Niepoprawna wersja", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // most likely version key is invalidated
                    lock (_versionKeyLocker)
                    {
                        // if it's first request with invalidated version key
                        if (_versionKeyInfo != null
                            && _versionKeyInfo.IssuedOn == requestVersionKeyInfo.IssuedOn)
                        {
                            // cause version key refresh on the next request
                            _versionKeyInfo = null;
                        }
                    }
                }

                throw new AllegroPlRequestException("Request to WebApi failed.", ex);
            }
        }
    }
}