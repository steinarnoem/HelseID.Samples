using System.Windows;

namespace HelseID.Clients.WPF.Controls
{
    /// <summary>
    /// Interaction logic for TextViewer.xaml
    /// </summary>
    public partial class TextViewerWindow : Window
    {
        public string Text { get; set; }

        public TextViewerWindow()
        {
            InitializeComponent();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox.Text = Text;
        }
    }
}
