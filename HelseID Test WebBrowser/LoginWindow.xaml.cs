using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;
using HelseID.Test.WPF.Common;
using HelseID.Test.WPF.WebBrowser.EventArgs;
using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace HelseID.Test.WPF.WebBrowser
{
    public delegate void LoginEventHandler(object sender, LoginEventArgs e);
    
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private OidcClient _client;

        private readonly OidcClientOptions _clientOptions;
        private AuthorizeState _state;

        public event LoginEventHandler OnLoginSuccess;
        public event LoginEventHandler OnLoginError;
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(OidcClientOptions options)
        {
            InitializeComponent();
            _clientOptions = options;
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
            var responseData = WebBrowserControlHelper.GetResponseDataFromFormPostPage(webBrowser);
            var result = await _client.ProcessResponseAsync(responseData, _state);            

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
