using AngleSharp;
using AngleSharp.XPath;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Webscan.Scanner
{
    public class WebScannerService : IWebScannerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebScannerService> _logger;
        private readonly IBrowsingContext _context;
        private readonly WebScannerSettings _webScannerSettings; 

        public WebScannerService(IHttpClientFactory httpClientFactory, ILogger<WebScannerService> logger, WebScannerSettings webScannerSettings)
        {
            //Use the default configuration for AngleSharp
            //Create a new context for evaluating webpages with the given config
            _context = BrowsingContext.New(Configuration.Default);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (httpClientFactory == null) throw new ArgumentNullException($"{nameof(httpClientFactory)} cannot be null");
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException($"{nameof(httpClientFactory)} cannot be null");
            _webScannerSettings = webScannerSettings ?? throw new ArgumentNullException($"{nameof(webScannerSettings)} cannot be null");            
        }

        public async Task<string> GetDocument(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException($"{nameof(request)} Cannot be null.");            
            using(HttpClient httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, _webScannerSettings.HttpRequestTimeOutInSeconds);
                HttpResponseMessage result = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();
                using (Stream responseStream = await result.Content.ReadAsStreamAsync())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }

        }

        public async Task<string> GetDocumentStealth(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException($"{nameof(request)} Cannot be null.");

            using (HttpClient httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.Timeout = new TimeSpan(0, 0, _webScannerSettings.HttpRequestTimeOutInSeconds);
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_0_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
                request.Headers.TryAddWithoutValidation("Connection", "keep-alive");

                using (var response = await httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        return await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }            
        }


        public async Task<string> GetXpathText(string markup, string xPath)
        {
            if (string.IsNullOrWhiteSpace(markup)) throw new ArgumentNullException($"{nameof(markup)} cannot be null or whitespace.");
            if (string.IsNullOrWhiteSpace(xPath)) throw new ArgumentNullException($"{nameof(xPath)} cannot be null or whitespace.");
            //Create a virtual request to specify the document to load (here from our fixed string)

            var document = await _context.OpenAsync(req => req.Content(markup));
            return document.Body.SelectSingleNode(xPath).TextContent;
        }
    }
}
