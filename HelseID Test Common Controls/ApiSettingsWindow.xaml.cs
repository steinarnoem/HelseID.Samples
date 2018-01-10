using System.Windows;

namespace HelseID.Test.WPF.Common.Controls
{
    /// <summary>
    /// Interaction logic for ApiWindow.xaml
    /// </summary>
    public partial class ApiSettingsWindow : Window
    {
        public string ApiAddress { get; set; }
        public ApiSettingsWindow()
        {
            InitializeComponent();
        }
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ApiAddress = ApiAddressTextBox.Text;
            DialogResult = true;
            Close();            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ApiAddress = string.Empty;
            DialogResult = false;
            Close();
        }

        private void ApiAddressTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
