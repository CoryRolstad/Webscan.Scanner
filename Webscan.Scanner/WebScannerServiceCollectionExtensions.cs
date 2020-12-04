using Microsoft.Extensions.DependencyInjection;
using System;

namespace Webscan.Scanner
{
    public static class WebScannerServiceCollectionExtensions
    {
        public static IServiceCollection AddWebScannerService(this IServiceCollection services, WebScannerSettings webScannerSettings)
        {
            ValidateSettings(services, webScannerSettings);

            services.AddSingleton(webScannerSettings);

            services.AddHttpClient();

            services.AddTransient<IWebScannerService, WebScannerService>();
            return services; 
        }

        private static void ValidateSettings(IServiceCollection services, WebScannerSettings webScannerSettings)
        {
            if (services == null) throw new ArgumentNullException($"{nameof(services)} cannot be null");
            if (webScannerSettings == null) throw new ArgumentNullException($"{nameof(webScannerSettings)} cannot be null");
        }
    }
}
