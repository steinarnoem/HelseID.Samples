using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using HelseID.Test.WPF.Common.Controls;
using HelseID.Test.WPF.Common;
using HelseID.Test.WPF.WebBrowser.EventArgs;
using HelseID.Test.WPF.WebBrowser.Model;
using IdentityModel.OidcClient;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;

namespace HelseID.Test.WPF.WebBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoginWindow _login;
        private AuthInfo _result;
        private LoginResult _loginResult;
        private OidcClientOptions _options;
        private List<string> _configuredScopes;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                var browserManager = new BrowserManager();
                browserManager.Initialize();
                //SetDefaultClientConfiguration();
                ShowApiLoadingDialog = false;

                try
                {
                    var rsaPublicKey = RSAKeyGenerator.GetPublicKeyAsXml();
                    RsaPublicKeyTextBox.Text = rsaPublicKey;
                }
                catch (Exception e)
                {
                    RsaPublicKeyTextBox.Text = "No RSA public key available";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    @"The application does not have sufficient priveleges to write to the registry. Try starting again in administrator modus if you would like the applicastion to do the neccessary configurations.");
            }
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

            if (_result == null)
            {
                return;
            }

            client.BaseAddress = url;

            client.SetBearerToken(_result.AccessToken);
            try
            {
                var result = await client.GetAsync(url);

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                var json = JArray.Parse(content).ToString();

                var viewer = new TextViewerWindow { Text = json };

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

        private void ConfigSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new OidcClientSettingsWindow(_configuredScopes, _options);
            settingsWindow.OptionsChanged += (s, updatedOptions) =>
            {
                _options = updatedOptions.Options;
                _configuredScopes = updatedOptions.Scopes;

                UpdateConfigurationView();
                UpdateScopesList();
            };
            var result = settingsWindow.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_options == null)
            {

                MessageBox.Show("You must create a client configuration before logging in..", "Missing client configuration");
                return;
            }

            //if (!NetworkHelper.StsIsAvailable(_options.Authority))
            //{
            //    MessageBox.Show("Could not reach the address:" + _options.Authority);
            //}

            _login = new LoginWindow(_options);

            _login.OnLoginSuccess += LoginSuccess;
            _login.OnLoginError += LoginError;

            _login.ShowDialog();
        }

        public bool ShowApiLoadingDialog
        {
            get;
            set;
        }

        private async void CallApiButton_Click(object sender, RoutedEventArgs e)
        {

            //var queue = new SnackbarMessageQueue(new TimeSpan(0,0,0,20));
            //MySnackbar.MessageQueue = queue;
            //queue.Enqueue("Wow, easy!");

            //await ApiLoadingDialog.ShowDialog(this);
            ShowApiLoadingDialog = true;
            

            if (_result == null)
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

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowIdTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_result != null && _result.IdentityToken.IsNotNullOrEmpty())
                ShowTokenViewer(_result.IdentityToken);
            else
                MessageBox.Show("The Id Token is not available - please authenticate again");
        }

        private void ShowAccessTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_result != null && _result.AccessToken.IsNotNullOrEmpty())
                ShowTokenViewer(_result.AccessToken);
            else
                MessageBox.Show("The Id Token is not available - please authenticate again");
        }

        private void GetRsaPublicKeyButton_Click(object sender, RoutedEventArgs e)
        {
            var rsaPublicKey = RSAKeyGenerator.CreateNewKey(false);
            RsaPublicKeyTextBox.Text = rsaPublicKey;
        }

        private static void ShowTokenViewer(string content)
        {
            var view = new TokenViewerWindow { Token = content };
            view.ShowDialog();
        }

        private void CopyRsaPublicKey_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RsaPublicKeyTextBox.Text);
            RsaPublicKeyTextBox.SelectAll();
            RsaPublicKeyTextBox.Focus();
        }
    }
}
