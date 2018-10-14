using NAudio.Wave;
using NVorbis.Ogg;
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
    /// Interaction logic for MusicPlayerViewControl.xaml
    /// </summary>
    public partial class MusicPlayerViewControl : UserControl
    {
        private IWavePlayer player;
        private WaveStream inputStream;

        public MusicPlayerViewControl()
        {
            InitializeComponent();
            player = new WaveOut();
        }

        /// <summary>
        /// Event method for when the Play/Pause button is clicked
        /// </summary>
        public void DoPlayPauseButtonAction(object sender, RoutedEventArgs args)
        {
            if (inputStream == null)
            {
                //AudioFileReader for most, use the  vorbus for ogg
                //sample file
                NAudio.Vorbis.VorbisWaveReader fileStream = new NAudio.Vorbis.VorbisWaveReader("c:/temp/one_shortened.ogg");
                inputStream = fileStream;
                player.Init(inputStream);

                player.Volume = (float)VolumeSlider.Value;
                player.Play();
                player.PlaybackStopped += (stoppedSender, stoppedArgs) => 
                {
                    inputStream.Close();
                    inputStream = null;
                    MessageBox.Show(stoppedArgs.Exception == null ? "Song ended fine" : "Error: " + stoppedArgs.Exception.Message);
                };
            }
            else if (player.PlaybackState == PlaybackState.Paused)
            {
                player.Play();
            }
            else if (player.PlaybackState == PlaybackState.Playing)
            {
                player.Pause();
            }
        }

        public void DoVolumeChange(object sender, RoutedEventArgs args)
        {
            player.Volume = (float)VolumeSlider.Value;
        }

        public void DoSeek(object sender, RoutedEventArgs args)
        {

        }
        
    }
}
