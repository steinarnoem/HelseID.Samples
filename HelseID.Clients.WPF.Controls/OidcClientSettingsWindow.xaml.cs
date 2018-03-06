using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HelseID.Common.ClientConfig;
using IdentityModel.OidcClient;
using System.Linq;
using System.Collections.ObjectModel;
using HelseID.Common.Clients;

namespace HelseID.Clients.WPF.Controls
{
    public class OidcOptionsChangedEventArgs : EventArgs
    {
        public HelseIdClientOptions Options { get; set; }
        public List<string> Scopes { get; set; }
        public string Name { get; set; }

        public OidcOptionsChangedEventArgs(HelseIdClientOptions options, List<string> configuredScopes, string name)
        {
            Options = options;
            Scopes = configuredScopes;
            Name = name;
        }
    }

    /// <summary>
    /// Interaction logic for OidcClientSettingsWindow.xaml
    /// </summary>
    public partial class OidcClientSettingsWindow : Window
    {        
        public event EventHandler<OidcOptionsChangedEventArgs> OptionsChanged;

        private List<string> ConfiguredScopes { get; set; }
        private HelseIdClientOptions _options;
        private ObservableCollection<Setting> Settings;

        public OidcClientSettingsWindow(string selected)
        {
            InitializeComponent();
            ConfiguredScopes = new List<string>();
            Settings = new ObservableCollection<Setting>(Config.Settings());
            PresetComboBox.ItemsSource = Settings;
            PresetComboBox.SelectedItem = Settings.FirstOrDefault(x => x.Name == selected) ?? Settings.FirstOrDefault();
            SetDefaultClientConfiguration();
            CreateScopesCheckboxes();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
           
        }

        private void SetDefaultClientConfiguration()
        {
            AuthoritiesComboBox.ItemsSource = Authorities.HelseIdAuthorities;
        }

        private void SetSetting(Setting setting)
        {
            SetDefaultClientConfiguration();
            NameTextBox.Text = setting.Name;
            ClientIdTextBox.Text = setting.ClientId;
            ConfiguredScopes = setting.Scopes;
            SecretTextBox.Text = setting.ClientSecret;
            RedirectUrlTextBox.Text = setting.RedirectUrl;
            AuthoritiesComboBox.SelectedItem = setting.Authority;
            PreselectIdpTextBox.Text = setting.PreselectedIdp;

            CreateScopesCheckboxes();
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
            ScopesList.Children.Clear();

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

        public void UpdateSelected(Setting selectedItem)
        {
            if (selectedItem == null)
                return;
            var authority = AuthoritiesComboBox.SelectedValue as string;
            var clientId = ClientIdTextBox.Text.Trim();
            var scope = string.Join(" ", ConfiguredScopes);
            var secret = SecretTextBox.Text;
            var redirectUrl = RedirectUrlTextBox.Text;
            var preselect = PreselectIdpTextBox.Text;

            selectedItem.Name = NameTextBox.Text;
            selectedItem.Authority = authority;
            selectedItem.ClientId = clientId;
            selectedItem.ClientSecret = secret;
            selectedItem.Scopes = ConfiguredScopes;
            selectedItem.RedirectUrl = redirectUrl;
            selectedItem.PreselectedIdp = preselect;
        }

        public HelseIdClientOptions GetClientConfiguration()
        {
            var authority = AuthoritiesComboBox.SelectedValue as string;
            var clientId = ClientIdTextBox.Text.Trim();
            var scope = string.Join(" ", ConfiguredScopes);
            var secret = SecretTextBox.Text;
            var redirectUrl = RedirectUrlTextBox.Text;
            var preselectIdP = PreselectIdpTextBox.Text;

            var options = new HelseIdClientOptions()
            {
                Authority = authority,
                ClientId = clientId,
                RedirectUri =  redirectUrl,
                Scope = scope,
                ClientSecret = secret,
                PreselectIdp = preselectIdP  
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
            UpdateSelected(PresetComboBox.SelectedItem as Setting);
            Config.Save(Settings.ToList());
            _options = GetClientConfiguration();
          
            var name = NameTextBox.Text;

            OptionsChanged?.Invoke(this, new OidcOptionsChangedEventArgs(_options, ConfiguredScopes, name));

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PresetComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            if(e.RemovedItems.Count > 0)
                UpdateSelected(e.RemovedItems[0] as Setting);
            if (e.AddedItems.Count > 0)
                SetSetting(e.AddedItems[0] as Setting);
            else if (Settings.Any())
            {
                SetSetting(Settings.First());
            }
            else
            {
                NewButton_Click(null, null);
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var newSetting = new Setting();
            Settings.Add(newSetting);

            PresetComboBox.SelectedItem = newSetting;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = Settings.FirstOrDefault(x => x.Name == NameTextBox.Text);
            if(selected != null)
                Settings.Remove(selected);

            PresetComboBox.SelectedItem = Settings.First(); 
        }
    }
}
