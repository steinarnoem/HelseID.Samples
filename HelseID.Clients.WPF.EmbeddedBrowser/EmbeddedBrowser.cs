using HelseID.Common.Browser;
using System;
using System.Windows.Controls;

namespace HelseID.Clients.WPF.EmbeddedBrowser
{
    public class EmbeddedBrowser : Browser
    {
        private WebBrowser _browser;

        public EmbeddedBrowser(WebBrowser browser, string uri)
            :base(uri)
        {
            _browser = browser;
        }

        public override void OpenBrowser(string url)
        {
            _browser.Source = new Uri(url);
        }
    }
}
