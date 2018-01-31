using System.Linq;
using mshtml;

namespace HelseID.Clients.WPF.EmbeddedBrowser.Model
{
    internal class WebBrowserDocument
    {
        private readonly IHTMLDocument3 _webBrowserDocument;
        public WebBrowserDocument(IHTMLDocument3 webBrowserDocument)
        {
            _webBrowserDocument = webBrowserDocument;
        }

        public string Data
        {
            get
            {
                var inputElements = _webBrowserDocument.getElementsByTagName("INPUT").OfType<IHTMLElement>();
                var resultUrl = "?";
                
                foreach (var input in inputElements)
                {
                    resultUrl += input.getAttribute("name") + "=";
                    resultUrl += input.getAttribute("value") + "&";
                }

                resultUrl = resultUrl.TrimEnd('&');

                return resultUrl;
            }
        }
    }
}