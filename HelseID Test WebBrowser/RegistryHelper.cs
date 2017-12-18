using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace HelseID.Test.WPF.WebBrowser
{
    internal class RegistryHelper
    {
        private const string PackagesKey = @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Packages\";
        private const string BrowserEmulationKey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION\";
        private const string BrowserEmulationKey64 = @"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION";
        private const string InternetExplorerKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer";

        /// <summary>
        /// Konfigurererer systemet til å bruke siste versjon av IE eller Edge
        /// Oppdaterer registry key Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl
        /// </summary>
        internal void Configure()
        {
            var executableFileName = GetExeName();
            var registryKey = GetKey();

            SetBrowserVersion(registryKey, executableFileName);
        }

        private void SetBrowserVersion(RegistryKey registryKey, string executableFileName)
        {
            if (IsEdgeInstalled())
                registryKey.SetValue(executableFileName, BrowserEmulationValues.Edge, RegistryValueKind.DWord);
            else
            {
                var version = GetBrowserVersion();
                var browserValue = BrowserEmulationValues.BrowserVersion(version);
                registryKey.SetValue(executableFileName, browserValue, RegistryValueKind.DWord);
            }
        }

        private static string GetExeName()
        {
            var exeName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            return exeName;
        }

        private static RegistryKey GetKey()
        {
            try
            {
                RegistryKey key;
                if (Environment.Is64BitOperatingSystem)
                    key = Registry.LocalMachine.OpenSubKey(BrowserEmulationKey64, true);
                else  //For 32 bit machine
                    key = Registry.LocalMachine.OpenSubKey(BrowserEmulationKey, true);

                if (key == null)
                {
                    throw new Exception("Fant ikke innstillinger for systemnettleser : nøkkel i regedit FEATURE_BROWSER_EMULATION");
                }

                return key;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private bool IsEdgeInstalled()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(PackagesKey))
            {
                if (key == null) return false;

                if (key.GetSubKeyNames().Any(subkey => subkey.StartsWith("Microsoft.MicrosoftEdge_")))
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetBrowserVersion()
        {            
            const string keyPath = InternetExplorerKey;
            var keys = new[] { "svcVersion", "svcUpdateVersion", "Version", "W2kVersion" };

            var maxVer = 0;
            foreach (var key in keys)
            {
                var regKeyValue = Registry.GetValue(keyPath, key, "0");
                if (regKeyValue == null) continue;

                var value = regKeyValue as string;
                if (value != null)
                {
                    var pos = value.IndexOf('.');
                    if (pos > 0)
                        value = value.Substring(0, pos);
                }

                if (int.TryParse(value, out int res))
                    maxVer = Math.Max(maxVer, res);
            }

            return maxVer;
        } 


    }
}
