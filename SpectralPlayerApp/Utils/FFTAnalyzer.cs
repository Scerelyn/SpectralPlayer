using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralPlayerApp.Utils
{
    public class FFTAnalyzer : ISampleProvider
    {
        /// <summary>
        /// An ISampleProvider to obtain samples from
        /// </summary>
        public ISampleProvider Samples { get; private set; }

        /// <summary>
        /// The raw complex number data that acts as a buffer between FFT steps
        /// </summary>
        private Complex[] Data { get; set; }

        /// <summary>
        /// The FFT transformed float sample array
        /// </summary>
        private float[] Transformed { get; set; }

        /// <summary>
        /// The M value for FFT
        /// </summary>
        private int M { get; set; }

        /// <summary>
        /// The number of channels from the ISampleProvider
        /// </summary>
        private int Channels { get; set; }

        /// <summary>
        /// The length of FFT frames in number of samples
        /// </summary>
        private int FFTLength { get; set; }

        /// <summary>
        /// Current write position for the Data array
        /// </summary>
        private int DataPos { get; set; } = 0;

        public event EventHandler<float[]> CalculationDone;

        public WaveFormat WaveFormat { get => Samples.WaveFormat; }

        public WaveStream WaveStream { get; set; }

        /// <summary>
        /// Creates a Fourier Fast Transform analyzer ISampleProvider
        /// </summary>
        /// <param name="samples">The ISampleProvider to use</param>
        /// <param name="fftLength">The length of the FFT grouping, should be a power of two, or left empty for a 1024 default</param>
        /// <param name="backingStream">The backing WaveStream</param>
        public FFTAnalyzer(ISampleProvider samples, int fftLength = 1024, WaveStream backingStream = null)
        {
            WaveStream = backingStream;
            Samples = samples;
            FFTLength = fftLength;
            Channels = Samples.WaveFormat.Channels;
            M = (int)Math.Log(fftLength, 2);
            Data = new Complex[fftLength];
            Transformed = new float[fftLength];
            CalculationDone += (s, f) => { }; //prevent the listener from being null, so nothing breaks when this isnt being used
        }

        /// <summary>
        /// Performs FFT on a single float array of samples, then adds it into the ConvertedSampleFrames list
        /// </summary>
        /// <param name="sample">The float sample to analyze</param>
        public void AnalyzeStep(float sample)
        {
            //add the sample as a complex
            Data[DataPos].X = (float)(sample * FastFourierTransform.HammingWindow(DataPos, FFTLength));
            Data[DataPos].Y = 0;
            DataPos++; // increment position of the data array
            if (DataPos >= Data.Length) //reached the end of the data grouping, so tranform the data
            {
                DataPos = 0; //reset
                FastFourierTransform.FFT(true, M, Data); //transform
                for(int j = 0; j < Data.Length; j++) //then write
                {
                    Transformed[j] = (float)Math.Sqrt((Data[j].X * Data[j].X) + (Data[j].Y * Data[j].Y));
                }
                //fire the event saying the transforming is done
                CalculationDone(this, Transformed);

            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Samples.Read(buffer, offset, count); //let the other sampleprovider to that work
            
            //analyze the floats that came in
            for(int i = 0; i < samplesRead; i++)
            {
                AnalyzeStep(buffer[i]);
            }

            //then give the samples read
            return samplesRead;
        }

    }
}
