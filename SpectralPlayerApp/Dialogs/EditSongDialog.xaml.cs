using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace SpectralPlayerApp.Dialogs
{
    /// <summary>
    /// Interaction logic for EditSongDialog.xaml
    /// </summary>
    public partial class EditSongDialog : Window
    {
        public Song SelectedSong { get; set; }
        public string ImageSelectedPath { get; private set; }
        public EditSongDialog(Song selected)
        {
            SelectedSong = selected;
            InitializeComponent();
            SongNameTextBox.Text = SelectedSong.Name;
            SongArtistTextBox.Text = SelectedSong.Artist;
            AlbumTextBox.Text = SelectedSong.AlbumName;
            AlbumArtistTextBox.Text = SelectedSong.AlbumArtist;
            GenreTextBox.Text = SelectedSong.Genre;
            YearTextBox.Text = SelectedSong.Year;
            TrackNumberTextBox.Text = SelectedSong.TrackNumber + "";
            FilepathTextBox.Text = SelectedSong.FilePath;
        }

        public void DoSave(object sender, RoutedEventArgs args)
        {
            //do verification for year and track number
            bool verifyTrackNum = int.TryParse(TrackNumberTextBox.Text, out int trackNum);
            bool verifyYear = int.TryParse(YearTextBox.Text, out int year) || YearTextBox.Text == ""; //year could be blank and thats fine
            if (!verifyTrackNum && !verifyYear)
            {
                MessageBox.Show("Track number and year are not valid numbers!");
            }
            else if (!verifyTrackNum)
            {
                MessageBox.Show("Track number is not a valid number");
            }
            else if (!verifyYear)
            {
                MessageBox.Show("The year is not a valid number");
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }

        public void DoSelectImage(object sender, RoutedEventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            bool? result = ofd.ShowDialog();
            if (result ?? false)
            {
                ImageSelectedPath = ofd.FileName;
                ImageFilePathLabel.Content = ofd.FileName;
            }
        }

        public void DoCancel(object sender, RoutedEventArgs args)
        {
            DialogResult = false;
            Close();
        }
    }
}
