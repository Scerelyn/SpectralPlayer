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
        public Brush SelectedBackgroundBrush { get; set; }
        public Brush SelectedForegroundBrush { get; set; }
        public string VisualizerChoice { get; set; }

        public VisualizerSettingsDialog(Brush fg, Brush bg, int selected=0, bool useDecibel=false)
        {
            InitializeComponent();
            // set defaults for convenience
            if (bg is SolidColorBrush)
            {
                BackgroundColor1Picker.SelectedColor = (bg as SolidColorBrush).Color;
                BackgroundColor2Picker.SelectedColor = (bg as SolidColorBrush).Color;
                UseBGGradientBrushCheckBox.IsChecked = false;
                BGColor2StackPanel.Visibility = Visibility.Collapsed;
            }
            else if(bg is LinearGradientBrush)
            {
                BackgroundColor1Picker.SelectedColor = (bg as GradientBrush).GradientStops[0].Color;
                BackgroundColor2Picker.SelectedColor = (bg as GradientBrush).GradientStops[1].Color;
                UseBGGradientBrushCheckBox.IsChecked = true;
                if ((bg as LinearGradientBrush).EndPoint.X == 0)
                {
                    UseBGVerticalGradientCheckBox.IsChecked = true;
                }
                BGColor2StackPanel.Visibility = Visibility.Visible;
            }
            if (fg is SolidColorBrush)
            {
                ForegroundColor1Picker.SelectedColor = (fg as SolidColorBrush).Color;
                ForegroundColor2Picker.SelectedColor = (fg as SolidColorBrush).Color;
                UseFGGradientBrushCheckBox.IsChecked = false;
                FGColor2StackPanel.Visibility = Visibility.Collapsed;
            }
            else if (fg is LinearGradientBrush)
            {
                ForegroundColor1Picker.SelectedColor = (fg as GradientBrush).GradientStops[0].Color;
                ForegroundColor2Picker.SelectedColor = (fg as GradientBrush).GradientStops[1].Color;
                UseFGGradientBrushCheckBox.IsChecked = true;
                if ((fg as LinearGradientBrush).EndPoint.X == 0)
                {
                    UseFGVerticalGradientCheckBox.IsChecked = true;
                }
                FGColor2StackPanel.Visibility = Visibility.Visible;
            }

            VisualizerSelectionComboBox.ItemsSource = new List<string>() { "Album Art", "Spectrum", "Peak Meter", "Spectrum Peak" };
            VisualizerSelectionComboBox.SelectedIndex = selected;
            DecibelScaleCheckBox.IsChecked = useDecibel;
            DoSelectionChanged(null, null);
        }

        public void DoCloseSave(object sender, RoutedEventArgs args)
        {
            if (UseBGGradientBrushCheckBox.IsChecked ?? false)
            {
                Color bg1 = (Color)BackgroundColor1Picker.SelectedColor;
                Color bg2 = (Color)BackgroundColor2Picker.SelectedColor;
                LinearGradientBrush gBGBrush = new LinearGradientBrush();
                if (UseBGVerticalGradientCheckBox.IsChecked ?? false)
                {
                    gBGBrush.StartPoint = new Point(0, 0);
                    gBGBrush.EndPoint = new Point(0, 1);
                }
                else
                {
                    gBGBrush.StartPoint = new Point(0, 0);
                    gBGBrush.EndPoint = new Point(1, 0);
                }
                gBGBrush.GradientStops.Add(new GradientStop(bg1, 0));
                gBGBrush.GradientStops.Add(new GradientStop(bg2, 0.75));
                SelectedBackgroundBrush = gBGBrush;
            }
            else
            {
                Color bg1 = (Color)BackgroundColor1Picker.SelectedColor;
                SelectedBackgroundBrush = new SolidColorBrush(bg1);
            }
            if (UseFGGradientBrushCheckBox.IsChecked ?? false)
            {
                Color fg1 = (Color)ForegroundColor1Picker.SelectedColor;
                Color fg2 = (Color)ForegroundColor2Picker.SelectedColor;
                LinearGradientBrush gFGBrush = new LinearGradientBrush();
                if (UseFGVerticalGradientCheckBox.IsChecked ?? false)
                {
                    gFGBrush.StartPoint = new Point(0, 0);
                    gFGBrush.EndPoint = new Point(0, 1);
                }
                else
                {
                    gFGBrush.StartPoint = new Point(0, 0);
                    gFGBrush.EndPoint = new Point(1, 0);
                }
                gFGBrush.GradientStops.Add(new GradientStop(fg1, 0));
                gFGBrush.GradientStops.Add(new GradientStop(fg2, 0.75));
                SelectedForegroundBrush = gFGBrush;
            }
            else
            {
                Color fg1 = (Color)ForegroundColor1Picker.SelectedColor;
                SelectedForegroundBrush = new SolidColorBrush(fg1);
            }

            VisualizerChoice = VisualizerSelectionComboBox.SelectedItem as string;
            DialogResult = true;
            Close();
        }

        public void DoCancel(object sender, RoutedEventArgs args)
        {
            Close();
        }

        public void DoFGCheckedChanged(object sender, RoutedEventArgs args)
        {
            if (UseFGGradientBrushCheckBox.IsChecked ?? false)
            {
                FGColor2StackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                FGColor2StackPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void DoBGCheckedChanged(object sender, RoutedEventArgs args)
        {
            if (UseBGGradientBrushCheckBox.IsChecked ?? false)
            {
                BGColor2StackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BGColor2StackPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void DoSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (VisualizerSelectionComboBox.SelectedItem as string == "Peak Meter")
            {
                BGColorStackPanel.Visibility = Visibility.Visible;
                FGColorStackPanel.Visibility = Visibility.Visible;
                UseBGGradientBrushCheckBox.Visibility = Visibility.Visible;
                UseFGGradientBrushCheckBox.Visibility = Visibility.Visible;
                DecibelScaleCheckBox.Visibility = Visibility.Hidden;
            }
            else if (VisualizerSelectionComboBox.SelectedItem as string == "Spectrum" || VisualizerSelectionComboBox.SelectedItem as string == "Spectrum Peak")
            {
                BGColorStackPanel.Visibility = Visibility.Visible;
                FGColorStackPanel.Visibility = Visibility.Visible;
                UseBGGradientBrushCheckBox.Visibility = Visibility.Visible;
                UseFGGradientBrushCheckBox.Visibility = Visibility.Visible;
                DecibelScaleCheckBox.Visibility = Visibility.Visible;
            }
            else if(VisualizerSelectionComboBox.SelectedItem as string == "Album Art")
            {
                BGColorStackPanel.Visibility = Visibility.Hidden;
                FGColorStackPanel.Visibility = Visibility.Hidden;
                UseBGGradientBrushCheckBox.Visibility = Visibility.Hidden;
                UseFGGradientBrushCheckBox.Visibility = Visibility.Hidden;
                DecibelScaleCheckBox.Visibility = Visibility.Hidden;
            }
        }
    }
}
