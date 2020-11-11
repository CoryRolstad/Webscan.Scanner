using AngleSharp;
using AngleSharp.XPath;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Webscan.Scanner
{
    public class WebScannerService : IWebScannerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebScannerService> _logger;
        private readonly IBrowsingContext _context;
        
        public WebScannerService(IHttpClientFactory httpClientFactory, ILogger<WebScannerService> logger)
        {
            //Use the default configuration for AngleSharp
            //Create a new context for evaluating webpages with the given config
            _context = BrowsingContext.New(Configuration.Default);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClient = httpClientFactory.CreateClient("WebScannerHttpClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<string> GetDocument(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException($"{nameof(request)} Cannot be null.");            
            _logger.LogDebug($"Executing Http Request: {request.RequestUri.ToString()}");
            HttpResponseMessage result = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            _logger.LogDebug($"HTTP Request Comlete: {request.RequestUri.ToString()}");
            _logger.LogDebug($"\tResult HTTP Code: {result.StatusCode}");

            result.EnsureSuccessStatusCode();
            using (Stream responseStream = await result.Content.ReadAsStreamAsync())
            {
                using (StreamReader streamReader = new StreamReader(responseStream))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public async Task<string> GetXpathText(string markup, string xPath)
        {
            if (string.IsNullOrWhiteSpace(markup)) throw new ArgumentNullException($"{nameof(markup)} cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(xPath)) throw new ArgumentNullException($"{nameof(xPath)} cannot be null or whitespace.");
            //Create a virtual request to specify the document to load (here from our fixed string)

            _logger.LogDebug("GetXpathText values:");
            _logger.LogDebug($"\tMarkup: {markup}");
            _logger.LogDebug($"\tXpath: {xPath}");
            var document = await _context.OpenAsync(req => req.Content(markup));
            return document.Body.SelectSingleNode(xPath).TextContent;
        }
    }
}
