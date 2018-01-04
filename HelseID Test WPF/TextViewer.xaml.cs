using System.Windows;

namespace HelseID.Test.WPF
{
    /// <summary>
    /// Interaction logic for TextViewer.xaml
    /// </summary>
    public partial class TextViewer : Window
    {
        public string Text { get; set; }

        public TextViewer()
        {
            InitializeComponent();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox.Text = Text;
        }
    }
}
