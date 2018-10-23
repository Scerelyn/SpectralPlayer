using MusicLibraryLib;
using NAudio.Flac;
using NAudio.Vorbis;
using NAudio.Wave;
using NVorbis.Ogg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using TagLib;

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
        private bool stopLock = false; // determines if a stop is deliberate, ie: to clear the audio buffer, or not
        private Song activeSong = null;

        private double _seekBarPos = 0;
        public double SeekBarPos
        {
            get => _seekBarPos;
            set
            {
                _seekBarPos = value;
                FieldChanged();
            }
        }

        /// <summary>
        /// The UpNext control to get the songs to play from
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }

        public MusicPlayerViewControl()
        {
            InitializeComponent();
            player = new WaveOut();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += DoSeekBarUpdate;
            TimestampLabel.Content = "00:00";
            SeekSlider.IsEnabled = false;
        }

        /// <summary>
        /// Event method for when the Play/Pause button is clicked
        /// </summary>
        public void DoPlayPauseButtonAction(object sender, RoutedEventArgs args)
        {
            if (inputStream == null && UpNextControl.UpNext.SongList.Count > 0) // first click on play button
            {
                SetupNextInputStream();
                if (inputStream == null) // if the SetupNextInputStream method fails due to an invalid song filepath, inputstream will be null
                {
                    return; // in that case, cutoff the method since nothing can be done with a null stream
                }
                player.Init(inputStream);

                SetupImageVisualizer();
                SetupNowPlayingLabel();
                //set the seek bar 
                SeekSlider.Maximum = inputStream.TotalTime.TotalSeconds;
                SeekSlider.IsEnabled = true;

                //set volume
                player.Volume = (float)VolumeSlider.Value;

                //play
                player.Play();

                //set the button's pause image
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };

                //handle onstop
                player.PlaybackStopped += (stoppedSender, stoppedArgs) =>
                {
                    if (!stopLock) // stop is called either by automatic end of stream, or some error
                    {
                        if (stoppedArgs.Exception == null) // end of song reached
                        {
                            timer.Stop();
                            inputStream.Close();
                            SetupNextInputStream();
                            SetupImageVisualizer();
                            SetupNowPlayingLabel();
                            if (inputStream != null) //not null means a song after the finished on exists
                            {
                                //set the seek bar 
                                SeekSlider.Maximum = inputStream.TotalTime.TotalSeconds;
                                SeekSlider.IsEnabled = true;
                                //setup the player
                                player.Init(inputStream);
                                player.Play();
                                timer.Start();
                            }
                        }
                        else //error occured, shut it all down
                        {
                            inputStream.Close();
                            inputStream = null;
                            SeekBarPos = 0;
                            SeekSlider.Value = 0;
                            SeekSlider.IsEnabled = false;
                            timer.Stop();
                        }
                    }
                    else // stop is called when seeking
                    {
                        stopLock = false;
                    }
                };

                //start the seekbar updater
                timer.Start();
            }
            else if (player.PlaybackState == PlaybackState.Paused)
            {
                timer.Start();
                player.Play();
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };
            }
            else if (player.PlaybackState == PlaybackState.Playing)
            {
                timer.Stop();
                player.Pause();
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/play.png")) };
            }
            else if (player.PlaybackState == PlaybackState.Stopped && inputStream != null) // hits here when the seekbar is moved when paused
            {
                timer.Start();
                player.Play();
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };
            }
        }

        private void SetupNextInputStream()
        {
            Song nextSong = null;
            //TODO: improve the failing song filepath behavior. Currently, the silent skipping isnt very user friendly in telling why
            do  // loop through songs until a valid/existing file is found. Ideally, this loops once
            {
                nextSong = UpNextControl.UpNext.PopNextSong();
                if (nextSong == null) // null means the upnext list is empty
                {
                    inputStream = null; // null the inputstream
                    return; //and just cutoff the method
                }
            }
            while (!System.IO.File.Exists(nextSong.FilePath));

            activeSong = nextSong;

            if (System.IO.File.Exists(nextSong.FilePath))
            {
                if (nextSong.FilePath.EndsWith(".ogg")) // use the vorbis library
                {
                    VorbisWaveReader fileStream = new VorbisWaveReader(nextSong.FilePath);
                    inputStream = fileStream;
                }
                else if (nextSong.FilePath.EndsWith(".flac")) // use the flac library
                {
                    FlacReader fileStream = new FlacReader(nextSong.FilePath);
                    inputStream = fileStream;
                }
                else // for anything else, presumably something that naudio supports
                {
                    AudioFileReader fileStream = new AudioFileReader(nextSong.FilePath);
                    inputStream = fileStream;
                }
            }
        }

        public void SetupImageVisualizer()
        {
            if (inputStream != null)
            {
                using (TagLib.File file = TagLib.File.Create(activeSong.FilePath))
                {
                    Tag tags = file.Tag;
                    if (tags.Pictures.Length > 0)
                    {
                        IPicture albumArt = tags.Pictures[0];
                        using (MemoryStream ms = new MemoryStream(albumArt.Data.Data))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();
                            image.Freeze();
                            ImageBrush ib = new ImageBrush(image);
                            //MessageBox.Show(image.PixelWidth + " by " + image.PixelHeight);
                            ImageHoldingLabel.Background = ib;
                            ib.Stretch = Stretch.Uniform;
                        }
                    }
                    else
                    {
                        ImageHoldingLabel.Background = Brushes.Azure;
                    }
                }
            }
        }

        public void SetupNowPlayingLabel()
        {
            NowPlayingLabel.Content = $"Now playing: {activeSong.Name} by {activeSong.Artist}";
        }

        public void DoSeekBarUpdate(object sender, EventArgs args)
        {
            TimestampLabel.Content = inputStream?.CurrentTime.ToString(@"mm\:ss") ?? "00:00";
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
                if (player.PlaybackState == PlaybackState.Paused)
                {
                    stopLock = true;
                    player.Stop(); // to flush the buffer, or else we get some of the song from before we moved playing
                }
            }
            TimestampLabel.Content = inputStream.CurrentTime.ToString(@"mm\:ss");
            timer.Start();
        }

        public void DoHoldTimer(object sender, MouseButtonEventArgs args)
        {
            timer.Stop();
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
