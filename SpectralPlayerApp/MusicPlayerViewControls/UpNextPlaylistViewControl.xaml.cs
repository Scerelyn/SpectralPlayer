﻿using MusicLibraryLib;
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

namespace SpectralPlayerApp.MusicPlayerViewControls
{
    /// <summary>
    /// Interaction logic for UpNextPlaylistViewControl.xaml
    /// </summary>
    public partial class UpNextPlaylistViewControl : UserControl
    {
        public PlayList UpNext { get; } = new PlayList();
        public UpNextPlaylistViewControl()
        {
            InitializeComponent();
            UpNextPlaylistListBox.ItemsSource = UpNext.SongList;
        }

        public void DoShuffle(object sender, RoutedEventArgs args)
        {
            UpNext.Shuffle();
        }

        public void DoClear(object sender, RoutedEventArgs args)
        {
            UpNext.SongList.Clear();
        }

        public void DoRemoveSelected(object sender, RoutedEventArgs args)
        {
            if (UpNextPlaylistListBox.SelectedItems.Count > 0)
            {
                List<Song> toRemove = new List<Song>();
                foreach (Song s in UpNextPlaylistListBox.SelectedItems)
                {
                    toRemove.Add(s);
                }
                
                for (int i = 0; i < UpNext.SongList.Count; i++)
                {
                    if (toRemove.Contains(UpNext.SongList[i]))
                    {
                        UpNext.SongList.RemoveAt(i);
                        i--;
                    }
                }
                
            }
        }
    }
}
