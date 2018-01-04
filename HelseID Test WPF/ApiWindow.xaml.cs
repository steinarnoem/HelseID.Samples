using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for ApiWindow.xaml
    /// </summary>
    public partial class ApiWindow : Window
    {
        public string ApiAddress { get; set; }
        public ApiWindow()
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
    }
}
