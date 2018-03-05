using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;
using HelseID.Clients.Common;
using HelseID.Clients.Common.Clients;
using HelseID.Clients.Common.Extensions;
using HelseID.Clients.Common.Jwt;
using HelseID.Clients.Common.Oidc;
using HelseID.Clients.WPF.EmbeddedBrowser.EventArgs;
using HelseID.Clients.WPF.EmbeddedBrowser.Model;
using IdentityModel;
using IdentityModel.OidcClient;
using mshtml;

namespace HelseID.Clients.WPF.EmbeddedBrowser
{
    public delegate void LoginEventHandler(object sender, LoginEventArgs e);
    
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {        private readonly HelseIdClientOptions _clientOptions;

        public event LoginEventHandler OnLoginSuccess;
        public event LoginEventHandler OnLoginError;
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(HelseIdClientOptions options)
        {
            InitializeComponent();
            _clientOptions = options; 
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var browser = new EmbeddedBrowser(webBrowser, _clientOptions.RedirectUri);
            _clientOptions.Browser = browser;

            var client = new HelseIdClient(_clientOptions);

            var result = await client.Login();

            if (result.IsError)
            {
                OnLoginError?.Invoke(this, new LoginEventArgs()
                {
                    Response = result
                });
            }
            else
            {
                OnLoginSuccess?.Invoke(this, new LoginEventArgs()
                {
                    Response = result,
                    AccessToken = result.AccessToken,
                    IdentityToken = result.IdentityToken,
                    IsError = result.IsError,
                    Error = result.Error
                });
            }
        }



    
    }
}
