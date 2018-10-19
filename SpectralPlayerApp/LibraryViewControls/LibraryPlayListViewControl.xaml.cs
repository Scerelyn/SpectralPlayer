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
    /// Interaction logic for LibraryPlayListViewControl.xaml
    /// </summary>
    public partial class LibraryPlayListViewControl : UserControl
    {
        /// <summary>
        /// The UpNext control to access and change the upnext playlist with
        /// </summary>
        public UpNextPlaylistViewControl UpNextControl { get; set; }
        public MainWindow ParentWindow { get; set; }

        public LibraryPlayListViewControl()
        {
            InitializeComponent();
        }

        public void DoAddSongsToUpNext(object sender, RoutedEventArgs args)
        {
            if (PlaylistListBox.SelectedItems.Count > 0)
            {
                foreach (PlayList playlist in PlaylistListBox.SelectedItems)
                {
                    if (playlist.SongList.Count > 0)
                    {
                        foreach (Song s in playlist.SongList)
                        {
                            UpNextControl.UpNext.SongList.Add(s);
                        }
                    }
                }
            }
        }

        public void DoShuffleInto(object sender, RoutedEventArgs args)
        {
            if (PlaylistListBox.SelectedItems.Count > 0)
            {
                List<Song> songs = new List<Song>();
                foreach (PlayList pl in PlaylistListBox.SelectedItems)
                {
                    foreach (Song s in pl.SongList)
                    {
                        songs.Add(s);
                    }
                }
                UpNextControl.UpNext.ShuffleSongsInto(songs);
            }
        }

        public void DoDeletePlaylist(object sender, RoutedEventArgs args)
        {
            
            for (int i = 0; i < ParentWindow.SongLibrary.PlayListList.Count(); i++)
            {
                if (ParentWindow.SongLibrary.PlayListList.Contains(PlaylistListBox.SelectedItems[i] as PlayList))
                {
                    ParentWindow.SongLibrary.PlayListList.Remove(PlaylistListBox.SelectedItems[i] as PlayList);
                    i--;
                }
                if (PlaylistListBox.SelectedItems.Count <= 0)
                {
                    break;
                }
            }
            ParentWindow.UpdateLists();
        }
    }
}
