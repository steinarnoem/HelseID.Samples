using System;
using System.Windows;
using HelseID.Test.WPF.Common.Controls;
using HelseID.Test.WPF.Common;
using HelseID.Test.WPF.WebBrowser.EventArgs;
using HelseID.Test.WPF.WebBrowser.Model;
using IdentityModel.OidcClient;

namespace HelseID.Test.WPF.WebBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoginWindow _login;
        private AuthInfo _result;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                var browserManager = new BrowserManager();
                browserManager.Initialize();
                SetDefaultClientConfiguration();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    @"Applikasjonen har ikke tilstrekkelige rettigheter til å skrive til registeret. Start på nytt i administratormodus dersom du vil at applikasjonen skal gjøre nødvendige innstillinger");
            }
        }

        private void SetDefaultClientConfiguration()
        {
            AuthorityTextBox.Text = DefaultClientConfigurationValues.DefaultAuthority;
            ClientIdTextBox.Text = DefaultClientConfigurationValues.DefaultClientId;
            ScopeTextBox.Text = DefaultClientConfigurationValues.DefaultScope;
            SecretTextBox.Text = DefaultClientConfigurationValues.DefaultSecret;
            RedirectUrlTextBox.Text = DefaultClientConfigurationValues.DefaultUri;
        }

        public OidcClientOptions GetClientConfiguration()
        {
            var authority = AuthorityTextBox.Text;
            var clientId = ClientIdTextBox.Text;
            var scope = ScopeTextBox.Text.Replace(Environment.NewLine, " ");
            var secret = SecretTextBox.Text;
            var options = new OidcClientOptions()
            {
                Authority = string.IsNullOrEmpty(authority) ? DefaultClientConfigurationValues.DefaultAuthority : authority,
                ClientId = string.IsNullOrEmpty(clientId) ? DefaultClientConfigurationValues.DefaultClientId : clientId,
                RedirectUri = DefaultClientConfigurationValues.DefaultUri,
                Scope = string.IsNullOrEmpty(scope) ? DefaultClientConfigurationValues.DefaultScope : scope,
                ClientSecret = string.IsNullOrEmpty(secret) ? DefaultClientConfigurationValues.DefaultSecret : secret
            };

            if (UseADFSCheckBox.IsChecked.HasValue && UseADFSCheckBox.IsChecked.Value)
            {
                options.Policy =
                    new Policy()
                    {
                        RequireAccessTokenHash = false //ADFS 2016 spesifikk kode - ikke krev hash for access_token
                    };
            }
            options.BrowserTimeout = TimeSpan.FromSeconds(5);

            return options;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {                                    
            var clientConfig = GetClientConfiguration();

            if (!NetworkHelper.StsIsAvailable(clientConfig.Authority))
            {
                MessageBox.Show("Kunne ikke nå adressen:" + clientConfig.Authority);
            }

            _login = new LoginWindow(clientConfig);
            
            _login.OnLoginSuccess += LoginSuccess;
            _login.OnLoginError += LoginError;

            _login.ShowDialog();
        }

        private void LoginError(object sender, LoginEventArgs e)
        {
            _result = null;
            AccessTokenClaimsTextBox.Dispatcher.Invoke(() => { AccessTokenClaimsTextBox.Text = string.Empty; });
            IdentityTokenClaimsTextBox.Dispatcher.Invoke(() => { IdentityTokenClaimsTextBox.Text = string.Empty; });

            _login.OnLoginSuccess -= LoginSuccess;
            _login.OnLoginError -= LoginError;

            _login.Close();
            this.Activate();
            MessageBox.Show("Login failed");
        }

        private void LoginSuccess(object sender, LoginEventArgs e)
        {
            _login.OnLoginSuccess -= LoginSuccess;
            _login.OnLoginError -= LoginError;
            _login.Close();

            this.Activate();

            _result = new AuthInfo()
            {
                AccessToken = e.AccessToken,
                IdentityToken = e.IdentityToken
            };

            ShowTokenContent(_result.AccessToken, _result.IdentityToken);

            MessageBox.Show("Login Succeeded");
        }

        private void ShowTokenContent(string accessToken, string identityToken)
        {
            var accessTokenAsText = accessToken.DecodeToken();
            var idTokenAsText = identityToken.DecodeToken();

            AccessTokenClaimsTextBox.Dispatcher.Invoke(() => { AccessTokenClaimsTextBox.Text = accessTokenAsText; });
            IdentityTokenClaimsTextBox.Dispatcher.Invoke(() => { IdentityTokenClaimsTextBox.Text = idTokenAsText; });
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowIdTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_result != null && _result.IdentityToken.IsNotNullOrEmpty())
                ShowTokenViewer(_result.IdentityToken);
            else
                MessageBox.Show("Id Token er ikke tilgjengelig - prøv å logg inn på nytt");
        }

        private void ShowAccessTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_result != null && _result.AccessToken.IsNotNullOrEmpty())
                ShowTokenViewer(_result.AccessToken);
            else
                MessageBox.Show("Access Token er ikke tilgjengelig - prøv å logg inn på nytt");
        }

        private static void ShowTokenViewer(string content)
        {
            var view = new TokenViewerWindow { Token = content };
            view.ShowDialog();
        }
    }
}
