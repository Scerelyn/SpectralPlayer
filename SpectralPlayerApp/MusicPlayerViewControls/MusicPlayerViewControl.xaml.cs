using NAudio.Wave;
using NVorbis.Ogg;
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
using System.Windows.Threading;

namespace SpectralPlayerApp.MusicPlayerViewControls
{
    /// <summary>
    /// Interaction logic for MusicPlayerViewControl.xaml
    /// </summary>
    public partial class MusicPlayerViewControl : UserControl, INotifyPropertyChanged
    {
        private IWavePlayer player;
        private WaveStream inputStream;
        private DispatcherTimer timer = new DispatcherTimer(); // better than the normal timer bc no threading issues nor needing to use Dispatcher.Invoke

        private double _seekBarPos = 0;
        public double SeekBarPos
        {
            get => SeekBarPos;
            set
            {
                _seekBarPos = value;
                FieldChanged();
            }
        }

        public MusicPlayerViewControl()
        {
            InitializeComponent();
            player = new WaveOut();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += DoSeekBarUpdate;
            TimestampLabel.Content = "0:00";
            SeekSlider.IsEnabled = false;
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

                //set the seek bar 
                SeekSlider.Maximum = inputStream.TotalTime.TotalSeconds;
                SeekSlider.IsEnabled = true;

                //set volume
                player.Volume = (float)VolumeSlider.Value;

                //play
                player.Play();

                //handle onstop
                player.PlaybackStopped += (stoppedSender, stoppedArgs) => 
                {
                    inputStream.Close();
                    inputStream = null;
                    SeekBarPos = 0;
                    SeekSlider.Value = 0;
                    SeekSlider.IsEnabled = false;
                    timer.Stop();
                };

                //start the seekbar updater
                timer.Start();
            }
            else if (player.PlaybackState == PlaybackState.Paused)
            {
                timer.Start();
                player.Play();
            }
            else if (player.PlaybackState == PlaybackState.Playing)
            {
                timer.Stop();
                player.Pause();
            }
        }

        public void DoSeekBarUpdate(object sender, EventArgs args)
        {
            TimestampLabel.Content = inputStream?.CurrentTime.ToString(@"mm\:ss") ?? "0:00";
            SeekBarPos = inputStream.CurrentTime.TotalSeconds;
            SeekSlider.Value = inputStream.CurrentTime.TotalSeconds;
        }

        public void DoVolumeChange(object sender, RoutedEventArgs args)
        {
            player.Volume = (float)VolumeSlider.Value;
        }

        public void DoSeek(object sender, RoutedEventArgs args)
        {
            if (inputStream != null)
            {
                inputStream.CurrentTime = TimeSpan.FromSeconds(SeekSlider.Value);
            }
            TimestampLabel.Content = inputStream.CurrentTime.ToString(@"mm\:ss");
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
