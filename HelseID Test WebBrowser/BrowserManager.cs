using System;

namespace HelseID.Test.WPF.WebBrowser
{
    public class BrowserManager
    {
        public BrowserManager()
        {
                   
        }

        public void Initialize()
        {
            try
            {
                var registryHelper = new RegistryHelper();
                registryHelper.Configure();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

       
    }
}
