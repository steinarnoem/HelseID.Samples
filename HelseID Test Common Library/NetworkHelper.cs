﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelseID.Test.WPF.Common
{
    public class NetworkHelper
    {
        public static bool StsIsAvailable(string url)
        {            
            var request = (HttpWebRequest)WebRequest.Create(url);
            
            request.Timeout = 120;
            request.AllowAutoRedirect = true;
            
            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response == null) return false;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return true;
                        case HttpStatusCode.Redirect:
                            var uriString = response.Headers["Location"];
                            return StsIsAvailable(uriString);
                    }

                    response.Close();
                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Det oppstod en feil når vi sjekker om STSen er tilgjengelig for applikasjonen.{Environment.NewLine}Feilmelding: {e.Message}");
                return false;
            }
        }
    }
}
