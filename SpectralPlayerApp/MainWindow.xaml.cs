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
                };
                MenuItem albumControlAddSongsMenuItem = new MenuItem() { Header = pl.Name }; //album control add selected songs to playlist
                albumControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in AlbumsControl.AlbumListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
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
                };
                MenuItem artistControlAddSongsMenuItem = new MenuItem() { Header = pl.Name };
                artistControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in ArtistsControl.ArtistListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
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
                };
                MenuItem genreControlAddSongsMenuItem = new MenuItem() { Header = pl.Name };
                genreControlAddSongsMenuItem.Click += (sender, args) =>
                {
                    foreach (Song s in GenresControl.GenreListBox.SelectedItems)
                    {
                        pl.SongList.Add(s);
                    }
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

        private async Task BackgroundCallback()
        {
            BackgroundTaskDockPanel.Visibility = Visibility.Collapsed;
        }

        public void SendDiscordRPCUpdate(string details, string state)
        {
            RichPresence rp = new RichPresence()
            {
                Details = details,
                State = state,
            };
            client.SetPresence(rp);
            client.Invoke();
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
            ConvertFileDialog cfd = new ConvertFileDialog(this);
            cfd.ShowDialog();
        }

        public void DoConvertSong(object sender, RoutedEventArgs args)
        {
            ConvertSongDialog csd = new ConvertSongDialog(SongLibrary, this);
            csd.ShowDialog();
        }

        public void DoVisualizerSettings(object sender, RoutedEventArgs args)
        {
            VisualizerSettingsDialog vsd = new VisualizerSettingsDialog(ForegroundBrush, BackgroundBrush);
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

        public void DoMonoStereoSwap(object sender, RoutedEventArgs args)
        {
            if (MusicPlayerControl.UseStereo)
            {
                MusicPlayerControl.UseStereo = false;
                MonoStereoMenuItem.Header = "Use stereo audio for music output";
            }
            else
            {
                MusicPlayerControl.UseStereo = true;
                MonoStereoMenuItem.Header = "Use mono audio for music output";
            }
            
        }

        public void DoOnClose(object sender, EventArgs args)
        {
            client.Dispose();
        }

        #endregion
    }
}
