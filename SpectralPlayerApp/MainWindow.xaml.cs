using DiscordRPC;
using Microsoft.Win32;
using MusicLibraryLib;
using NAudio.Wave;
using SpectralPlayerApp.Dialogs;
using SpectralPlayerApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Serialization;
using TagLib;

namespace SpectralPlayerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public OpenFileDialog OpenAudioFileDialog { get; } = new OpenFileDialog();
        public Library SongLibrary { get; set; }

        public Brush BackgroundBrush { get; set; } = Brushes.White;
        public Brush ForegroundBrush { get; set; } = Brushes.Black;

        public DiscordRpcClient client = new DiscordRpcClient("512791431883128847");
        private RichPresence prevRP { get; set; } = new RichPresence();


        public MainWindow()
        {
            InitializeComponent();

            AsyncDeserialize(BackgroundCallback);

            OpenAudioFileDialog.Multiselect = true;
            OpenAudioFileDialog.Filter = "Audio (*.mp3;*.wav;*.flac;*.ogg;*.aac;*.m4a)|*.mp3;*.wav;*.flac;*.ogg;*.aac;*.m4a";

            AllSongsControl.UpNextControl = UpNextControl;
            MusicPlayerControl.UpNextControl = UpNextControl;
            ArtistsControl.UpNextControl = UpNextControl;
            AlbumsControl.UpNextControl = UpNextControl;
            GenresControl.UpNextControl = UpNextControl;
            PlaylistControl.UpNextControl = UpNextControl;

            PlaylistControl.ParentWindow = this;
            AllSongsControl.ParentWindow = this;
            ArtistsControl.ParentWindow = this;
            AlbumsControl.ParentWindow = this;
            GenresControl.ParentWindow = this;
            MusicPlayerControl.ParentWindow = this;

            client.Initialize();
            SendDiscordRPCUpdate("Sitting in the main window", "Listening to nothing");
        }

        #region Public methods

        /// <summary>
        /// Refreshes the music lists for the UI when the SongLibrary's song list changes
        /// </summary>
        public void UpdateLists()
        {
            AllSongsControl.LibraryListView.ItemsSource = SongLibrary.GetOrderedListBySong();
            PlaylistControl.PlaylistListBox.ItemsSource = null;
            PlaylistControl.PlaylistListBox.ItemsSource = SongLibrary.PlayListList;

            AlbumsControl.AlbumListBox.ItemsSource = SongLibrary.GetOrderedListByAlbum();
            ICollectionView albumGroupView = CollectionViewSource.GetDefaultView(AlbumsControl.AlbumListBox.ItemsSource);
            albumGroupView.GroupDescriptions.Add(new PropertyGroupDescription("AlbumName"));

            ArtistsControl.ArtistListBox.ItemsSource = SongLibrary.GetOrderedListByArtist();
            ICollectionView artistGroupView = CollectionViewSource.GetDefaultView(ArtistsControl.ArtistListBox.ItemsSource);
            artistGroupView.GroupDescriptions.Add(new PropertyGroupDescription("Artist"));

            GenresControl.GenreListBox.ItemsSource = SongLibrary.GetOrderedListByGenre();
            ICollectionView genreGroupView = CollectionViewSource.GetDefaultView(GenresControl.GenreListBox.ItemsSource);
            genreGroupView.GroupDescriptions.Add(new PropertyGroupDescription("Genre"));

            UpdatePlayListContextMenuItems();
        }

        /// <summary>
        /// Updates the playlist context menus of each library ViewControl
        /// </summary>
        public void UpdatePlayListContextMenuItems()
        {
            //playlist context menu setup
            //first clear the original lists
            AllSongsControl.AddPlaylistMenuItem.Items.Clear();
            ArtistsControl.AddPlaylistMenuItem.Items.Clear();
            ArtistsControl.AddArtistPlaylistMenuItem.Items.Clear();
            AlbumsControl.AddPlaylistMenuItem.Items.Clear();
            AlbumsControl.AddAlbumPlaylistMenuItem.Items.Clear();
            GenresControl.AddPlaylistMenuItem.Items.Clear();
            GenresControl.AddGenrePlaylistMenuItem.Items.Clear();
            PlaylistControl.AddPlaylistMenuItem.Items.Clear();
            PlaylistControl.AddSongsMenuItem.Items.Clear();

            //and readd them
            foreach (PlayList pl in SongLibrary.PlayListList)
            {
                // set the playlist menu items per playlist, for each control
                MenuItem allSongsPlayListMenuItem = new MenuItem() { Header = pl.Name };
                // set the onClick for the menuitem
                allSongsPlayListMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in AllSongsControl.LibraryListView.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem albumControlAddSongsMenuItem = new MenuItem() { Header = pl.Name }; //album control add selected songs to playlist
                albumControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in AlbumsControl.AlbumListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem albumControlAddAlbumMenuItem = new MenuItem() { Header = pl.Name }; //album control add selected album to playlist
                albumControlAddAlbumMenuItem.Click += (sender, args) =>
                {
                    Song selected = (Song)AlbumsControl.AlbumListBox.SelectedItems[0];
                    foreach (Song s in AlbumsControl.AlbumListBox.Items)
                    {
                        if (s.AlbumName == selected.AlbumName)
                        {
                            pl.SongList.Add(s);
                        }
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem artistControlAddSongsMenuItem = new MenuItem() { Header = pl.Name };
                artistControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in ArtistsControl.ArtistListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem artistControlAddArtistMenuItem = new MenuItem() { Header = pl.Name }; //add artist to playlist
                artistControlAddArtistMenuItem.Click += (sender, args) =>
                {
                    Song selected = (Song)ArtistsControl.ArtistListBox.SelectedItems[0];
                    foreach (Song s in ArtistsControl.ArtistListBox.Items)
                    {
                        if (s.Artist == selected.Artist)
                        {
                            pl.SongList.Add(s);
                        }
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem genreControlAddSongsMenuItem = new MenuItem() { Header = pl.Name };
                genreControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in GenresControl.GenreListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem genreControlAddGenreMenuItem = new MenuItem() { Header = pl.Name }; //genre control add selected genre to playlist
                genreControlAddGenreMenuItem.Click += (sender, args) =>
                {
                    Song selected = (Song)GenresControl.GenreListBox.SelectedItems[0];
                    foreach (Song s in GenresControl.GenreListBox.Items)
                    {
                        if (s.Genre == selected.Genre)
                        {
                            pl.SongList.Add(s);
                        }
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem playlistControlAddSongsMenuItem = new MenuItem() { Header = pl.Name };
                playlistControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    int oldPlaylistcount = pl.SongList.Count(); //to deal with recursive adding
                    foreach (PlayList playlist in PlaylistControl.PlaylistListBox.SelectedItems)
                    {
                        if (playlist.Equals(pl))
                        {
                            for (int i = 0; i < oldPlaylistcount; i++) //add the songs that were not just added in
                            {
                                pl.SongList.Add(playlist.SongList[i]);
                            }
                        }
                        else
                        {
                            foreach (Song s in playlist.SongList)
                            {
                                pl.SongList.Add(s);
                            }
                        }
                    }
                    AsyncSerialize(BackgroundCallback);
                };
                MenuItem playlistControlAddSelectedSongsMenuItem = new MenuItem() { Header = pl.Name };
                playlistControlAddSelectedSongsMenuItem.Click += (sender, args) => 
                {
                    foreach (ListBox lb in PlaylistControl.InnerListBoxes)
                    {
                        foreach (Song s in lb.SelectedItems)
                        {
                            pl.SongList.Add(s);
                        }
                    }
                    AsyncSerialize(BackgroundCallback);
                };

                //add the menuitems to the context menu
                AllSongsControl.AddPlaylistMenuItem.Items.Add(allSongsPlayListMenuItem);
                ArtistsControl.AddPlaylistMenuItem.Items.Add(artistControlAddSongsMenuItem);
                ArtistsControl.AddArtistPlaylistMenuItem.Items.Add(artistControlAddArtistMenuItem);
                AlbumsControl.AddPlaylistMenuItem.Items.Add(albumControlAddSongsMenuItem);
                AlbumsControl.AddAlbumPlaylistMenuItem.Items.Add(albumControlAddAlbumMenuItem);
                GenresControl.AddPlaylistMenuItem.Items.Add(genreControlAddSongsMenuItem);
                GenresControl.AddGenrePlaylistMenuItem.Items.Add(genreControlAddGenreMenuItem);
                PlaylistControl.AddPlaylistMenuItem.Items.Add(playlistControlAddSongsMenuItem);
                PlaylistControl.AddSongsMenuItem.Items.Add(playlistControlAddSelectedSongsMenuItem);
            }
        }

        /// <summary>
        /// Edits the Song data via a dialog, and attempts to save the new tag information onto the Song's respective filepath
        /// </summary>
        /// <param name="selectedSong">The Song to modify</param>
        public void EditSongData(Song selectedSong)
        {
            EditSongDialog esd = new EditSongDialog(selectedSong);
            esd.ShowDialog();
            if (esd.DialogResult ?? false)
            {
                selectedSong.Name = esd.SongNameTextBox.Text;
                selectedSong.Artist = esd.SongArtistTextBox.Text;
                selectedSong.AlbumName = esd.AlbumTextBox.Text;
                selectedSong.AlbumArtist = esd.AlbumArtistTextBox.Text;
                selectedSong.Genre = esd.GenreTextBox.Text;
                selectedSong.TrackNumber = int.Parse(esd.TrackNumberTextBox.Text);
                selectedSong.Year = esd.YearTextBox.Text;
                selectedSong.FilePath = esd.FilepathTextBox.Text;
                
                try
                {
                    using (TagLib.File file = TagLib.File.Create(selectedSong.FilePath))
                    {
                        //set the file metadata
                        file.Tag.Title = selectedSong.Name;

                        if (file.Tag.Performers.Length == 0)
                        {
                            file.Tag.Performers = null;
                            file.Tag.Performers = new string[1] { selectedSong.Artist }; //to edit the array tags, change out the ENTIRE array with values already in it
                        }
                        else
                        {
                            file.Tag.Performers[0] = selectedSong.Artist;
                        }

                        file.Tag.Album = selectedSong.AlbumName;

                        if (file.Tag.AlbumArtists.Length == 0)
                        {
                            file.Tag.AlbumArtists = null;
                            file.Tag.AlbumArtists = new string[1] { selectedSong.AlbumArtist };
                        }
                        else
                        {
                            file.Tag.AlbumArtists[0] = selectedSong.AlbumArtist;
                        }

                        if (file.Tag.Genres.Length == 0)
                        {
                            file.Tag.Genres = null;
                            file.Tag.Genres = new string[1] { selectedSong.Genre };
                        }
                        else
                        {
                            file.Tag.Genres[0] = selectedSong.Genre;
                        }

                        file.Tag.Year = (uint)int.Parse(selectedSong.Year);
                        file.Tag.Track = (uint)selectedSong.TrackNumber;
                        if (!string.IsNullOrEmpty(esd.ImageSelectedPath))
                        {
                            ByteVector imageBytes = ByteVector.FromPath(esd.ImageSelectedPath);
                            IPicture pic = new Picture(imageBytes);
                            file.Tag.Pictures = new IPicture[1] { pic };
                        }
                        file.Save();
                    }
                    UpdateLists();
                    AsyncSerialize(BackgroundCallback);
                }
                catch (UnsupportedFormatException ufe)
                {
                    MessageBox.Show("Cannot save tags to the given file, but the tags will still be saved on the program.");
                }
            }
        }

        /// <summary>
        /// Serializes the SongLibrary instance into an XML document
        /// </summary>
        /// <param name="library">The Library instance to serialize</param>
        /// <param name="path">The path to write the XML file to</param>
        public void XMLSerializeLibrary(Library library, string path="data/library.xml")
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Library");
            XmlSerializer ser = new XmlSerializer(typeof(Library), xmlRoot);
            if (!System.IO.File.Exists(path))
            {
                System.IO.Directory.CreateDirectory("data");
            }
            using (System.IO.FileStream export = System.IO.File.Create(path))
            {
                ser.Serialize(export, library);
            }
                
        }

        /// <summary>
        /// Deserializes the XML file from the given path. Returns true if read properly, false if the file does not exist
        /// </summary>
        /// <param name="path">The path of the XML file to read</param>
        /// <returns>True if the file was found and deserialized. False if the file was not found</returns>
        public bool XMLDeserializeLibrary(string path="data/library.xml")
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Library");
            XmlSerializer ser = new XmlSerializer(typeof(Library), xmlRoot);
            if (System.IO.File.Exists(path))
            {
                using (System.IO.FileStream import = System.IO.File.Open(path, System.IO.FileMode.Open))
                {
                    SongLibrary = ser.Deserialize(import) as Library;
                }
                Dispatcher.Invoke(() => { UpdateLists(); });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously deserializes the backing library xml
        /// </summary>
        /// <param name="callBack">The function to callback when deserialization is finished</param>
        /// <returns></returns>
        public async Task AsyncDeserialize(Func<Task> callBack)
        {
            Dispatcher.Invoke(() => {
                BackgroundTaskLabel.Content = "Opening Library...";
                BackgroundTaskDockPanel.Visibility = Visibility.Visible;
            });
            bool result = false;
            await Task.Factory.StartNew(() => {
                result = XMLDeserializeLibrary();
            });
            if (!result)
            {
                SongLibrary = new Library();
            }
            UpdateLists();
            await callBack();
        }

        /// <summary>
        /// Asynchronously serializes the library into a xml
        /// </summary>
        /// <param name="callBack">The function to callback when the serialization is finished</param>
        /// <returns></returns>
        public async Task AsyncSerialize(Func<Task> callBack)
        {
            Dispatcher.Invoke(() => {
                BackgroundTaskLabel.Content = "Saving Library...";
                BackgroundTaskDockPanel.Visibility = Visibility.Visible;
            });
            await Task.Factory.StartNew(() => {
                XMLSerializeLibrary(SongLibrary);
            });
            await callBack();
        }

        public async Task BackgroundCallback()
        {
            BackgroundTaskDockPanel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sends an update for Discord's rich presence
        /// </summary>
        /// <param name="details">The details to send</param>
        /// <param name="state">The state to send</param>
        public void SendDiscordRPCUpdate(string details, string state)
        {
            RichPresence rp = new RichPresence()
            {
                Details = details,
                State = state,
            };
            client.SetPresence(rp);
            prevRP.Details = rp.Details;
            prevRP.State = rp.State;
            client.Invoke();
        }

        /// <summary>
        /// Removes an IEnumerable of Songs to remove from the backing Library
        /// </summary>
        /// <param name="songsToRemove">An IEnumerable of Songs to remove</param>
        public void RemoveSongs(IEnumerable<Song> songsToRemove)
        {
            foreach(Song s in songsToRemove)
            {
                SongLibrary.SongList.Remove(s);
            }
            AsyncSerialize(BackgroundCallback);
            UpdateLists();
        }

        #endregion

        #region Event handlers

        public void DoAddFile(object sender, RoutedEventArgs args)
        {
            bool? result = OpenAudioFileDialog.ShowDialog();
            if (result ?? false)
            {
                foreach(string name in OpenAudioFileDialog.FileNames)
                {
                    var tagFile = TagLib.File.Create(name);
                    var tags = tagFile.Tag;

                    SongLibrary.SongList.Add(new Song()
                    {
                        Name = tags.Title == "" ? "Unknown Song" : tags.Title,
                        FilePath = name,
                        Artist = tags.FirstPerformer == "" ? "Unknown Artist" : tags.FirstPerformer,
                        AlbumName = tags.Album == "" ? "Unknown Album" : tags.Album,
                        AlbumArtist = tags.FirstAlbumArtist,
                        Genre = tags.FirstGenre,
                        TrackNumber = (int)tags.Track,
                        Year = tags.Year == 0 ? "" : tags.Year+"",
                    });
                }
                UpdateLists();
                AsyncSerialize(BackgroundCallback);
                
            }
        }

        public void DoExport(object sender, RoutedEventArgs args)
        {
            AsyncSerialize(BackgroundCallback);
        }

        public void DoImport(object sender, RoutedEventArgs args)
        {
            AsyncDeserialize(BackgroundCallback);
        }

        public void DoConvertFile(object sender, RoutedEventArgs args)
        {
            string prevDetails = prevRP.Details;
            string prevState = prevRP.State;
            SendDiscordRPCUpdate("Converting an audio file", "of some kind");
            ConvertFileDialog cfd = new ConvertFileDialog(this);
            cfd.Closed += (s, e) => { SendDiscordRPCUpdate(prevDetails, prevState); };
            cfd.ShowDialog();
        }

        public void DoConvertSong(object sender, RoutedEventArgs args)
        {
            string prevDetails = prevRP.Details;
            string prevState = prevRP.State;
            SendDiscordRPCUpdate("Converting a song","of some kind");
            ConvertSongDialog csd = new ConvertSongDialog(SongLibrary, this);
            csd.Closed += (s,e) => { SendDiscordRPCUpdate(prevDetails, prevState); };
            csd.ShowDialog();
        }

        public void DoVisualizerSettings(object sender, RoutedEventArgs args)
        {
            string prevDetails = prevRP.Details;
            string prevState = prevRP.State;
            SendDiscordRPCUpdate("Changing some settings", "for their needs");
            // get the active visualizer so i can set it as a default value for the settings window
            int selectedVisualizer = 0;
            bool useDecibel = false;
            if (MusicPlayerControl.ImageHoldingLabel.Visibility == Visibility.Visible)
            {
                selectedVisualizer = 0;
            }
            else if (MusicPlayerControl.SpectrumAnalyzer.Visibility == Visibility.Visible)
            {
                selectedVisualizer = 1;
                useDecibel = MusicPlayerControl.SpectrumAnalyzer.UseDecibelScale;
            }
            else if (MusicPlayerControl.PeakMeterAnalyzer.Visibility == Visibility.Visible)
            {
                selectedVisualizer = 2;
            }
            else if (MusicPlayerControl.SpectrumPeakAnalyzer.Visibility == Visibility.Visible)
            {
                selectedVisualizer = 3;
                useDecibel = MusicPlayerControl.SpectrumPeakAnalyzer.UseDecibelScale;
            }
            
            VisualizerSettingsDialog vsd = new VisualizerSettingsDialog(ForegroundBrush, BackgroundBrush, selectedVisualizer, useDecibel);
            vsd.Closed += (s, e) => { SendDiscordRPCUpdate(prevDetails, prevState); };
            vsd.ShowDialog();
            if (vsd.DialogResult ?? false)
            {
                MusicPlayerControl.SpectrumAnalyzer.BackgroundBrush = vsd.SelectedBackgroundBrush;
                MusicPlayerControl.SpectrumAnalyzer.GraphLineBrush = vsd.SelectedForegroundBrush;
                MusicPlayerControl.PeakMeterAnalyzer.CanvasBrush = vsd.SelectedBackgroundBrush;
                MusicPlayerControl.PeakMeterAnalyzer.MeterBrush = vsd.SelectedForegroundBrush;
                MusicPlayerControl.SpectrumPeakAnalyzer.BackgroundBrush = vsd.SelectedBackgroundBrush;
                MusicPlayerControl.SpectrumPeakAnalyzer.BarBrush = vsd.SelectedForegroundBrush;
                BackgroundBrush = vsd.SelectedBackgroundBrush;
                ForegroundBrush = vsd.SelectedForegroundBrush;
                switch (vsd.VisualizerChoice)
                {
                    case "Album Art":
                        MusicPlayerControl.SpectrumAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.PeakMeterAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.ImageHoldingLabel.Visibility = Visibility.Visible;
                        MusicPlayerControl.SpectrumPeakAnalyzer.Visibility = Visibility.Collapsed;
                        break;
                    case "Spectrum":
                        MusicPlayerControl.SpectrumAnalyzer.Visibility = Visibility.Visible;
                        MusicPlayerControl.PeakMeterAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.ImageHoldingLabel.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.SpectrumPeakAnalyzer.Visibility = Visibility.Collapsed;
                        break;
                    case "Peak Meter":
                        MusicPlayerControl.SpectrumAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.PeakMeterAnalyzer.Visibility = Visibility.Visible;
                        MusicPlayerControl.ImageHoldingLabel.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.SpectrumPeakAnalyzer.Visibility = Visibility.Collapsed;
                        break;
                    case "Spectrum Peak":
                        MusicPlayerControl.SpectrumAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.PeakMeterAnalyzer.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.ImageHoldingLabel.Visibility = Visibility.Collapsed;
                        MusicPlayerControl.SpectrumPeakAnalyzer.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
                MusicPlayerControl.SpectrumAnalyzer.UseDecibelScale = vsd.DecibelScaleCheckBox.IsChecked ?? false;
                MusicPlayerControl.SpectrumPeakAnalyzer.UseDecibelScale = vsd.DecibelScaleCheckBox.IsChecked ?? false;
            }
        }

        public void DoSelectStereo(object sender, RoutedEventArgs args)
        {
            MusicPlayerControl.UseStereo = true;
            StereoChannelMenuItem.Header = "Use Stereo (Selected)";
            MonoChannelMenuItem.Header = "Use Mono";
            DefaultChannelMenuItem.Header = "Use Default";
        }

        public void DoSelectMono(object sender, RoutedEventArgs args)
        {
            MusicPlayerControl.UseStereo = false;
            StereoChannelMenuItem.Header = "Use Stereo";
            MonoChannelMenuItem.Header = "Use Mono (Selected)";
            DefaultChannelMenuItem.Header = "Use Default";
        }

        public void DoSelectDefault(object sender, RoutedEventArgs args)
        {
            MusicPlayerControl.UseStereo = null;
            StereoChannelMenuItem.Header = "Use Stereo";
            MonoChannelMenuItem.Header = "Use Mono";
            DefaultChannelMenuItem.Header = "Use Default (Selected)";
        }

        public void DoOnClose(object sender, EventArgs args)
        {
            client.Dispose();
        }

        #endregion
    }
}
