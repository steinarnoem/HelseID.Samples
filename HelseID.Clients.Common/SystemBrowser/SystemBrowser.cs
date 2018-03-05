using System.Diagnostics;

namespace HelseID.Clients.Common.Browser
{

    public class SystemBrowser : Browser
    {
        public SystemBrowser(string uri) : base(uri)
        {
        }

        public override void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
        }
    }
}