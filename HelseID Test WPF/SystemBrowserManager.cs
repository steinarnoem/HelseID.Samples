using System.Diagnostics;

namespace HelseID.Test.WPF
{
    internal class SystemBrowserManager
    {
        private Process _process;
         

        public void Start(string startUrl)
        {           
            _process = Process.Start(startUrl);              
        } 
    }
}
