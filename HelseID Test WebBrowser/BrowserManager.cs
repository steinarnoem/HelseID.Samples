using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

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
