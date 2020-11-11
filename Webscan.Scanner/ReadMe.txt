
Webscan.Scanner MicroService Library
----------------------------------------------
The Webscan.Scanner is a Microservice that allows you to retrieve a documents from a URL and parse it via XPath, and return the string results.

Things to keep in mind
----------------------------------------------
You must wrap the calls in error handling, the http call will throw errors on non success HTTP status codes. 

Logging
--------
The library offers logging capabilities for debugging and troubleshooting. Debug level will offer application workflow and Trace will offer both application workflow and variable information. 

Configuration Format (Appsettings.json)
----------------------------------------
Use the Webscan.Scanner MicroService Settings format below within your appsettings Json

{
  ...
  "WebScannerSettings": {
    "Uri": "https://www.test.com"
  },
  ...
}

Example of how to use the Webscan.Scanner MicroService Library
---------------------------------------
The Webscan.Scanner MicroService Library can be injected into your project by leveraging the extension method while passing in the configuration.
This example shows the configuration being pulled from the appsettings.json, but it can be pulled from any other configuration.
As long as it is an Action\<T\>

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            //The only line you need to Add the ScmService to your project.
            services.AddWebScannerService(Configuration.GetSection("WebscannerSettings").Get<WebScannerSettings>());        
            ...
        }

The Webscan.Scanner MicroService Library is now in your DI Container and is fully configured and ready to be injected into your application and controllers
Here is an example of a controller utilizing the Webscan.Scanner MicroService library after injecting it with DI.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Webscan.Scanner;

namespace ScannerDemo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebScannerService _webScannerService; 

        public IndexModel(ILogger<IndexModel> logger, IWebScannerService webScannerService)
        {
            _webScannerService = webScannerService; 
            _logger = logger;
        }

        public async void OnGet()
        {
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://www.newegg.com/evga-geforce-rtx-3080-10g-p5-3897-kr/p/N82E16814487518?Description=3080%20ftw&cm_re=3080_ftw-_-14-487-518-_-Product")
            };

            string html = await _webScannerService.GetPageHtml(request);
            string buttonText = await _webScannerService.GetXpathText(html, "//*[@id=\"ProductBuy\"]/div/div/span");
        }
    }
}
