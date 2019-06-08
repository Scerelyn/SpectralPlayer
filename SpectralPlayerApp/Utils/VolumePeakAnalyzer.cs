using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralPlayerApp.Utils
{
    public class VolumePeakAnalyzer : ISampleProvider
    {
        public WaveFormat WaveFormat { get => SampleProvider.WaveFormat; }

        /// <summary>
        /// The ISampleProvider to read samples from
        /// </summary>
        private ISampleProvider SampleProvider { get; set; }

        private int Channels { get; set; }

        /// <summary>
        /// The event handler that fires when the peaks are found per grouping
        /// </summary>
        public EventHandler<(float,float)> PeakFindingDone;

        /// <summary>
        /// The current max of the first channel float grouping
        /// </summary>
        private float Max1 { get; set; } = float.MinValue;

        /// <summary>
        /// The current max of thesecond channel float grouping
        /// </summary>
        private float Max2 { get; set; } = float.MinValue;

        /// <summary>
        /// The size of the groupings of float samples to get a max from
        /// </summary>
        private int PeakGrouping { get; set; }

        /// <summary>
        /// The current size of the peak grouping
        /// </summary>
        private int PeakGroupingCounter { get; set; } = 0;

        /// <summary>
        /// Enables and disables the analyzer
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Creates a VolumePeakAnalayzer instance to analyze for peaks while reading the samples provided
        /// </summary>
        /// <param name="sampleProvider">The sample provider to read and analyzer from</param>
        /// <param name="samplesPerPeakGrouping">The number of samples to read and get a peak from at a time</param>
        public VolumePeakAnalyzer(ISampleProvider sampleProvider, int samplesPerPeakGrouping)
        {
            SampleProvider = sampleProvider;
            PeakGrouping = samplesPerPeakGrouping;
            Channels = sampleProvider.WaveFormat.Channels;
            PeakFindingDone += (s, e) => { }; //avoid null events
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int readSamples = SampleProvider.Read(buffer, offset, count); // let the other sample provider to that work
            if (Enabled)
            {
                for (int i = 0; i < readSamples; i+=Channels)
                {
                    if (Channels == 2 && i < buffer.Length)
                    {
                        Max1 = Math.Max(Max1, buffer[i]);
                        Max2 = Math.Max(Max2, buffer[i+1]);
                        PeakGroupingCounter++;
                        if (PeakGroupingCounter >= PeakGrouping)
                        {
                            PeakGroupingCounter = 0;
                            PeakFindingDone(this, (Max1, Max2));
                            Max1 = float.MinValue;
                            Max2 = float.MinValue;
                        }
                    }
                    else
                    {
                        Max1 = Math.Max(Max1, buffer[i]);
                        PeakGroupingCounter++;
                        if (PeakGroupingCounter >= PeakGrouping)
                        {
                            PeakGroupingCounter = 0;
                            PeakFindingDone(this, (Max1, -1));
                            Max1 = float.MinValue;
                            Max2 = float.MinValue;
                        }
                    }
                }
            }
            return readSamples;
        }
    }
}
