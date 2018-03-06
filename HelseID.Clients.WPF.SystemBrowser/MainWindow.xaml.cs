using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HelseID.Common.ClientConfig;
using HelseID.Common.Crypto;
using HelseID.Common.Extensions;
using HelseID.Common.Certificates;
using HelseID.Clients.WPF.Controls;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using HelseID.Common.Clients;

namespace HelseID.Test.WPF
{
    public partial class MainWindow : Window
    {        
        private LoginResult _loginResult;
        private string _selectedName;
        private string RsaPublicKey;
        private HelseIdClientOptions _options;
        private List<string> _configuredScopes = new List<string>();
        private TokenResponse _tokenExchangeResult;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                var rsaPublicKey = RSAKeyGenerator.GetPublicKeyAsXml();
                RsaPublicKey = rsaPublicKey;
            }
            catch (Exception)
            {
                RsaPublicKey = "No RSA public key available";
            }           
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Login();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
            }
        }

        private async Task Login()
        {
            if (_options == null)
            {
                MessageBox.Show("You must create a client configuration before logging in..", "Missing client configuration");
                return;
            }

            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (UseJwtBearerClientAuthenticationRSA.IsChecked.HasValue &&
                        UseJwtBearerClientAuthenticationRSA.IsChecked.Value)
                    {
                        _options.SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.RsaSecurityKey;
                    }
                    if (UseJwtBearerClientAuthenticationEntCert.IsChecked.HasValue &&
                      UseJwtBearerClientAuthenticationEntCert.IsChecked.Value)
                    {
                        _options.SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.X509EnterpriseSecurityKey;
                        _options.CertificateThumbprint = EnterpriseCertificateTextBox.Text;
                    }
                });

                var client = new HelseIdClient(_options);

                var result = await client.Login();

                HandleLoginResult(result);

                Dispatcher.Invoke(() =>
                {
                    if (Application.Current.MainWindow == null) return;

                    Application.Current.MainWindow.Activate();
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message, e.StackTrace);
                throw;
            }
        }


        public void DialogHostOpeningEventHandler(object sender, DialogOpenedEventArgs eventargs)
        {
            //Nothing
        }

        public void HandleLoginResult(LoginResult result)
        {
            if ((result == null) || (result.IsError))
            {
                if (result != null)
                    MessageBox.Show(result.Error);

                _loginResult = null;
                AccessTokenClaimsTextBox.Dispatcher.Invoke(() => { AccessTokenClaimsTextBox.Text = string.Empty; });
                IdentityTokenClaimsTextBox.Dispatcher.Invoke(() => { IdentityTokenClaimsTextBox.Text = string.Empty; });
                return;
            }            

            _loginResult = result;

            ShowTokenContent(result.AccessToken, result.IdentityToken);            
        }       

        private void ShowTokenContent(string accessToken, string identityToken)
        {
            var accessTokenAsText = accessToken.DecodeToken();
            var idTokenAsText = identityToken.DecodeToken();

            AccessTokenClaimsTextBox.Dispatcher.Invoke(() => { AccessTokenClaimsTextBox.Text = accessTokenAsText; });
            IdentityTokenClaimsTextBox.Dispatcher.Invoke(() => { IdentityTokenClaimsTextBox.Text = idTokenAsText; });
        }

        #region Event Handlers

        private void ShowIdTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult != null && _loginResult.IdentityToken.IsNotNullOrEmpty())
                ShowTokenViewer(_loginResult.IdentityToken);
            else
                MessageBox.Show("There is no Id Token available - try to log in");
        }

        private void ShowExchangedTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tokenExchangeResult != null && _tokenExchangeResult.AccessToken.IsNotNullOrEmpty())
                ShowTokenViewer(_tokenExchangeResult.AccessToken);
            else
                MessageBox.Show("There is no Exchanged Access Token Token available - try to run token exchange");
        }

        private void ShowAccessTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult != null && _loginResult.AccessToken.IsNotNullOrEmpty())
                ShowTokenViewer(_loginResult.AccessToken);
            else
                MessageBox.Show("There is no Access Token available - try to log in");
        }

        private async void TokenExchange_Click(object sender, RoutedEventArgs e)
        {

            if (UseJwtBearerClientAuthenticationRSA.IsChecked.HasValue &&
                   UseJwtBearerClientAuthenticationRSA.IsChecked.Value)
            {
                _options.SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.RsaSecurityKey;
            }
            if (UseJwtBearerClientAuthenticationEntCert.IsChecked.HasValue &&
              UseJwtBearerClientAuthenticationEntCert.IsChecked.Value)
            {
                _options.SigningMethod = Common.Jwt.JwtGenerator.SigningMethod.X509EnterpriseSecurityKey;
                _options.CertificateThumbprint = EnterpriseCertificateTextBox.Text;
            }

            _options.Scope = "nhn/helseid.test.api.fullframework";
            var client = new HelseIdClient(_options);
            var response = await client.TokenExchange(_loginResult.AccessToken);
           
            _tokenExchangeResult = response;
            var tokenAsText = response.AccessToken.DecodeToken();

            ExchangeTokenClaimsTextBox.Dispatcher.Invoke(() => { ExchangeTokenClaimsTextBox.Text = tokenAsText; });
        }

        private void ThumbprintCheck_Click(object sernder, RoutedEventArgs e)
        {
            try
            {
                var cert = CertificateStore.GetCertificateByThumbprint(EnterpriseCertificateTextBox.Text);
                MessageBox.Show(cert.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Close your browser window to log out ;)");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateScopesList();
        }

        private void CallApiButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult == null)
            {
                MessageBox.Show("You need to authenticate before you can call the API");
                return;
            }

            var apiWindow = new ApiSettingsWindow();
            var result = apiWindow.ShowDialog();

            if (!result.HasValue || !result.Value) return;

            var apiUrl = apiWindow.ApiAddress;

            try
            {
                var url = new Uri(apiUrl);

                if (!url.IsWellFormedOriginalString())
                    throw new UriFormatException();

                CallApi(url);
            }
            catch (UriFormatException uriFormatException)
            {
                MessageBox.Show($"The Url you entered was invalid : {uriFormatException.Message}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        private void ConfigSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new OidcClientSettingsWindow(_selectedName);
            settingsWindow.OptionsChanged += (s, updatedOptions) =>
            {
                _selectedName = updatedOptions.Name;
                _options = updatedOptions.Options;
                _configuredScopes = updatedOptions.Scopes;

                UpdateConfigurationView();
                UpdateScopesList();
                PreselectIdpLabel.Content = updatedOptions.Options.PreselectIdp.Trim();
            };
            var result = settingsWindow.ShowDialog();
        }


        private void GetRsaPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            var rsaPublicKey = RSAKeyGenerator.CreateNewKey(false);
            RsaPublicKey = rsaPublicKey;
        }

        private void CopyRsaPublicKey_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RsaPublicKey);
        }
        #endregion

        private static void ShowTokenViewer(string content)
        {
            var view = new TokenViewerWindow { Token = content };
            view.ShowDialog();
        }

        private void UpdateConfigurationView()
        {
            ClientIdLabel.Content = _options.ClientId;
            SecretLabel.Content = _options.ClientSecret;
            RedirectUrlLabel.Content = _options.RedirectUri;
            AuthoritiesLabel.Content = _options.Authority;
        }        


        private void UpdateScopesList()
        {
            ScopesList.Children.Clear();
            foreach (var scope in Scopes.DefaultScopes)
            {
                var scopeCheckBox = new CheckBox()
                {
                    Content = scope,
                };

                if (_configuredScopes.Contains(scope))
                    scopeCheckBox.IsChecked = true;

                scopeCheckBox.Unchecked += (checkbox, args) =>
                {
                    //Hack for å unngå at brukeren velger noe her
                    args.Handled = true;
                    UpdateScopesList();
                };

                scopeCheckBox.Checked += (checkbox, args) =>
                {
                    //Hack for å unngå at brukeren velger noe her
                    args.Handled = true;
                    UpdateScopesList();
                };

                ScopesList.Children.Add(scopeCheckBox);
            }

        }

        private async void CallApi(Uri url)
        {
            var client = new HttpClient();

            if (_loginResult == null)
            {                
                return;
            }

            client.BaseAddress = url;

            client.SetBearerToken(_loginResult.AccessToken);
            try
            {
                var result = await client.GetAsync(url);

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var json = JArray.Parse(content).ToString();

                var viewer = new TextViewerWindow {Text = json};

                viewer.ShowDialog();
            }
            catch (HttpRequestException requestException)
            {
                MessageBox.Show($"{requestException.Message}. The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.");
            }
            finally
            {
                client.Dispose();
            }
        }


    }
}
