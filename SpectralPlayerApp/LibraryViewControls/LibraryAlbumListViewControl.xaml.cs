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
            }
            else
            {
                AddAlbumMenuItem.Visibility = Visibility.Collapsed;
                AddAlbumPlaylistMenuItem.Visibility = Visibility.Collapsed;
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
    }
}
