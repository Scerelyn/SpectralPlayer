using MusicLibraryLib;
using SpectralPlayerApp.MusicPlayerViewControls;
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

namespace SpectralPlayerApp.LibraryViewControls
{
    /// <summary>
    /// Interaction logic for LibraryGenreView.xaml
    /// </summary>
    public partial class LibraryGenreListViewControl : UserControl
    {
        /// <summary>
        /// The UpNext control to access and change the upnext playlist with
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }
        /// <summary>
        /// The parent mainwindow instance
        /// </summary>
        public MainWindow ParentWindow { get; set; }

        public LibraryGenreListViewControl()
        {
            InitializeComponent();
        }

        public void DoAddSongsToUpNext(object sender, RoutedEventArgs args)
        {
            if (GenreListBox.SelectedItems.Count > 0)
            {
                foreach (Song s in GenreListBox.SelectedItems)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoSelectionChanged(object sender, RoutedEventArgs args)
        {
            if (GenreListBox.SelectedItems.Count == 1)
            {
                AddGenreMenuItem.Visibility = Visibility.Visible;
                AddGenreMenuItem.Header = $"Add Genre {(GenreListBox.SelectedItems[0] as Song).Genre} to Up-Next";
                AddGenrePlaylistMenuItem.Visibility = Visibility.Visible;
                AddGenrePlaylistMenuItem.Header = $"Add Genre {(GenreListBox.SelectedItems[0] as Song).Genre} to playlist...";
                EditSongInfoMenuItem.Visibility = Visibility.Visible;
                ShuffleGenreMenuItem.Header = $"Shuffle Genre {(GenreListBox.SelectedItems[0] as Song).Genre} into Up-Next";
                ShuffleGenreMenuItem.Visibility = Visibility.Visible;
            }
            else
            {
                AddGenreMenuItem.Visibility = Visibility.Collapsed;
                AddGenrePlaylistMenuItem.Visibility = Visibility.Collapsed;
                EditSongInfoMenuItem.Visibility = Visibility.Collapsed;
                ShuffleGenreMenuItem.Visibility = Visibility.Collapsed;
            }
        }

        public void DoAddGenreToUpNext(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)GenreListBox.SelectedItems[0];
            foreach (Song s in GenreListBox.Items)
            {
                if (s.Genre == selected.Genre)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoShuffleInto(object sender, RoutedEventArgs args)
        {
            if (GenreListBox.SelectedItems.Count > 0)
            {
                List<Song> songs = new List<Song>();
                foreach (Song s in GenreListBox.SelectedItems)
                {
                    songs.Add(s);
                }
                UpNextControl.UpNext.ShuffleSongsInto(songs);
            }
        }

        public void DoAddSongsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList pl = new PlayList() { Name = "New Playlist" };
            foreach (Song s in GenreListBox.SelectedItems)
            {
                pl.SongList.Add(s);
            }
            ParentWindow.SongLibrary.PlayListList.Add(pl);
            ParentWindow.UpdatePlayListContextMenuItems();
        }

        public void DoEditSong(object sender, RoutedEventArgs args)
        {
            ParentWindow.EditSongData(GenreListBox.SelectedItems[0] as Song);
        }

        public void DoShuffleGenreInto(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)GenreListBox.SelectedItems[0];
            List<Song> songs = new List<Song>();
            foreach (Song s in GenreListBox.Items)
            {
                if (s.Genre == selected.Genre)
                {
                    songs.Add(s);
                }
            }
            UpNextControl.UpNext.ShuffleSongsInto(songs);
        }

        public void DoRemoveSongs(object sender, RoutedEventArgs args)
        {
            MessageBoxResult result = MessageBox.Show("Remove selected songs from library?", "Remove songs?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Song[] songsToRemove = new Song[GenreListBox.SelectedItems.Count];
                GenreListBox.SelectedItems.CopyTo(songsToRemove, 0);
                ParentWindow.RemoveSongs(songsToRemove);
                MessageBox.Show("Songs removed from library");
            }
        }
    }
}
