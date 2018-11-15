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
    /// Interaction logic for SpectrumPeakViewControl.xaml
    /// </summary>
    public partial class SpectrumPeakViewControl : UserControl, INotifyPropertyChanged
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
        private Brush barBrush = Brushes.Black;

        public int BarsToUse { get; set; } = 30;

        public Brush BackgroundBrush
        {
            get { return backgroundBrush; }
            set { backgroundBrush = value; FieldChanged(); }
        }
        public Brush BarBrush
        {
            get { return barBrush; }
            set { barBrush = value; FieldChanged(); }
        }

        public bool UseDecibelScale { get; set; } = true;

        public SpectrumPeakViewControl()
        {
            InitializeComponent();
            VisualCanvas.DataContext = this;
        }

        private void OnCalculatedFinished(object sender, float[] e)
        {
            if (VisualCanvas.Children.Count > BarsToUse) //if the BarsToUse ever changes and its new value is less than the previous, clear it to avoid extra bars from showing
            {
                VisualCanvas.Children.Clear();
            }
            int groupingSize = (int)(e.Length * ReductionRatio) / BarsToUse; //size of the sample groupings
            int groupingCount = 0; //size of currently calculated grouping of samples
            double groupingMax = double.MinValue; //max value of the grouping
            int groupingIndex = 0; //the "index" of the grouping
            for(int i = 0; i < groupingSize * BarsToUse; i++) // not using length since extra samples might not be enough for the last grouping, so just ignore those and keep it even and nice
            {
                double plotY = 
                    UseDecibelScale
                        ? -20 * Math.Log10(e[i]) //decibel scale
                        : VisualCanvas.ActualHeight - (50000 * e[i]); //linear scale
                plotY = plotY > VisualCanvas.ActualHeight ? VisualCanvas.ActualHeight : plotY; // limit the max value to avoid infinite values
                plotY = plotY < 0 ? 0 : plotY; // and avoid negatives
                groupingMax = Math.Max(groupingMax, plotY); //find max
                groupingCount++;
                if (groupingCount >= groupingSize) // if the grouping is at max size
                {
                    //add/alter drawn rectangle
                    PointCollection pc = new PointCollection();
                    pc.Add(new Point(groupingIndex     * VisualCanvas.ActualWidth / BarsToUse, VisualCanvas.ActualHeight)); //bottom left point
                    pc.Add(new Point((groupingIndex+1) * VisualCanvas.ActualWidth / BarsToUse, VisualCanvas.ActualHeight)); //bottom right point
                    pc.Add(new Point((groupingIndex+1) * VisualCanvas.ActualWidth / BarsToUse, groupingMax)); //top right point
                    pc.Add(new Point(groupingIndex     * VisualCanvas.ActualWidth / BarsToUse, groupingMax)); //top left point
                    pc.Add(new Point(groupingIndex     * VisualCanvas.ActualWidth / BarsToUse, VisualCanvas.ActualHeight)); //back to bottom left point to close the polygon
                    if (VisualCanvas.Children.Count <= groupingIndex) //if the children dont have a replacable polygon, add it
                    {
                        Polygon rect = new Polygon(); //use a polygon for more control over the shape, rectangles dont have positions as properties
                        rect.Points = pc; //set its points
                        rect.DataContext = this;
                        rect.SetBinding(Polygon.FillProperty, new Binding("BarBrush"));
                        VisualCanvas.Children.Add(rect); //add
                    }
                    else //else just replace its points rather than making a new polygon. Something something performance reasons, ideally
                    {
                        (VisualCanvas.Children[groupingIndex] as Polygon).Points = pc;
                    }
                    groupingCount = 0; //reset counter
                    groupingMax = double.MinValue; //reset max
                    groupingIndex++; //move onto the next polygon/grouping

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
