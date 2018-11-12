using SpectralPlayerApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpectralPlayerApp.MusicPlayerViewControls
{
    /// <summary>
    /// Interaction logic for PeakMeterViewControl.xaml
    /// </summary>
    public partial class PeakMeterViewControl : UserControl, INotifyPropertyChanged
    {

        private Brush canvasBrush = Brushes.White;
        private Brush meterBrush = Brushes.Black;
        public Brush CanvasBrush
        {
            get { return canvasBrush; }
            set { canvasBrush = value; FieldChanged(); }
        }
        public Brush MeterBrush
        {
            get { return meterBrush; }
            set { meterBrush = value; FieldChanged(); }
        }

        private VolumePeakAnalyzer peakAnalyzer;
        public VolumePeakAnalyzer PeakAnalyzer
        {
            get => peakAnalyzer;
            set
            {
                if (peakAnalyzer != null)
                {
                    peakAnalyzer.PeakFindingDone -= OnPeakDone; //remove old listener
                }
                peakAnalyzer = value; //change
                if (value != null)
                {
                    peakAnalyzer.PeakFindingDone += OnPeakDone; //readd listener
                }
            }
        }

        public PeakMeterViewControl()
        {
            InitializeComponent();
            VisualCanvas.DataContext = this;
            Rectangle1.DataContext = this;
            Rectangle2.DataContext = this;
        }

        public void OnPeakDone(object sender, (float, float) maxes)
        {
            if (maxes.Item2 < 0) // one channel
            {
                double max = maxes.Item1 * 200;
                Rectangle1.Points.Clear();
                Rectangle1.Points.Add(new Point(0, 0));
                Rectangle1.Points.Add(new Point(max, 0));
                Rectangle1.Points.Add(new Point(max, VisualCanvas.ActualHeight));
                Rectangle1.Points.Add(new Point(0, VisualCanvas.ActualHeight));
                Rectangle1.Points.Add(new Point(0, 0));
                
            }
            else // two channels
            {
                double max1 = 200 * maxes.Item1;//maxes.Item1 != 0 ? 100 * -Math.Log10(maxes.Item1) : 0;
                double max2 = 200 * maxes.Item2;//maxes.Item2 != 0 ? 100 * -Math.Log10(maxes.Item2) : 0;
                Rectangle1.Points.Clear();
                Rectangle1.Points.Add(new Point(0, 0));
                Rectangle1.Points.Add(new Point(max1, 0));
                Rectangle1.Points.Add(new Point(max1, VisualCanvas.ActualHeight / 2));
                Rectangle1.Points.Add(new Point(0, VisualCanvas.ActualHeight / 2));
                Rectangle1.Points.Add(new Point(0, 0));

                Rectangle2.Points.Clear();
                Rectangle2.Points.Add(new Point(0, VisualCanvas.ActualHeight / 2));
                Rectangle2.Points.Add(new Point(max2, VisualCanvas.ActualHeight / 2));
                Rectangle2.Points.Add(new Point(max2, VisualCanvas.ActualHeight));
                Rectangle2.Points.Add(new Point(0, VisualCanvas.ActualHeight));
                Rectangle2.Points.Add(new Point(0, VisualCanvas.ActualHeight / 2));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void FieldChanged(string field = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(field));
            }
        }
    }
}
