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
            }
            else
            {
                AddGenreMenuItem.Visibility = Visibility.Collapsed;
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
    }
}
