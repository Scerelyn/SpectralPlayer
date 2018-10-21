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
    }
}
