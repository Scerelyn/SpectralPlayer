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
    /// Interaction logic for VisualizerSettingsDialog.xaml
    /// </summary>
    public partial class VisualizerSettingsDialog : Window
    {
        public SolidColorBrush SelectedBackgroundBrush { get; set; }
        public SolidColorBrush SelectedForegroundBrush { get; set; }
        public string VisualizerChoice { get; set; }

        public VisualizerSettingsDialog(SolidColorBrush fg, SolidColorBrush bg)
        {
            InitializeComponent();
            //using reflection to get the list of brushes from the Brushes class
            var brushes = typeof(Brushes).GetProperties().Select(p => new NamedSolidColorBrush(){ Name = p.Name, Brush = p.GetValue(null) as SolidColorBrush }).ToList();
            BackgroundColorComboBox.ItemsSource = brushes;
            if (bg != null)
            {
                BackgroundColorComboBox.SelectedIndex = brushes.IndexOf(brushes.Where(a => a.Brush == bg).First());
            }
            ForegroundColorComboBox.ItemsSource = brushes;
            if (fg != null)
            {
                ForegroundColorComboBox.SelectedIndex = brushes.IndexOf(brushes.Where(a => a.Brush == fg).First());
            }

            VisualizerSelectionComboBox.ItemsSource = new List<string>() { "Album Art", "Spectrum", "Peak Meter", "Spectrum Peak" };
            VisualizerSelectionComboBox.SelectedIndex = 0;
        }

        public void DoCloseSave(object sender, RoutedEventArgs args)
        {
            SelectedBackgroundBrush = (BackgroundColorComboBox.SelectedItem as NamedSolidColorBrush).Brush;
            SelectedForegroundBrush = (ForegroundColorComboBox.SelectedItem as NamedSolidColorBrush).Brush;
            VisualizerChoice = VisualizerSelectionComboBox.SelectedItem as string;
            DialogResult = true;
            Close();
        }

        public void DoCancel(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }

    public class NamedSolidColorBrush
    {
        public SolidColorBrush Brush { get; set; }
        public string Name { get; set; }
    }
}
