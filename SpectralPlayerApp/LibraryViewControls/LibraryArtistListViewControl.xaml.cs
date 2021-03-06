﻿using MusicLibraryLib;
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
        /// <summary>
        /// The parent mainwindow instance
        /// </summary>
        public MainWindow ParentWindow { get; set; }

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
                EditSongInfoMenuItem.Visibility = Visibility.Visible;
                ShuffleArtistIntoMenuItem.Header = $"Shuffle Artist {(ArtistListBox.SelectedItems[0] as Song).Artist} into Up-Next";
            }
            else
            {
                AddArtistMenuItem.Visibility = Visibility.Collapsed;
                AddArtistPlaylistMenuItem.Visibility = Visibility.Collapsed;
                EditSongInfoMenuItem.Visibility = Visibility.Collapsed;
                ShuffleArtistIntoMenuItem.Visibility = Visibility.Collapsed;
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

        public void DoAddSongsToNewPlayList(object sender, RoutedEventArgs args)
        {
            PlayList pl = new PlayList() { Name = "New Playlist" };
            foreach (Song s in ArtistListBox.SelectedItems)
            {
                pl.SongList.Add(s);
            }
            ParentWindow.SongLibrary.PlayListList.Add(pl);
            ParentWindow.UpdatePlayListContextMenuItems();
            ParentWindow.AsyncSerialize(ParentWindow.BackgroundCallback);
        }

        public void DoEditSong(object sender, RoutedEventArgs args)
        {
            ParentWindow.EditSongData(ArtistListBox.SelectedItems[0] as Song);
        }

        public void DoShuffleArtistInto(object sender, RoutedEventArgs args)
        {
            Song selected = (Song)ArtistListBox.SelectedItems[0];
            List<Song> songs = new List<Song>();
            foreach (Song s in ArtistListBox.Items)
            {
                if (s.Artist == selected.Artist)
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
                Song[] songsToRemove = new Song[ArtistListBox.SelectedItems.Count];
                ArtistListBox.SelectedItems.CopyTo(songsToRemove, 0);
                ParentWindow.RemoveSongs(songsToRemove);
                MessageBox.Show("Songs removed from library");
            }
        }

        public void DoAddToUpNext(object sender, RoutedEventArgs args)
        {
            ListBox songsListBox = sender as ListBox;
            foreach (Song s in songsListBox.SelectedItems)
            {
                UpNextControl.UpNext.SongList.Add(s);
            }
        }
    }
}
