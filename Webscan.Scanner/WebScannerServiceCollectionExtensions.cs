using Microsoft.Extensions.DependencyInjection;
using System;

namespace Webscan.Scanner
{
    public static class WebScannerServiceCollectionExtensions
    {
        public static IServiceCollection AddWebScannerService(this IServiceCollection services, WebScannerSettings webScannerSettings)
        {
            ValidateSettings(services, webScannerSettings);

            // Register our WebScannerHttpClient with a delegate handler
            //services.AddHttpClient("WebScannerHttpClient", client =>
            //{
            //UriBuilder uribuilder = new UriBuilder()
            //{
            //    Host = webScannerSettings.Uri,
            //    Scheme = "https"
            //};
            //client.BaseAddress = uribuilder.Uri;
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.ConnectionClose = true;
            //});

            services.AddHttpClient();

            services.AddTransient<IWebScannerService, WebScannerService>();
            return services; 
        }

        private static void ValidateSettings(IServiceCollection services, WebScannerSettings webScannerSettings)
        {
            if (services == null) throw new ArgumentNullException($"{nameof(services)} cannot be null");
            if (webScannerSettings == null) throw new ArgumentNullException($"{nameof(webScannerSettings)} cannot be null");
            //if (string.IsNullOrWhiteSpace(webScannerSettings.Uri)) throw new ArgumentNullException($"{nameof(webScannerSettings.Uri)} cannot be null");
        }
    }
}
