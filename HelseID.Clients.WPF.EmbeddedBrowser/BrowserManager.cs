﻿using System;

namespace HelseID.Clients.WPF.EmbeddedBrowser
{
    public class BrowserManager
    {
        public BrowserManager()
        {
                   
        }

        public static void Initialize()
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
