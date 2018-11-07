using SpectralPlayerApp.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpectralPlayerApp.MusicPlayerViewControls
{
    /// <summary>
    /// Interaction logic for SpectrumAnalyzerViewControl.xaml
    /// </summary>
    public partial class SpectrumAnalyzerViewControl : UserControl
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

        public Brush BackgroundBrush { get; set; } = Brushes.WhiteSmoke;
        public Brush GraphLineBrush { get; set; } = Brushes.Black;

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
                double ratio = -20*Math.Log10(transformedData[i]);
                if (GraphLine.Points.Count <= i)
                {
                    GraphLine.Points.Add(new Point(x, ratio)); //add if nothing there
                }
                else
                {
                    GraphLine.Points[i] = new Point(x, ratio); //else just overwrite
                }
            }
        }
    }
}
