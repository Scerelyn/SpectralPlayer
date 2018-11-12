using MusicLibraryLib;
using NAudio.Flac;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SpectralPlayerApp.Dialogs
{
    /// <summary>
    /// Interaction logic for ConvertSongDialog.xaml
    /// </summary>
    public partial class ConvertSongDialog : Window
    {
        List<Song> selectedSongs = new List<Song>();
        public ConvertSongDialog(Library songLibrary)
        {
            InitializeComponent();
            Library.LibraryListView.ItemsSource = songLibrary.SongList;
            List<string> Filetypes = new List<string>() {
                //"Windows Media Audio (.wma)",
                "Waveform Audio File Format (.wav)",
                "MPEG Audio Layer III (.mp3)",
            };
            OutputFileTypeComboBox.ItemsSource = Filetypes;
            OutputFileTypeComboBox.SelectedIndex = 0;
            Library.LibraryListView.ContextMenu = null; //context menu makes no sense in this context, so just remove it
            
        }

        public async void DoFileConvert(object sender, RoutedEventArgs args)
        {
            foreach (Song s in Library.LibraryListView.SelectedItems)
            {
                selectedSongs.Add(s);
            }
            if (selectedSongs.Count() <= 0)
            {
                MessageBox.Show("Select files first!");
            }
            else
            {
                WaitingProgressBar.Value = WaitingProgressBar.Minimum;
                WaitingProgressBar.IsIndeterminate = true;
                WaitingProgressBar.Visibility = Visibility.Visible;
                WaitingLabel.Visibility = Visibility.Visible;
                WaitingLabel.Content = "Converting files, hang on...";
                Exportbutton.IsEnabled = false;
                await Task.Factory.StartNew(async () => {
                    await ConvertFiles(ConvertCallBack);
                });
            }
        }

        private async Task ConvertFiles(Func<Task> callBack)
        {
            await Task.Delay(250); // give time for rendering changes
            WaveStream inputStream = null;
            for (int i = 0; i < selectedSongs.Count(); i++)
            {
                try
                {
                    if (selectedSongs[i].FilePath.EndsWith(".ogg"))
                    {
                        inputStream = new VorbisWaveReader(selectedSongs[i].FilePath);
                    }
                    else if (selectedSongs[i].FilePath.EndsWith(".flac"))
                    {
                        inputStream = new FlacReader(selectedSongs[i].FilePath);
                    }
                    else
                    {
                        inputStream = new AudioFileReader(selectedSongs[i].FilePath);
                    }

                    string selectedFileType = "";
                    Dispatcher.Invoke(() =>
                    {
                        selectedFileType = OutputFileTypeComboBox.SelectedItem as string;
                        WaitingLabel.Content = $"Now converting {selectedSongs[i].Name} to {selectedFileType.Substring(selectedFileType.IndexOf("(") + 1, 4)}";
                    });
                    if (selectedFileType.EndsWith("(.mp3)"))
                    {
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{selectedSongs[i].Name}.mp3"))
                        using (WaveFileWriter writer = new WaveFileWriter(fileStream, inputStream.WaveFormat))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = 0;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                writer.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }

                        TagLib.File outputTags = TagLib.File.Create($"c:/temp/conversiontest/{selectedSongs[i].Name}.mp3");
                        TagLib.File inputTags = TagLib.File.Create(selectedSongs[i].FilePath);
                        outputTags.Tag.Album = inputTags.Tag.Album;
                        outputTags.Tag.AlbumArtists = inputTags.Tag.AlbumArtists;
                        outputTags.Tag.Genres = inputTags.Tag.Genres;
                        outputTags.Tag.Performers = inputTags.Tag.Performers;
                        outputTags.Tag.Year = inputTags.Tag.Year;
                        outputTags.Tag.Pictures = inputTags.Tag.Pictures;
                        outputTags.Tag.Title = inputTags.Tag.Title;
                        outputTags.Tag.Track = inputTags.Tag.Track;
                        outputTags.Save();
                        inputTags.Dispose();
                        outputTags.Dispose();
                    }
                    else if (selectedFileType.EndsWith("(.wav)"))
                    {
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{selectedSongs[i].Name}.wav"))
                        using (WaveFileWriter writer = new WaveFileWriter(fileStream, inputStream.WaveFormat))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = 0;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                writer.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }

                        TagLib.File outputTags = TagLib.File.Create($"c:/temp/conversiontest/{selectedSongs[i].Name}.wav");
                        TagLib.File inputTags = TagLib.File.Create(selectedSongs[i].FilePath);
                        outputTags.Tag.Album = inputTags.Tag.Album;
                        outputTags.Tag.AlbumArtists = inputTags.Tag.AlbumArtists;
                        outputTags.Tag.Genres = inputTags.Tag.Genres;
                        outputTags.Tag.Performers = inputTags.Tag.Performers;
                        outputTags.Tag.Year = inputTags.Tag.Year;
                        outputTags.Tag.Pictures = inputTags.Tag.Pictures;
                        outputTags.Tag.Title = inputTags.Tag.Title;
                        outputTags.Tag.Track = inputTags.Tag.Track;
                        outputTags.Save();
                        inputTags.Dispose();
                        outputTags.Dispose();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error converting " + selectedSongs[i].FilePath + ", message: " + e.Message);
                    await callBack();
                    inputStream?.Close();
                    return;
                }
                finally
                {
                    inputStream?.Close();
                }
            }
            await callBack();
            MessageBox.Show("File(s) successfully converted!");
        }

        private async Task ConvertCallBack()
        {
            Dispatcher.Invoke(() =>
            {
                WaitingLabel.Content = "Conversion Done";
                WaitingProgressBar.IsIndeterminate = false;
                WaitingProgressBar.Value = WaitingProgressBar.Maximum;
                Exportbutton.IsEnabled = true;
                selectedSongs.Clear();
            });
        }

    }
}
