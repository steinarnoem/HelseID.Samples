using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HelseID.Test.WPF.Common.Controls;
using HelseID.Test.WPF.Common;
using IdentityModel.OidcClient;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;

namespace HelseID.Test.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private readonly SystemBrowserManager _browserManager;
        private readonly RequestHandler _requestHandler;
        private LoginResult _loginResult;
        private OidcClientOptions _options;
        private List<string> _configuredScopes = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            _browserManager = new SystemBrowserManager();

            _requestHandler = new RequestHandler();            
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
            //var options = GetClientConfiguration();

            //if (!NetworkHelper.StsIsAvailable(options.Authority))
            //{
            //    MessageBox.Show("Kunne ikke nå adressen:" + options.Authority);
            //}               

            if (_options == null)
            {

                MessageBox.Show("You must create a client configuration before logging in..", "Missing client configuration");
                return;
            }

            var client = new OidcClient(_options);

            try
            {                                
                var state = await client.PrepareLoginAsync(GetExtraParameters());
                _browserManager.Start(state.StartUrl);

#pragma warning disable 4014
                _requestHandler.Start(client, state, OnLoginSuccess);
#pragma warning restore 4014
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

        private async void OnLoginSuccess(string formData, OidcClient client, AuthorizeState state)
        {
            object extraParams = null;

            Dispatcher.Invoke(() =>
            {
                if (UseJwtBearerClientAuthentication.IsChecked.HasValue && UseJwtBearerClientAuthentication.IsChecked.Value)
                    extraParams = GetClientAssertionParameters(client.Options);
            });
            
            var result = await client.ProcessResponseAsync(formData, state, extraParams);

            HandleLoginResult(result);

            Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow == null) return;

                Application.Current.MainWindow.Activate();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            });
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

        public object GetExtraParameters()
        {
            var preselectIdp = (string)PreselectIdpLabel.Content;

            if (string.IsNullOrEmpty(preselectIdp))
                return null;


            return new { acr_values = preselectIdp, prompt = "Login" };
        }

        public object GetClientAssertionParameters(OidcClientOptions clientOptions)
        {
            var assertion = JwtGenerator.Generate(clientOptions);

            return new { client_assertion = assertion, client_assertion_type = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer };
        }


        private void ShowTokenContent(string accessToken, string identityToken)
        {
            var accessTokenAsText = accessToken.DecodeToken();
            var idTokenAsText = identityToken.DecodeToken();

            AccessTokenClaimsTextBox.Dispatcher.Invoke(() => { AccessTokenClaimsTextBox.Text = accessTokenAsText; });
            IdentityTokenClaimsTextBox.Dispatcher.Invoke(() => { IdentityTokenClaimsTextBox.Text = idTokenAsText; });
        }

        private void ShowIdTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult != null && _loginResult.IdentityToken.IsNotNullOrEmpty())
                ShowTokenViewer(_loginResult.IdentityToken);
            else
                MessageBox.Show("There is no Id Token available - try to log in");
        }

        private void ShowAccessTokenRawButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginResult != null && _loginResult.AccessToken.IsNotNullOrEmpty())
                ShowTokenViewer(_loginResult.AccessToken);
            else
                MessageBox.Show("There is no Access Token available - try to log in");
        }

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

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {            
            MessageBox.Show("Close your browser window to log out ;)");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateScopesList();
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

        private void DialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {

        }

        private void DialogHost_DialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {

        }
    }
}
