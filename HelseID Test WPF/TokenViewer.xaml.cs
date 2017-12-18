using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HelseID.Test.WPF
{
    /// <summary>
    /// Interaction logic for TokenViewer.xaml
    /// </summary>
    public partial class TokenViewer : Window
    {
        public TokenViewer()
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
