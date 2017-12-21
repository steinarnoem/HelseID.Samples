using System.Linq;
using mshtml;

namespace HelseID.Test.WPF.Common
{
    public static class WebBrowserControlHelper
    {
        public static string GetResponseDataFromFormPostPage(dynamic browser)
        {
            var document = (IHTMLDocument3)browser.Document;
            var inputElements = document.getElementsByTagName("INPUT").OfType<IHTMLElement>();
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
