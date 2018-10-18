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

            foreach (PlayList pl in SongLibrary.PlayListList)
            {
                AllSongsControl.AddPlaylistMenuItem.Items.Add(new MenuItem() { Header = pl.Name });
                ArtistsControl.AddPlaylistMenuItem.Items.Add(new MenuItem() { Header = pl.Name });
                AlbumsControl.AddPlaylistMenuItem.Items.Add(new MenuItem() { Header = pl.Name });
                GenresControl.AddPlaylistMenuItem.Items.Add(new MenuItem() { Header = pl.Name });
                PlaylistControl.AddPlaylistMenuItem.Items.Add(new MenuItem() { Header = pl.Name });
            }

            
            OpenAudioFileDialog.Multiselect = true;
            OpenAudioFileDialog.Filter = "Audio (*.mp3;*.wav;*.flac;*.ogg;*.aac)|*.mp3;*.wav;*.flac;*.ogg;*.aac";

            AllSongsControl.UpNextControl = UpNextControl;
            MusicPlayerControl.UpNextControl = UpNextControl;
            
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
