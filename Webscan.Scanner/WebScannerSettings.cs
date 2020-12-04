using AngleSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webscan.Scanner
{
    public class WebScannerSettings
    {
        public string Uri { get; set; }
        public int HttpRequestTimeOutInSeconds { get; set; }
    }
}
