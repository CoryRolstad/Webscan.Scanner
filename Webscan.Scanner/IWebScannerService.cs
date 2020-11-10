using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Webscan.Scanner
{
    public interface IWebScannerService
    {
        /// <summary>
        /// GetPageHtml - Gets the html for a given pageUri 
        /// </summary>
        /// <param name="request">HttpRequestMessage</param>
        /// <returns> Task of string </returns>
        public Task<string> GetPageHtml(HttpRequestMessage request);

        /// <summary>
        /// GetXpathText - Retrieves the Text in a given Xpath Element
        /// </summary>
        /// <param name="markup">The xml or html markup</param>
        /// <param name="xPath">The xpath of the elemenet you would like to get the text from</param>
        /// <returns>The string of the element requested</returns>
        public Task<string> GetXpathText(string markup, string xPath);


    }
}
