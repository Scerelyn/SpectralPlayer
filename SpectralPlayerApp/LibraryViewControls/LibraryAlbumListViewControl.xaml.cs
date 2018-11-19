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
    /// Interaction logic for LibraryAlbumListViewControl.xaml
    /// </summary>
    public partial class LibraryAlbumListViewControl : UserControl
    {
        /// <summary>
        /// The UpNext control to access and change the upnext playlist with
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }
        /// <summary>
        /// The parent mainwindow instance
        /// </summary>
        public MainWindow ParentWindow { get; set; }

        public LibraryAlbumListViewControl()
        {
            InitializeComponent();
        }

        public void DoAddSongsToUpNext(object sender, RoutedEventArgs args)
        {
            if (AlbumListBox.SelectedItems.Count > 0)
            {
                foreach (Song s in AlbumListBox.SelectedItems)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoSelectionChanged(object sender, RoutedEventArgs args)
        {
            if (AlbumListBox.SelectedItems.Count == 1)
            {
                AddAlbumMenuItem.Visibility = Visibility.Visible;
                AddAlbumMenuItem.Header = $"Add Album {(AlbumListBox.SelectedItems[0] as Song).AlbumName} to Up-Next";
                AddAlbumPlaylistMenuItem.Visibility = Visibility.Visible;
                AddAlbumPlaylistMenuItem.Header = $"Add Album {(AlbumListBox.SelectedItems[0] as Song).AlbumName} to playlist...";
                EditSongInfoMenuItem.Visibility = Visibility.Visible;
                ShuffleAlbumIntoMenuItem.Header = $"Shuffle Album {(AlbumListBox.SelectedItems[0] as Song).AlbumName}";
            }
            else
            {
                AddAlbumMenuItem.Visibility = Visibility.Collapsed;
                AddAlbumPlaylistMenuItem.Visibility = Visibility.Collapsed;
                EditSongInfoMenuItem.Visibility = Visibility.Collapsed;
                ShuffleAlbumIntoMenuItem.Visibility = Visibility.Collapsed;
            }
        }

        public void DoAddAlbumToUpNext(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)AlbumListBox.SelectedItems[0];
            foreach (Song s in AlbumListBox.Items)
            {
                if (s.AlbumName == selected.AlbumName)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoShuffleInto(object sender, RoutedEventArgs args)
        {
            if (AlbumListBox.SelectedItems.Count > 0)
            {
                List<Song> songs = new List<Song>();
                foreach (Song s in AlbumListBox.SelectedItems)
                {
                    songs.Add(s);
                }
                UpNextControl.UpNext.ShuffleSongsInto(songs);
            }
        }

        public void DoAddSongsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList pl = new PlayList() { Name = "New Playlist" };
            foreach (Song s in AlbumListBox.SelectedItems)
            {
                pl.SongList.Add(s);
            }
            ParentWindow.SongLibrary.PlayListList.Add(pl);
            ParentWindow.UpdatePlayListContextMenuItems();
        }

        public void DoEditSong(object sender, RoutedEventArgs args)
        {
            ParentWindow.EditSongData(AlbumListBox.SelectedItems[0] as Song);
        }

        public void DoShuffleAlbumInto(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)AlbumListBox.SelectedItems[0];
            List<Song> songs = new List<Song>();
            foreach (Song s in AlbumListBox.Items)
            {
                if (s.AlbumName == selected.AlbumName)
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
                Song[] songsToRemove = new Song[AlbumListBox.SelectedItems.Count];
                AlbumListBox.SelectedItems.CopyTo(songsToRemove, 0);
                ParentWindow.RemoveSongs(songsToRemove);
                MessageBox.Show("Songs removed from library");
            }
        }
    }
}
