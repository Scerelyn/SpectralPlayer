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
    /// Interaction logic for LibraryArtistListViewControl.xaml
    /// </summary>
    public partial class LibraryArtistListViewControl : UserControl
    {
        /// <summary>
        /// The UpNext control to access and change the upnext playlist with
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }

        public LibraryArtistListViewControl()
        {
            InitializeComponent();
        }

        public void DoAddSongsToUpNext(object sender, RoutedEventArgs args)
        {
            if (ArtistListBox.SelectedItems.Count > 0)
            {
                foreach (Song s in ArtistListBox.SelectedItems)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoSelectionChanged(object sender, RoutedEventArgs args)
        {
            if (ArtistListBox.SelectedItems.Count == 1)
            {
                AddArtistMenuItem.Visibility = Visibility.Visible;
                AddArtistMenuItem.Header = $"Add Artist {(ArtistListBox.SelectedItems[0] as Song).Artist} to Up-Next";
                AddArtistPlaylistMenuItem.Visibility = Visibility.Visible;
                AddArtistPlaylistMenuItem.Header = $"Add Artist {(ArtistListBox.SelectedItems[0] as Song).Artist} to playlist...";
            }
            else
            {
                AddArtistMenuItem.Visibility = Visibility.Collapsed;
                AddArtistPlaylistMenuItem.Visibility = Visibility.Collapsed;
            }
        }

        public void DoAddArtistToUpNext(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)ArtistListBox.SelectedItems[0];
            foreach (Song s in ArtistListBox.Items)
            {
                if (s.Artist == selected.Artist)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoShuffleInto(object sender, RoutedEventArgs args)
        {
            if (ArtistListBox.SelectedItems.Count > 0)
            {
                List<Song> songs = new List<Song>();
                foreach (Song s in ArtistListBox.SelectedItems)
                {
                    songs.Add(s);
                }
                UpNextControl.UpNext.ShuffleSongsInto(songs);
            }
        }
    }
}
