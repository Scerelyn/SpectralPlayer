using MusicLibraryLib;
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

namespace SpectralPlayerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LibraryAllSongsListControl libraryAllSongs = new LibraryAllSongsListControl();
            LibraryDock.Children.Add(libraryAllSongs);
            Library l = new Library();
            l.SongList.Add(new Song() { Name = "AAA", Artist="bbb", AlbumName="rrr", Year="1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            l.SongList.Add(new Song() { Name = "AAA", Artist = "bbb", AlbumName = "rrr", Year = "1111" });
            

            libraryAllSongs.LibraryListView.ItemsSource = l.SongList;
        }
    }
}
