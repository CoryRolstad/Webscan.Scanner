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

        public async Task<string> GetPageHtml(HttpRequestMessage request)
        {            
            HttpResponseMessage result = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
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
            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = await _context.OpenAsync(req => req.Content(markup));
            return document.Body.SelectSingleNode(xPath).TextContent;
        }
    }
}
