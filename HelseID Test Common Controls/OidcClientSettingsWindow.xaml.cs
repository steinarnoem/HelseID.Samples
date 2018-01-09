using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using IdentityModel.OidcClient;

namespace HelseID.Test.WPF.Common.Controls
{
    /// <summary>
    /// Interaction logic for OidcClientSettingsWindow.xaml
    /// </summary>
    public partial class OidcClientSettingsWindow : Window
    {
        private readonly List<string> _configuredScopes = new List<string>();
        private OidcClientOptions _options;

        public OidcClientSettingsWindow()
        {
            InitializeComponent();
        }

        public OidcClientSettingsWindow(List<string> scopes)
        {
            InitializeComponent();
            _configuredScopes = scopes;
        }

        public OidcClientSettingsWindow(List<string> scopes, OidcClientOptions options)
        {
            InitializeComponent();
            _configuredScopes = scopes;
            _options = options;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetDefaultClientConfiguration();
            SetDefaultScopes();
        }

        private void SetDefaultClientConfiguration()
        {
            ClientIdTextBox.Text = DefaultClientConfigurationValues.DefaultClientId;
            _configuredScopes.Add(DefaultClientConfigurationValues.DefaultScope);
            SecretTextBox.Text = DefaultClientConfigurationValues.DefaultSecret;
            RedirectUrlTextBox.Text = SystemBrowserRequestHandler.DefaultUri;
            AuthoritiesComboBox.SelectedItem = DefaultClientConfigurationValues.DefaultAuthority;
        }

        private void SetDefaultScopes()
        {
            foreach (var scope in Scopes.DefaultScopes)
            {
                var scopeCheckBox = new CheckBox()
                {
                    Content = scope
                };

                if (_configuredScopes.Contains(scope))
                    scopeCheckBox.IsChecked = true;

                scopeCheckBox.Unchecked += (checkbox, args) =>
                {
                    var box = checkbox as CheckBox;
                    if (box == null) return;

                    var s = box.Content as string;

                    if (!_configuredScopes.Contains(s)) return;

                    _configuredScopes.Remove(s);
                    //UpdateScopesTextBox();
                };

                scopeCheckBox.Checked += (checkbox, args) =>
                {

                    var box = checkbox as CheckBox;
                    if (box == null) return;

                    var s = box.Content as string;

                    if (_configuredScopes.Contains(s)) return;

                    _configuredScopes.Add(s);
                    //UpdateScopesTextBox();
                };

                ScopesList.Children.Add(scopeCheckBox);
            }

        }

        public OidcClientOptions GetClientConfiguration()
        {
            var authority = AuthoritiesComboBox.SelectedValue as string;
            var clientId = ClientIdTextBox.Text.Trim();
            var scope = string.Join(" ", _configuredScopes);
            var secret = SecretTextBox.Text;
            var options = new OidcClientOptions()
            {
                Authority = string.IsNullOrEmpty(authority) ? DefaultClientConfigurationValues.DefaultAuthority : authority,
                ClientId = string.IsNullOrEmpty(clientId) ? DefaultClientConfigurationValues.DefaultClientId : clientId,
                RedirectUri = SystemBrowserRequestHandler.DefaultUri,
                Scope = string.IsNullOrEmpty(scope) ? DefaultClientConfigurationValues.DefaultScope : scope,
                ClientSecret = secret //string.IsNullOrEmpty(secret) ? DefaultClientConfigurationValues.DefaultSecret : secret,
            };

            if (UseADFSCheckBox.IsChecked.HasValue && UseADFSCheckBox.IsChecked.Value)
            {
                options.Policy =
                    new Policy()
                    {
                        RequireAccessTokenHash = false, //ADFS 2016 spesific code - don't require hash for access_token                        
                    };
            }

            options.BrowserTimeout = TimeSpan.FromSeconds(5);

            options.Flow = OidcClientOptions.AuthenticationFlow.Hybrid;

            return options;
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


    }
}
