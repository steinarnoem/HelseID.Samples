using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using IdentityModel.OidcClient;

namespace HelseID.Test.WPF.Common.Controls
{

    public class OidcOptionsChangedEventArgs : EventArgs
    {
        public OidcClientOptions Options { get; set; }
        public List<string> Scopes { get; set; }

        public OidcOptionsChangedEventArgs(OidcClientOptions options, List<string> configuredScopes)
        {
            Options = options;
            Scopes = configuredScopes;
        }
    }

    /// <summary>
    /// Interaction logic for OidcClientSettingsWindow.xaml
    /// </summary>
    public partial class OidcClientSettingsWindow : Window
    {        
        public event EventHandler<OidcOptionsChangedEventArgs> OptionsChanged;

        private List<string> ConfiguredScopes { get; set; }
        private OidcClientOptions _options;

        public OidcClientSettingsWindow()
        {
            InitializeComponent();

            ConfiguredScopes = new List<string>();

            SetDefaultClientConfiguration();
            CreateScopesCheckboxes();
        }

        public OidcClientSettingsWindow(IReadOnlyCollection<string> scopes)
        {
            InitializeComponent();
            ConfiguredScopes = scopes != null ? new List<string>(scopes) : new List<string>();

            SetDefaultClientConfiguration();
            CreateScopesCheckboxes();
        }

        public OidcClientSettingsWindow(List<string> scopes, OidcClientOptions options)
        {
            InitializeComponent();

            ConfiguredScopes = scopes != null ? new List<string>(scopes) : new List<string>();

            if (options != null)
            {
                _options = options;
                UpdateClientSettingControls();
            }
            else
            {
                SetDefaultClientConfiguration();
            }

            CreateScopesCheckboxes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
           
        }

        private void SetDefaultClientConfiguration()
        {
            AuthoritiesComboBox.ItemsSource = Authorities.HelseIdAuthorities;
            ClientIdTextBox.Text = DefaultClientConfigurationValues.DefaultClientId;
            ConfiguredScopes.Add(DefaultClientConfigurationValues.DefaultScope);
            SecretTextBox.Text = DefaultClientConfigurationValues.DefaultSecret;
            RedirectUrlTextBox.Text = SystemBrowserRequestHandler.DefaultUri;
            AuthoritiesComboBox.SelectedItem = DefaultClientConfigurationValues.DefaultAuthority;
            
        }

        private void UpdateClientSettingControls()
        {
            AuthoritiesComboBox.ItemsSource = Authorities.HelseIdAuthorities;
            AuthoritiesComboBox.SelectedItem = _options.Authority;

            ClientIdTextBox.Text = _options.ClientId;
            
            SecretTextBox.Text = _options.ClientSecret;
            RedirectUrlTextBox.Text = _options.RedirectUri;
        }

        private void CreateScopesCheckboxes()
        {
            foreach (var scope in Scopes.DefaultScopes)
            {
                var scopeCheckBox = new CheckBox()
                {
                    Content = scope.Replace("_", "__")
                };

                if (ConfiguredScopes.Contains(scope))
                    scopeCheckBox.IsChecked = true;

                scopeCheckBox.Unchecked += (checkbox, args) =>
                {
                    var box = checkbox as CheckBox;

                    var s = box?.Content as string;

                    if (s == null) return;

                    var sanitizedScope = s.Replace("__", "_");

                    if (!ConfiguredScopes.Contains(sanitizedScope)) return;

                    ConfiguredScopes.Remove(sanitizedScope);
                };

                scopeCheckBox.Checked += (checkbox, args) =>
                {
                    var box = checkbox as CheckBox;

                    var s = box?.Content as string;

                    if (s == null) return;

                    var sanitizedScope = s.Replace("__", "_");

                    if (ConfiguredScopes.Contains(sanitizedScope)) return;

                    ConfiguredScopes.Add(sanitizedScope);
                };

                ScopesList.Children.Add(scopeCheckBox);
            }

        }

        public OidcClientOptions GetClientConfiguration()
        {
            var authority = AuthoritiesComboBox.SelectedValue as string;
            var clientId = ClientIdTextBox.Text.Trim();
            var scope = string.Join(" ", ConfiguredScopes);
            var secret = SecretTextBox.Text;
            var redirectUrl = RedirectUrlTextBox.Text;

            var options = new OidcClientOptions()
            {
                Authority = authority.IsNullOrEmpty() ? DefaultClientConfigurationValues.DefaultAuthority : authority,
                ClientId = clientId.IsNullOrEmpty() ? DefaultClientConfigurationValues.DefaultClientId : clientId,
                RedirectUri = redirectUrl.IsNullOrEmpty() ? SystemBrowserRequestHandler.DefaultUri : redirectUrl,
                Scope = scope.IsNullOrEmpty() ? DefaultClientConfigurationValues.DefaultScope : scope,
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
            _options = GetClientConfiguration();
            OptionsChanged?.Invoke(this, new OidcOptionsChangedEventArgs(_options, ConfiguredScopes));

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


    }
}
