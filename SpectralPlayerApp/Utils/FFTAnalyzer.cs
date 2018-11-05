using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralPlayerApp.Utils
{
    public class FFTAnalyzer
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
        /// A list of float arrays that are the transformed buffers from the sample provider
        /// </summary>
        public List<float[]> ConvertedSampleFrames { get; private set; } = new List<float[]>();

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

        public FFTAnalyzer(ISampleProvider samples, int fftLength = 1024)
        {
            Samples = samples;
            FFTLength = fftLength;
            Channels = Samples.WaveFormat.Channels;
            M = (int)Math.Log(fftLength, 2);
            Data = new Complex[fftLength];
        }

        /// <summary>
        /// Reads all sample from Samples and creates a list of float arrays to transform
        /// </summary>
        /// <returns>A list of float arrays to be transformed via FFT</returns>
        public List<float[]> GetFrames()
        {
            List<float[]> frames = new List<float[]>();
            float[] buffer = new float[FFTLength];
            int samplesRead = Samples.Read(buffer, 0, FFTLength);
            while (samplesRead != 0)
            {
                frames.Add(buffer); //add to frame liist
                buffer = new float[FFTLength]; //new array to clear the buffer
                samplesRead = Samples.Read(buffer, 0, FFTLength); //read again
            }
            return frames;
        }

        /// <summary>
        /// Performs FFT on a single float array of samples, then adds it into the ConvertedSampleFrames list
        /// </summary>
        /// <param name="sampleFrame">The float array to transform viua FFT</param>
        public void AnalyzeStep(float[] sampleFrame)
        {
            float[] transformed = new float[sampleFrame.Length];
            int dataPos = 0;
            for(int i = 0; i < sampleFrame.Length; i++)
            {
                Data[dataPos].X = (float)(sampleFrame[i] * FastFourierTransform.HammingWindow(i, FFTLength));
                Data[dataPos].Y = 0;
                dataPos++;
                if (dataPos >= Data.Length) //reached the end of the data
                {
                    dataPos = 0; //reset
                    FastFourierTransform.FFT(true, M, Data); //transform
                    for(int j = 0; j < Data.Length; j++) //then write
                    {
                        transformed[j] = (float)Math.Sqrt((Data[j].X * Data[j].X) + (Data[j].Y * Data[j].Y));
                    }
                    ConvertedSampleFrames.Add(transformed);
                }
            }
        }
    }
}
