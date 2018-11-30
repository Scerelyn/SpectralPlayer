using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralPlayerApp.Utils
{
    /// <summary>
    /// A Sample Provider that allows for changing between stereo and mono channel conversions of the given sample provider stream by simply changing a bool
    /// </summary>
    public class MonoStereoSwapProvider : ISampleProvider
    {
        private ISampleProvider BackingSampleProvider { get; set; }
        private ISampleProvider MonoProvider { get; set; }
        private ISampleProvider StereoProvider { get; set; }
        /// <summary>
        /// Determines if this provider should use Mono (true), Stereo (false), or whatever the given stream is in (null)
        /// </summary>
        public bool? UseMono { get; set; } = null;
        public WaveFormat WaveFormat
        {
            get { return BackingSampleProvider.WaveFormat; }
        }

        public MonoStereoSwapProvider(ISampleProvider sampleProvider)
        {
            BackingSampleProvider = sampleProvider;
            MonoProvider = new MonoToStereoProvider16(sampleProvider.ToWaveProvider16()).ToSampleProvider();
            StereoProvider = new StereoToMonoProvider16(sampleProvider.ToWaveProvider16()).ToSampleProvider();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // Use mono and the backing stream is not in mono already
            // If the stream is already mono, dont waste time and cpu converting it again
            if (UseMono == true && BackingSampleProvider.WaveFormat.Channels != 1) 
            {
                return MonoProvider.Read(buffer, offset, count);
            }
            // Dont use mono and the backing stream is not in stereo already
            // Again, if the stream is already stereo, dont waste resources converting
            else if (UseMono == false && BackingSampleProvider.WaveFormat.Channels != 2) 
            {
                return StereoProvider.Read(buffer, offset, count);
            }
            else // Use the normal stream
            {
                return BackingSampleProvider.Read(buffer, offset, count);
            }
        }
    }
}
