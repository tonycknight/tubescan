using System.Diagnostics.CodeAnalysis;
using TubeScan.Telemetry;

namespace TubeScan.Tfl
{
    [ExcludeFromCodeCoverage] // until HTTP is mocked out
    internal class TflHttpClient : ITflClient
    {
        private readonly ITelemetry _telemetry;
        private readonly IHttpClientFactory _httpFactory;
        private readonly Lazy<string> _appKey;
        private readonly string _tflDomain;

        public TflHttpClient(ITelemetry telemetry, IHttpClientFactory httpFactory, Config.IAppConfigurationProvider configProvider)
        {

#pragma warning disable CS8601 // Possible null reference assignment.
            _telemetry = telemetry;
            _httpFactory = httpFactory;
            _appKey = new Lazy<string>(() => configProvider.GetAppConfiguration().Tfl.AppKey);
            _tflDomain = configProvider.GetAppConfiguration().Tfl.TflUri;

#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public async Task<TflResponse> GetAsync(string path, bool appendSubKey)
        {            
            try
            {
                $"Sending GET request to TFL: {path}...".CreateTelemetryEvent(TelemetryEventKind.Trace).Send(_telemetry);
                var client = _httpFactory.CreateClient(ITflClient.HttpClientName);

                var uri = _tflDomain.CreateUri(path, null);

                using var msg = new HttpRequestMessage(HttpMethod.Get, uri);
                if (appendSubKey)
                {
                    msg.Headers.Add("app_key", _appKey.Value);
                }

                var resp = await client.SendAsync(msg);

                $"Received {resp.StatusCode} from TFL {path}.".CreateTelemetryEvent(TelemetryEventKind.Trace).Send(_telemetry);

                return new TflResponse()
                {
                    IsSuccess = resp.IsSuccessStatusCode,
                    HttpStatus = (int)resp.StatusCode,
                    Body = await resp.Content.ReadAsStringAsync(),
                    Headers = resp.Content.Headers
                                  .Concat(resp.Headers)
                                  .ToDictionary(h => h.Key, h => h.Value.ToArray()),
                };
            }
            catch (Exception ex)
            {
                ex.Message.CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                return new TflResponse()
                {
                    Exception = ex,
                    IsSuccess = false,
                    HttpStatus = (int)System.Net.HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
