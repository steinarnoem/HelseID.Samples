using System.Diagnostics;
using System.Windows;

namespace HelseID.Test.WPF.Common.Controls
{
    /// <summary>
    /// Interaction logic for TokenViewer.xaml
    /// </summary>
    public partial class TokenViewerWindow : Window
    {
        public TokenViewerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }        

        public string Token { get; set; }

        private void CopyTokenAndFocusButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TokenText.Text);
            TokenText.SelectAll();
            TokenText.Focus();

        }

        private void OpenJwtIoButton_Click(object sender, RoutedEventArgs e)
        {
            var token = TokenText.Text;
            TokenText.SelectAll();
            TokenText.Focus();

            Process.Start("https://jwt.io/?value=" + token);
        }
    }
}
