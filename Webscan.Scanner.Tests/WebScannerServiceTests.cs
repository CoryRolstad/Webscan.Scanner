
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Webscan.Scanner.Tests
{

    public class WebScannerServiceTests
    {
        private readonly WebScannerSettings _webScannerSettings;
        private readonly Mock<IServiceCollection> _serviceCollectionMock;
        private readonly Mock<ILogger<WebScannerService>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IOptionsSnapshot<WebScannerSettings>> _options;
        private readonly string _resultHtmlString, _resultXmlString, _xPathHtmlString, _xPathXmlString, _expectedHtmlXpathReturnString;
        private readonly HttpResponseMessage _httpResponseMessage, _xmlResponseMessage;
        private readonly HttpRequestMessage _httpRequestMessage; 

        public WebScannerServiceTests()
        {
            _webScannerSettings = new WebScannerSettings()
            {
                Uri = "https://test.com"
            };
            _serviceCollectionMock = new Mock<IServiceCollection>();
            _loggerMock = new Mock<ILogger<WebScannerService>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _options = new Mock<IOptionsSnapshot<WebScannerSettings>>();

            _resultHtmlString = "<!doctype html><html lang=\"en\"><head>  <meta charset=\"utf - 8\">  <title>The HTML5 Herald</title>  <meta name=\"description\" content=\"The HTML5 Herald\">  <meta name=\"author\" content=\"SitePoint\">  <link rel=\"stylesheet\" href=\"css / styles.css ? v = 1.0\"></head><body>  <script src=\"js / scripts.js\"></script>  <button type=\"button\">Button Text</button></body></html>";
            _resultXmlString = "";

            _xPathHtmlString = "/html/body/button";
            _xPathXmlString = "";

            _expectedHtmlXpathReturnString = "Button Text";

            _httpResponseMessage = new HttpResponseMessage();
            _httpResponseMessage.StatusCode = HttpStatusCode.OK;
            _httpResponseMessage.Content = new StringContent(_resultHtmlString);

            _httpResponseMessage = new HttpResponseMessage();
            _httpResponseMessage.StatusCode = HttpStatusCode.OK;
            _httpResponseMessage.Content = new StringContent(_resultXmlString);

            _httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://www.test.com");
        }

        [Fact]
        [Trait("Category", "WebScannerServiceCollectionExtensions")]
        public void WebScannerServiceCollectionExtensions_AddServiceWithNullSettingsValue_ThrowsArguementNullException()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _serviceCollectionMock.Object.AddWebScannerService(null);
            });
        }

        [Fact]
        [Trait("Category", "WebScannerService")]
        public void WebScannerService_InitializeNullHttpClientFactory_ThrowsArgumentNullException()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                WebScannerService ServerInventoryService = new WebScannerService(null, _loggerMock.Object);
            });
        }

        [Fact]
        [Trait("Category", "WebScannerService")]
        public void WebScannerService_InitializeNullLogger_ThrowsArgumentNullException()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                WebScannerService ServerInventoryService = new WebScannerService(_httpClientFactoryMock.Object, null);
            });
        }

        [Fact]
        [Trait("Category", "WebScannerService")]
        public async void WebScannerService_GetDocumentWithBadValues_ThrowsArgumentError()
        {
            //Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_resultHtmlString),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _options.Setup(option => option.Get(It.IsAny<string>())).Returns(_webScannerSettings);
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);
            HttpRequestMessage requestNullReference = new HttpRequestMessage();

            // Act
            WebScannerService webScannerService = new WebScannerService(_httpClientFactoryMock.Object, _loggerMock.Object);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                string actual = await webScannerService.GetDocument(null);
            });

            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                string actual = await webScannerService.GetDocument(requestNullReference);
            });

        }

        [Fact]
        [Trait("Category", "WebScannerService")]
        public async void WebScannerService_GetDocumentWith404HttpStatusCode_RaisesHttpRequestException()
        {
            //Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.NotFound,
                   Content = new StringContent(_resultHtmlString),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _options.Setup(option => option.Get(It.IsAny<string>())).Returns(_webScannerSettings);
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            WebScannerService webScannerService = new WebScannerService(_httpClientFactoryMock.Object, _loggerMock.Object);

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                string actual = await webScannerService.GetDocument(_httpRequestMessage);
            });
        }


        [Fact]
        [Trait("Category", "WebScannerService")]
        public async void WebScannerService_GetDocument_ReturnsDocument()
        {
            //Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_resultHtmlString),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _options.Setup(option => option.Get(It.IsAny<string>())).Returns(_webScannerSettings);
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            WebScannerService webScannerService = new WebScannerService(_httpClientFactoryMock.Object, _loggerMock.Object);
            string expected = _resultHtmlString;
            string actual = await webScannerService.GetDocument(_httpRequestMessage);

            //Assert
            actual.Should().Be(expected);
        }



        [Fact]
        [Trait("Category", "WebScannerService")]
        public async void WebScannerService_GetXpathTextWithBadValues_ThrowsArgumentError()
        {
            //Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_resultHtmlString),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _options.Setup(option => option.Get(It.IsAny<string>())).Returns(_webScannerSettings);
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);
            HttpRequestMessage requestNullReference = new HttpRequestMessage();

            // Act
            WebScannerService webScannerService = new WebScannerService(_httpClientFactoryMock.Object, _loggerMock.Object);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string actual = await webScannerService.GetXpathText(null, _xPathHtmlString);
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string actual = await webScannerService.GetXpathText(_resultHtmlString, null);
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string actual = await webScannerService.GetXpathText(string.Empty, _xPathHtmlString);
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                string actual = await webScannerService.GetXpathText(_resultHtmlString, string.Empty);
            });
        }

        [Fact]
        [Trait("Category", "WebScannerService")]
        public async void WebScannerService_GetXpathText_ReturnsStringValue()
        {
            //Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_resultHtmlString),
               })
               .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            _options.Setup(option => option.Get(It.IsAny<string>())).Returns(_webScannerSettings);
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);
            HttpRequestMessage requestNullReference = new HttpRequestMessage();

            // Act
            WebScannerService webScannerService = new WebScannerService(_httpClientFactoryMock.Object, _loggerMock.Object);
            string actual = await webScannerService.GetXpathText(_resultHtmlString, _xPathHtmlString);

            //Assert    
            actual.Should().Be(_expectedHtmlXpathReturnString);
        }


    }
}
