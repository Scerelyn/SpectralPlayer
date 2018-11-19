using MusicLibraryLib;
using SpectralPlayerApp.Dialogs;
using SpectralPlayerApp.MusicPlayerViewControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using TagLib;

namespace SpectralPlayerApp.LibraryViewControls
{
    /// <summary>
    /// Interaction logic for LibraryAllSongsListViewControl.xaml
    /// </summary>
    public partial class LibraryAllSongsListViewControl : UserControl
    {
        /// <summary>
        /// The UpNext control to access and change the upnext playlist with
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }
        /// <summary>
        /// The parent mainwindow instance
        /// </summary>
        public MainWindow ParentWindow { get; set; }

        public LibraryAllSongsListViewControl()
        {
            InitializeComponent();
        }

        public void DoAddSongsToUpNext(object sender, RoutedEventArgs args)
        {
            if (LibraryListView.SelectedItems.Count > 0)
            {
                foreach(Song s in LibraryListView.SelectedItems)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoShuffleInto(object sender, RoutedEventArgs args)
        {
            if (LibraryListView.SelectedItems.Count > 0)
            {
                List<Song> songs = new List<Song>();
                foreach (Song s in LibraryListView.SelectedItems)
                {
                    songs.Add(s);
                }
                UpNextControl.UpNext.ShuffleSongsInto(songs);
            }
        }

        public void DoAddSongsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList pl = new PlayList() { Name = "New Playlist" };
            foreach (Song s in LibraryListView.SelectedItems)
            {
                pl.SongList.Add(s);
            }
            ParentWindow.SongLibrary.PlayListList.Add(pl);
            ParentWindow.UpdatePlayListContextMenuItems();
        }

        public void DoSelectionChanged(object sender, RoutedEventArgs args)
        {
            if (LibraryListView.SelectedItems.Count == 1)
            {
                EditSongInfoMenuItem.Visibility = Visibility.Visible;
            }
            else
            {
                EditSongInfoMenuItem.Visibility = Visibility.Collapsed;
            }
        }

        public void DoEditSong(object sender, RoutedEventArgs args)
        {
            ParentWindow.EditSongData(LibraryListView.SelectedItems[0] as Song);
        }

        public void DoRemoveSongs(object sender, RoutedEventArgs args)
        {
            MessageBoxResult result = MessageBox.Show("Remove selected songs from library?", "Remove songs?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Song[] songsToRemove = new Song[LibraryListView.SelectedItems.Count];
                LibraryListView.SelectedItems.CopyTo(songsToRemove, 0);
                ParentWindow.RemoveSongs(songsToRemove);
                MessageBox.Show("Songs removed from library");
            }
        }
    }
}
