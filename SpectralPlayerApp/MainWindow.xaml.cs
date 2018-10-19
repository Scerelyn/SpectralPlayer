using Microsoft.Win32;
using MusicLibraryLib;
using NAudio.Wave;
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

namespace SpectralPlayerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public OpenFileDialog OpenAudioFileDialog { get; } = new OpenFileDialog();
        public Library SongLibrary { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            SongLibrary = GetSampleLibrary();

            UpdateLists();

            PlaylistControl.PlaylistListBox.ItemsSource = SongLibrary.PlayListList;

            OpenAudioFileDialog.Multiselect = true;
            OpenAudioFileDialog.Filter = "Audio (*.mp3;*.wav;*.flac;*.ogg;*.aac)|*.mp3;*.wav;*.flac;*.ogg;*.aac";

            AllSongsControl.UpNextControl = UpNextControl;
            MusicPlayerControl.UpNextControl = UpNextControl;
            ArtistsControl.UpNextControl = UpNextControl;
            AlbumsControl.UpNextControl = UpNextControl;
            GenresControl.UpNextControl = UpNextControl;
            PlaylistControl.UpNextControl = UpNextControl;

            PlaylistControl.ParentWindow = this;
        }

        public void DoAddFile(object sender, RoutedEventArgs args)
        {
            bool? result = OpenAudioFileDialog.ShowDialog();
            if (result ?? false)
            {
                foreach(string name in OpenAudioFileDialog.FileNames)
                {
                    SongLibrary.SongList.Add(new Song() { Name=name, FilePath=name });
                }
                UpdateLists();
            }
        }

        /// <summary>
        /// Refreshes the music lists for the UI when the SongLibrary's song list changes
        /// </summary>
        public void UpdateLists()
        {
            AllSongsControl.LibraryListView.ItemsSource = SongLibrary.GetOrderedListBySong();

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

                //add the menuitems to the context menu
                AllSongsControl.AddPlaylistMenuItem.Items.Add(allSongsPlayListMenuItem);
                ArtistsControl.AddPlaylistMenuItem.Items.Add(artistControlAddSongsMenuItem);
                ArtistsControl.AddArtistPlaylistMenuItem.Items.Add(artistControlAddArtistMenuItem);
                AlbumsControl.AddPlaylistMenuItem.Items.Add(albumControlAddSongsMenuItem);
                AlbumsControl.AddAlbumPlaylistMenuItem.Items.Add(albumControlAddAlbumMenuItem);
                GenresControl.AddPlaylistMenuItem.Items.Add(genreControlAddSongsMenuItem);
                GenresControl.AddGenrePlaylistMenuItem.Items.Add(genreControlAddGenreMenuItem);
                PlaylistControl.AddPlaylistMenuItem.Items.Add(playlistControlAddSongsMenuItem);
            }
        }

        /// <summary>
        /// Generates a sample music library for testing purposes
        /// </summary>
        /// <returns>A library instance with dummy Song and Playlist instances</returns>
        private Library GetSampleLibrary()
        {
            Library l = new Library();
            // sample "songs"
            Song s1 = (new Song() { Name = "jnfdkalf", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 1, Genre = "Rock" });
            Song s2 = (new Song() { Name = "jnggsdfm", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 1, Genre = "Classical" });
            Song s3 = (new Song() { Name = "klsdamfk", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 2, Genre = "Classical" });
            Song s4 = (new Song() { Name = "asdfklfm", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 2, Genre = "Rock" });
            Song s5 = (new Song() { Name = "ngkfadfs", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 1, Genre = "Rap" });
            Song s6 = (new Song() { Name = "mgfvjoks", Artist = "Bob", AlbumName = "Adequate", Year = "1992", TrackNumber = 1, Genre = "Rock" });
            Song s7 = (new Song() { Name = "kfdklmda", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 2, Genre = "Rap" });
            Song s8 = (new Song() { Name = "ntrdsmfd", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 3, Genre = "Rock" });
            Song s9 = (new Song() { Name = "klmdsaff", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 3, Genre = "Classical" });
            Song s10 = (new Song() { Name = "opksadfm", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 4, Genre = "Classical" });
            Song s11 = (new Song() { Name = "uitrsmmd", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 4, Genre = "Rock" });
            Song s12 = (new Song() { Name = "xzcvjior", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 3, Genre = "Rap" });
            Song s13 = (new Song() { Name = "nmsdafio", Artist = "Bob", AlbumName = "Adequate", Year = "1992", TrackNumber = 2, Genre = "Rock" });
            Song s14 = (new Song() { Name = "swrmjdsa", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 4, Genre = "Rap" });

            //add the songs
            l.SongList.Add(s1); l.SongList.Add(s2); l.SongList.Add(s3); l.SongList.Add(s4); l.SongList.Add(s5);
            l.SongList.Add(s6); l.SongList.Add(s7); l.SongList.Add(s8); l.SongList.Add(s9); l.SongList.Add(s10);
            l.SongList.Add(s11); l.SongList.Add(s12); l.SongList.Add(s13); l.SongList.Add(s14);

            //sample playlists
            PlayList p1 = new PlayList() { Name = "Sample playlist 1" };
            p1.SongList.Add(s1); p1.SongList.Add(s5); p1.SongList.Add(s9); p1.SongList.Add(s11);
            PlayList p2 = new PlayList() { Name = "Sample playlist 2" };
            p2.SongList.Add(s7); p2.SongList.Add(s8); p2.SongList.Add(s2); p2.SongList.Add(s6);
            PlayList p3 = new PlayList() { Name = "Sample playlist 3" };
            p3.SongList.Add(s14); p3.SongList.Add(s10); p3.SongList.Add(s4); p3.SongList.Add(s3);

            //add them
            l.PlayListList.Add(p1);
            l.PlayListList.Add(p2);
            l.PlayListList.Add(p3);

            return l;
        }
    }
}
