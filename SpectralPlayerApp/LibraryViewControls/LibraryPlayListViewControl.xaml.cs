using MusicLibraryLib;
using SpectralPlayerApp.Dialogs;
using SpectralPlayerApp.MusicPlayerViewControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// <summary>
        /// The parent mainwindow instance
        /// </summary>
        public MainWindow ParentWindow { get; set; }
        /// <summary>
        /// A list of the inner listboxes of the outer PlaylistListBox
        /// </summary>
        public List<ListBox> InnerListBoxes { get; } = new List<ListBox>();

        public LibraryPlayListViewControl()
        {
            InitializeComponent();
        }

        public void DoSelectionChanged(object sender, RoutedEventArgs args)
        {
            if(PlaylistListBox.SelectedItems.Count != 1)
            {
                RenamePlaylistMenuItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                RenamePlaylistMenuItem.Visibility = Visibility.Visible;
            }
        }

        public void DoAddSelectedPlaylistsToUpNext(object sender, RoutedEventArgs args)
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

        public void DoPlaylistShuffleInto(object sender, RoutedEventArgs args)
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

        public void DoSongsShuffleInto(object sender, RoutedEventArgs args)
        {
            List<Song> songs = new List<Song>();
            foreach (ListBox lb in InnerListBoxes)
            {
                foreach (Song s in lb.SelectedItems)
                {
                    songs.Add(s);
                }
            }
            UpNextControl.UpNext.ShuffleSongsInto(songs);
        }

        public void DoDeletePlaylist(object sender, RoutedEventArgs args)
        {
            MessageBoxResult result = MessageBox.Show("Remove selected playlists from library?", "Remove playlists?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
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
                ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
            }
        }

        public void DoRemoveSongFromPlayList(object sender, RoutedEventArgs args)
        {
            MessageBoxResult result = MessageBox.Show("Remove selected songs from playlist?", "Remove songs?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                foreach (ListBox lb in InnerListBoxes)
                {
                    while (lb.SelectedItems.Count > 0)
                    {
                        (lb.ItemsSource as ObservableCollection<Song>).Remove(lb.SelectedItems[0] as Song);
                    }
                }
                ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
            }
        }

        public void DoAddSelectedSongsToUpNext(object sender, RoutedEventArgs args)
        {
            foreach (ListBox lb in InnerListBoxes)
            {
                foreach (Song s in lb.SelectedItems)
                {
                    UpNextControl.UpNext.SongList.Add(s);
                }
            }
        }

        public void DoInnerListBoxSelectionChanged(object sender, RoutedEventArgs args)
        {
            //since there is no easy was to access all the listbox instances inside a listbox's data template, i have to do this
            ListBox lb = sender as ListBox; //get the sender, the innerlist box
            if (!InnerListBoxes.Contains(lb))
            {
                InnerListBoxes.Add(lb); //and add it to the list of listboxes if it is not already in it
            }
        }

        public void DoAddSelectedSongsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList pl = new PlayList() { Name = "New Playlist" };
            foreach (ListBox lb in InnerListBoxes)
            {
                foreach (Song s in lb.SelectedItems)
                {
                    pl.SongList.Add(s);
                }
            }
            ParentWindow.SongLibrary.PlayListList.Add(pl);
            ParentWindow.UpdatePlayListContextMenuItems();
            ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
        }

        public void DoAddSelectedPlaylistsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList newPlaylist = new PlayList() { Name = "New Playlist" };
            foreach (PlayList pl in PlaylistListBox.SelectedItems)
            {
                foreach (Song s in pl.SongList)
                {
                    newPlaylist.SongList.Add(s);
                }
            }
            ParentWindow.SongLibrary.PlayListList.Add(newPlaylist);
            ParentWindow.UpdatePlayListContextMenuItems();
            ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
        }

        public void DoRenamePlaylist(object sender, RoutedEventArgs args)
        {
            PlayList pl = PlaylistListBox.SelectedItems[0] as PlayList;
            SingleStringInputDialog ssid = new SingleStringInputDialog("Enter a new Playlist name", "Rename playlist");
            ssid.ShowDialog();
            if (ssid.DialogResult ?? false)
            {
                pl.Name = ssid.InputValue;
                //var plList = PlaylistListBox.ItemsSource as ObservableCollection<PlayList>;
                ParentWindow.UpdateLists();
                ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
            }
        }

    }
}
