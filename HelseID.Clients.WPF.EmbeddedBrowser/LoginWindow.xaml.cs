using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;
using HelseID.Clients.Common;
using HelseID.Clients.WPF.EmbeddedBrowser.EventArgs;
using HelseID.Clients.WPF.EmbeddedBrowser.Model;
using IdentityModel.OidcClient;
using mshtml;

namespace HelseID.Clients.WPF.EmbeddedBrowser
{
    public delegate void LoginEventHandler(object sender, LoginEventArgs e);
    
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private OidcClient _client;

        private readonly OidcClientOptions _clientOptions;
        private readonly JwtGenerator.SigningMethod _signingMethod;
        private AuthorizeState _state;

        public event LoginEventHandler OnLoginSuccess;
        public event LoginEventHandler OnLoginError;
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(OidcClientOptions options, JwtGenerator.SigningMethod signingMethod)
        {
            InitializeComponent();
            _clientOptions = options;
            _signingMethod = signingMethod;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _client = new OidcClient(_clientOptions);
            _state = await _client.PrepareLoginAsync();

            webBrowser.Source = new Uri(_state.StartUrl);
        }

        private async void WebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (CheckIfErrorOccurredInHelseId(e))
            {
                HandleErrorInHelseId(e);                
                return;
            }

            var uri = e.Uri.ToString();
            if (uri != _clientOptions.RedirectUri) return;
            
            await HandleLogin(e);
        }

        private async Task HandleLogin(NavigatingCancelEventArgs e)
        {
            var response = new WebBrowserDocument((IHTMLDocument3)webBrowser.Document);

            object extraParams = null;

            var discoveryDocument = await OidcDiscoveryHelper.GetDiscoveryDocument(_clientOptions.Authority);

                if (_signingMethod != JwtGenerator.SigningMethod.None)
                {
                    var clientAssertion = ClientAssertion.CreateWithRsaKeys(_clientOptions.ClientId, discoveryDocument.TokenEndpoint, _signingMethod);
                    extraParams = clientAssertion;
                }            

            var result = await _client.ProcessResponseAsync(response.Data, _state, extraParams);

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

            e.Cancel = true;
        }

        private static bool CheckIfErrorOccurredInHelseId(NavigatingCancelEventArgs e)
        {
            var error = HttpUtility.ParseQueryString(e.Uri.Query).Get("errorId");

            return !error.IsNullOrEmpty();
        }

        private void HandleErrorInHelseId(NavigatingCancelEventArgs e)
        {
            var error = HttpUtility.ParseQueryString(e.Uri.Query).Get("errorId");
            OnLoginError?.Invoke(this, new LoginEventArgs()
            {
                IsError = true,
                Error = error
            });

            e.Cancel = true;
        }

    }
}
