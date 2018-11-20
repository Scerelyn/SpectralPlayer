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

namespace SpectralPlayerApp.Dialogs
{
    /// <summary>
    /// Interaction logic for SingleStringInputDialog.xaml
    /// </summary>
    public partial class SingleStringInputDialog : Window
    {
        public string InputValue { get; private set; }

        public SingleStringInputDialog(string prompt="Enter a value", string title="Enter a value")
        {
            InitializeComponent();
            PromptLabel.Content = prompt;
            Title = title;
        }

        public void DoSubmit(object sender, RoutedEventArgs args)
        {
            InputValue = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        public void DoCancel(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }
}
