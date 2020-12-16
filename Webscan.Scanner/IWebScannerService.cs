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
        public Task<string> GetDocument(HttpRequestMessage request);

        /// <summary>
        /// GetXpathText - Retrieves the Text in a given Xpath Element
        /// </summary>
        /// <param name="markup">The xml or html markup</param>
        /// <param name="xPath">The xpath of the elemenet you would like to get the text from</param>
        /// <returns>The string of the element requested</returns>
        public Task<string> GetXpathText(string markup, string xPath);

        /// <summary>
        /// GetDocumentStealth makes the request appear as it is coming from a webbrowswer.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<string> GetDocumentStealth(HttpRequestMessage request);

        /// <summary>
        /// GetDocumentWaitForXpath uses Puppeteer headless browser behind the scenes and will render the javascript and dom elements correctly.  
        /// This request queries the url and waits for the Xpath element to render properly before returning the document.
        /// </summary>
        /// <param name="request">HttpRequestMessage that is requested</param>
        /// <param name="xPath">XPath of the item that we are waiting to render before returning</param>
        /// <returns>The Document that was queried in a string</returns>
        public Task<string> GetDocumentWaitForXpath(HttpRequestMessage request, string xPath);


        /// <summary>
        /// GetDocumentWaitForSelectoruses Puppeteer headless browser behind the scenes and will render the javascript and dom elements correctly.  
        /// This request queries the url and waits for the dom element to render properly before returning the document.
        /// </summary>
        /// <param name="request">HttpRequestMessage that is requested</param>
        /// <param name="selector">CSS Selector of the item that we are waiting to render before returning</param>
        /// <returns>The Document that was queried in a string</returns>
        public Task<string> GetDocumentWaitForSelector(HttpRequestMessage request, string selector);

    }
}
