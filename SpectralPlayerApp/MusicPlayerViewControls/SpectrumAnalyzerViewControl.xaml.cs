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
    /// Interaction logic for SpectrumAnalyzerViewControl.xaml
    /// </summary>
    public partial class SpectrumAnalyzerViewControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The percentage of the transformed samples to actually use. 
        /// Lower the ratio, lower the samples drawn onto the spectrum analyzer
        /// </summary>
        private readonly double ReductionRatio = 0.4;

        private FFTAnalyzer fftAnalyzer;
        public FFTAnalyzer FFTAnalyzer
        {
            get => fftAnalyzer;
            set
            {
                if (fftAnalyzer != null)
                {
                    fftAnalyzer.CalculationDone -= OnCalculatedFinished; //remove old listener
                }
                fftAnalyzer = value; //change
                if (value != null)
                {
                    fftAnalyzer.CalculationDone += OnCalculatedFinished; //readd listener
                }
            }
        }

        private Brush backgroundBrush = Brushes.White;
        private Brush graphLineBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return backgroundBrush; }
            set { backgroundBrush = value; FieldChanged(); }
        } 
        public Brush GraphLineBrush
        {
            get { return graphLineBrush; }
            set { graphLineBrush = value; FieldChanged(); }
        }

        public bool UseDecibelScale { get; set; } = false;

        public SpectrumAnalyzerViewControl()
        {
            InitializeComponent();
            VisualCanvas.DataContext = this;
            GraphLine.DataContext = this;
        }

        public void OnCalculatedFinished(object sender, float[] transformedData)
        {
            double xStep = VisualCanvas.ActualWidth / (transformedData.Length*ReductionRatio);
            double x = 0;
            for (int i = 0; i < transformedData.Length*ReductionRatio; i++, x+=xStep)
            {
                double plotY = 
                    UseDecibelScale 
                    ? -20 * Math.Log10(transformedData[i]) //decibel scale
                    : VisualCanvas.ActualHeight - (5000 * transformedData[i]); //linear scale
                plotY = plotY > VisualCanvas.ActualHeight ? VisualCanvas.ActualHeight : plotY; // limit the max value to avoid infinite values
                plotY = plotY < 0 ? 0 : plotY; // and avoid negatives
                if (GraphLine.Points.Count <= i)
                {
                    GraphLine.Points.Add(new Point(x, plotY)); //add if nothing there
                }
                else
                {
                    GraphLine.Points[i] = new Point(x, plotY); //else just overwrite
                }
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
