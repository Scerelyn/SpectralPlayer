﻿using MusicLibraryLib;
using NAudio.Flac;
using NAudio.Vorbis;
using NAudio.Wave;
using NVorbis.Ogg;
using SpectralPlayerApp.Utils;
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
        #region Private fields

        private IWavePlayer player;
        private ISampleProvider playerInputStream; //the stream that the player will use
        private WaveStream backingWaveStream; //the backing wavestream that is the origin of the playerInputStream. Kept in order to allow for seeking and repositioning
        private DispatcherTimer timer = new DispatcherTimer(); // better than the normal timer bc no threading issues nor needing to use Dispatcher.Invoke
        private bool stopLock = false; // determines if a stop is deliberate, ie: to clear the audio buffer, or not
        private bool prevRecord = false; // determines if a stop should record the previously playing song into the history stack
        private Song activeSong = null;
        private Stack<Song> previousSongs = new Stack<Song>(); // holds a history of previously played songs
        private double _seekBarPos = 0;

        #endregion

        #region Public Properties

        public bool? UseStereo { get; set; } = null;

        public MainWindow ParentWindow { get; set; }

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
        
        #endregion

        public MusicPlayerViewControl()
        {
            InitializeComponent();
            player = new WaveOut();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += DoSeekBarUpdate;
            TimestampLabel.Content = "00:00";
            SeekSlider.IsEnabled = false;
            
            //handle onstop
            player.PlaybackStopped += (stoppedSender, stoppedArgs) =>
            {
                if (!stopLock) // stop is called either by automatic end of stream, or some error
                {
                    if (stoppedArgs.Exception == null) // end of song reached
                    {
                        timer.Stop();
                        backingWaveStream.Close();
                        
                        if(prevRecord)
                        {
                            previousSongs.Push(activeSong); // push just played song onto the history stack
                        }
                        else
                        {
                            prevRecord = true;
                        }
                        SetupNextInputStream();
                        SetupImageVisualizer();
                        SetupNowPlayingLabel();

                        //discord update
                        if (activeSong != null)
                        {
                            ParentWindow.SendDiscordRPCUpdate($"{activeSong.Name}", $"by {activeSong.Artist}", "listening");
                        }
                        else
                        {
                            ParentWindow.SendDiscordRPCUpdate("Sitting in silence", "and pondering life", "silence");
                        }

                        if (playerInputStream != null) //not null means a song after the finished on exists
                        {
                            //set the seek bar 
                            SeekSlider.Maximum = backingWaveStream.TotalTime.TotalSeconds;
                            
                            SeekSlider.IsEnabled = true;
                            //setup the player
                            player.Init(playerInputStream);
                            Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };
                            player.Play();
                            timer.Start();
                        }
                        else // null inputstream means we reached the end of upnext, so shut everything down
                        {
                            TimestampLabel.Content = "00:00";
                            SeekBarPos = 0;
                            SeekSlider.Value = 0;
                            SeekSlider.IsEnabled = false;
                            timer.Stop();
                            Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/play.png")) };
                        }
                    }
                    else //error occured, shut it all down
                    {
                        backingWaveStream.Close();
                        playerInputStream = null;
                        SeekBarPos = 0;
                        SeekSlider.Value = 0;
                        SeekSlider.IsEnabled = false;
                        timer.Stop();
                        ParentWindow.SendDiscordRPCUpdate("Sitting in the main window","Enjoying the silence", "silence");
                    }
                }
                else // stop is called when seeking
                {
                    stopLock = false;
                }
            };
        }

        #region Music Player setup methods

        /// <summary>
        /// Setups up the next input stream for the IWavePlayer instance by popping the next song in the upnext queue and grabbing its audio file stream
        /// </summary>
        private void SetupNextInputStream()
        {
            Song nextSong = null;
            //TODO: improve the failing song filepath behavior. Currently, the silent skipping isnt very user friendly in telling why
            do  // loop through songs until a valid/existing file is found. Ideally, this loops once
            {
                nextSong = UpNextControl.GetNextSong();
                if (nextSong == null) // null means the upnext list is empty
                {
                    playerInputStream = null; // null the inputstream
                    activeSong = null;
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
                    playerInputStream = fileStream;
                }
                else if (nextSong.FilePath.EndsWith(".flac")) // use the flac library
                {
                    FlacReader fileStream = new FlacReader(nextSong.FilePath);
                    playerInputStream = fileStream;
                }
                else // for anything else, presumably something that naudio supports
                {
                    AudioFileReader fileStream = new AudioFileReader(nextSong.FilePath);
                    playerInputStream = fileStream;
                }
                backingWaveStream = playerInputStream as WaveStream; // keep track of the old filestream, since its the only thing letting the app do seeking

                if (playerInputStream.WaveFormat.BitsPerSample == 16) // convert if needed
                {
                    Wave16ToFloatProvider w16ToFloat = new Wave16ToFloatProvider(playerInputStream as IWaveProvider);
                    playerInputStream = w16ToFloat.ToSampleProvider();
                }

                FFTAnalyzer fft = new FFTAnalyzer(playerInputStream, 1024, backingWaveStream);
                playerInputStream = fft;

                VolumePeakAnalyzer vpa = new VolumePeakAnalyzer(playerInputStream, 1024);
                playerInputStream = vpa;

                if (backingWaveStream.WaveFormat.Channels == 2 && UseStereo == false)
                {
                    StereoToMonoProvider16 stm16 = new StereoToMonoProvider16(playerInputStream.ToWaveProvider16());
                    playerInputStream = stm16.ToSampleProvider();
                }
                else if(backingWaveStream.WaveFormat.Channels == 1 && UseStereo == true)
                {
                    MonoToStereoProvider16 mts16 = new MonoToStereoProvider16(playerInputStream.ToWaveProvider16());
                    playerInputStream = mts16.ToSampleProvider();
                }
                SpectrumAnalyzer.FFTAnalyzer = fft;
                PeakMeterAnalyzer.PeakAnalyzer = vpa;
                SpectrumPeakAnalyzer.FFTAnalyzer = fft;
                if (SpectrumAnalyzer.Visibility == Visibility.Visible)
                {
                    SpectrumAnalyzer.FFTAnalyzer.Enabled = true;
                }
                else if (SpectrumPeakAnalyzer.Visibility == Visibility.Visible)
                {
                    SpectrumPeakAnalyzer.FFTAnalyzer.Enabled = true;
                }
                else if (PeakMeterAnalyzer.Visibility == Visibility.Visible)
                {
                    PeakMeterAnalyzer.PeakAnalyzer.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Sets up the player for the next song to be ready to play
        /// </summary>
        public void SetupNextSong()
        {
            if (playerInputStream == null && UpNextControl.UpNext.SongList.Count > 0) // first click on play button
            {
                SetupNextInputStream();
                if (playerInputStream == null) // if the SetupNextInputStream method fails due to an invalid song filepath, inputstream will be null
                {
                    return; // in that case, cutoff the method since nothing can be done with a null stream
                }
                player.Init(playerInputStream);

                SetupImageVisualizer();
                SetupNowPlayingLabel();

                //discord update
                if (activeSong != null)
                {
                    ParentWindow.SendDiscordRPCUpdate($"{activeSong.Name}", $"by {activeSong.Artist}", "listening");
                }
                else
                {
                    ParentWindow.SendDiscordRPCUpdate("Sitting in silence", "and pondering life", "silence");
                }

                //set the seek bar 
                SeekSlider.Maximum = backingWaveStream.TotalTime.TotalSeconds;
                SeekSlider.IsEnabled = true;

                //set volume
                player.Volume = (float)VolumeSlider.Value;
            }
        }

        /// <summary>
        /// Sets up the image visualizer for the music player UI
        /// </summary>
        public void SetupImageVisualizer()
        {
            if (activeSong != null)
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
            else
            {
                ImageHoldingLabel.Background = Brushes.Azure;
            }
        }

        /// <summary>
        /// Sets up the now playing label for the music player UI
        /// </summary>
        public void SetupNowPlayingLabel()
        {
            if (activeSong != null)
            {
                NowPlayingLabel.Content = $"Now playing: {activeSong.Name} by {activeSong.Artist}";
            }
            else
            {
                NowPlayingLabel.Content = "Nothing is playing...";
            }
        }

        #endregion

        #region Event handling methods

        /// <summary>
        /// Event method for when the Play/Pause button is clicked
        /// </summary>
        /// /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoPlayPauseButtonAction(object sender, RoutedEventArgs args)
        {
            if (playerInputStream == null && UpNextControl.UpNext.SongList.Count > 0) // first click on play button
            {
                SetupNextSong();

                //play
                player.Play();

                //set the button's pause image
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };

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
            else if (player.PlaybackState == PlaybackState.Stopped && playerInputStream != null) // hits here when the seekbar is moved when paused
            {
                timer.Start();
                player.Play();
                Playbutton.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/SpectralPlayerApp;component/Images/PlayerUI/pause.png")) };
            }
        }

        /// <summary>
        /// The DispatcherTimer event for updating the seekbar time
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments with the event</param>
        public void DoSeekBarUpdate(object sender, EventArgs args)
        {
            TimestampLabel.Content = backingWaveStream?.CurrentTime.ToString(@"mm\:ss") ?? "00:00";
            SeekBarPos = backingWaveStream.CurrentTime.TotalSeconds;
            SeekSlider.Value = backingWaveStream.CurrentTime.TotalSeconds;
        }

        /// <summary>
        /// The event fired when the volume bar is changed, indicating a user made change to the volume
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoVolumeChange(object sender, RoutedEventArgs args)
        {
            player.Volume = (float)VolumeSlider.Value;
        }

        /// <summary>
        /// The seekbar event fired when the user dragged and let go of the seekbar
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoSeek(object sender, RoutedEventArgs args)
        {
            if (playerInputStream != null)
            {
                backingWaveStream.CurrentTime = TimeSpan.FromSeconds(SeekSlider.Value);
                
                if (player.PlaybackState == PlaybackState.Paused)
                {
                    stopLock = true;
                    player.Stop(); // to flush the buffer, or else we get some of the song from before we moved playing
                }
            }
            TimestampLabel.Content = backingWaveStream.CurrentTime.ToString(@"mm\:ss");
            
            timer.Start();
        }

        /// <summary>
        /// The event fired when the seekbar is being dragged but not released. This stops the timer temperarily in order to avoid the thumb from studdering from the timer's event
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoHoldTimer(object sender, MouseButtonEventArgs args)
        {
            timer.Stop();
        }

        /// <summary>
        /// The event fired when the skip button is pressed, skipping the current song and playing the next
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoSkipCurrentSong(object sender, RoutedEventArgs args)
        {
            if (UpNextControl.UpNext.SongList.Count > 0)
            {
                prevRecord = true; //tell the stop we want to record the previous song
                player.Stop(); //call the onstopped, this will take care of the rest, like the next song
                if (activeSong != null)
                {
                    ParentWindow.SendDiscordRPCUpdate($"{activeSong.Name}", $"by {activeSong.Artist}", "listening");
                }
                else
                {
                    ParentWindow.SendDiscordRPCUpdate("Sitting in silence", "and pondering life", "silence");
                }
            }
        }

        /// <summary>
        /// The event fired when the back button is pressed which recalls the previous song played
        /// </summary>
        /// <param name="sender">The object that sent this event</param>
        /// <param name="args">The related event arguments of the event</param>
        public void DoBackASong(object sender, RoutedEventArgs args)
        {
            if (previousSongs.Count > 0)
            {
                if (player.PlaybackState == PlaybackState.Stopped)
                {
                    UpNextControl.UpNext.SongList.Insert(0, previousSongs.Pop());
                    Song formerActive = activeSong;
                    prevRecord = false;
                    SetupNextSong();
                    if (formerActive != null)
                    {
                        UpNextControl.UpNext.SongList.Insert(0, formerActive);
                    }
                    if (activeSong != null)
                    {
                        ParentWindow.SendDiscordRPCUpdate($"{activeSong.Name}", $"by {activeSong.Artist}", "listening");
                    }
                    else
                    {
                        ParentWindow.SendDiscordRPCUpdate("Sitting in silence", "and pondering life", "silence");
                    }
                }
                else
                {
                    UpNextControl.UpNext.SongList.Insert(0, activeSong);
                    UpNextControl.UpNext.SongList.Insert(0, previousSongs.Pop());
                    prevRecord = false; //tell the stop to not record the previous song
                    player.Stop();
                    if (activeSong != null)
                    {
                        ParentWindow.SendDiscordRPCUpdate($"{activeSong.Name}", $"by {activeSong.Artist}", "listening");
                    }
                    else
                    {
                        ParentWindow.SendDiscordRPCUpdate("Sitting in silence", "and pondering life", "silence");
                    }
                }
            }
        }

        #endregion

        #region Events/Delegates

        public event PropertyChangedEventHandler PropertyChanged;
        protected void FieldChanged(string field = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(field));
            }
        }

        #endregion
    }
}
