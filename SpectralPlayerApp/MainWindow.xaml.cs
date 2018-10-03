using MusicLibraryLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            Library l = new Library();
            // sample "songs"
            l.SongList.Add(new Song() { Name = "jnfdkalf", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 1, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "jnggsdfm", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 1, Genre = "Classical" });
            l.SongList.Add(new Song() { Name = "klsdamfk", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 2, Genre = "Classical" });
            l.SongList.Add(new Song() { Name = "asdfklfm", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 2, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "ngkfadfs", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 1, Genre = "Rap" });
            l.SongList.Add(new Song() { Name = "mgfvjoks", Artist = "Bob", AlbumName = "Adequate", Year = "1992", TrackNumber = 1, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "kfdklmda", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 2, Genre = "Rap" });
            l.SongList.Add(new Song() { Name = "ntrdsmfd", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 3, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "klmdsaff", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 3, Genre = "Classical" });
            l.SongList.Add(new Song() { Name = "opksadfm", Artist = "Steve", AlbumName = "Something", Year = "2001", TrackNumber = 4, Genre = "Classical" });
            l.SongList.Add(new Song() { Name = "uitrsmmd", Artist = "Bob", AlbumName = "Good", Year = "1983", TrackNumber = 4, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "xzcvjior", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 3, Genre = "Rap" });
            l.SongList.Add(new Song() { Name = "nmsdafio", Artist = "Bob", AlbumName = "Adequate", Year = "1992", TrackNumber = 2, Genre = "Rock" });
            l.SongList.Add(new Song() { Name = "swrmjdsa", Artist = "Joe", AlbumName = "OK", Year = "2017", TrackNumber = 4, Genre = "Rap" });

            AllSongsControl.LibraryListView.ItemsSource = l.GetOrderedListBySong();

            AlbumsControl.AlbumListBox.ItemsSource = l.GetOrderedListByAlbum();

            ICollectionView view = CollectionViewSource.GetDefaultView(AlbumsControl.AlbumListBox.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("AlbumName"));

        }
    }
}
