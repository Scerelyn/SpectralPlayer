using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace SpectralPlayerApp.Dialogs
{
    /// <summary>
    /// Interaction logic for ConvertFileDialog.xaml
    /// </summary>
    public partial class ConvertFileDialog : Window
    {
        private OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect=true };
        private string[] filePaths = new string[0];
        private string[] safeFileNames = new string[0];

        public ConvertFileDialog()
        {
            InitializeComponent();
            List<string> Filetypes = new List<string>() {
                "Windows Media Audio (.wma)",
                "Waveform Audio File Format (.wav)",
                "MPEG Audio Layer III (.mp3)",
            };
            OutputFileTypeComboBox.ItemsSource = Filetypes;
            OutputFileTypeComboBox.SelectedIndex = 0;

        }

        public async void DoFileConvert(object sender, RoutedEventArgs args)
        {
            if (filePaths.Count() <= 0)
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
                await Task.Factory.StartNew( async () => {
                    await ConvertFiles(ConvertCallBack);
                });
            }
        }

        private async Task ConvertFiles(Func<Task> callBack)
        {
            await Task.Delay(250); // give time for rendering changes
            WaveStream inputStream = null;
            for (int i = 0; i < filePaths.Count(); i++)
            {
                try
                {
                    if (filePaths[i].EndsWith(".ogg"))
                    {
                        inputStream = new VorbisWaveReader(filePaths[i]);
                    }
                    else if (filePaths[i].EndsWith(".flac"))
                    {
                        inputStream = new FlacReader(filePaths[i]);
                    }
                    else
                    {
                        inputStream = new AudioFileReader(filePaths[i]);
                    }

                    string selectedFileType = "";
                    Dispatcher.Invoke(() => 
                    {
                        selectedFileType = OutputFileTypeComboBox.SelectedItem as string;
                        WaitingLabel.Content = $"Now converting {safeFileNames[i]} to {selectedFileType.Substring(selectedFileType.IndexOf("(")+1,4)}";
                    });  
                    if (selectedFileType.EndsWith("(.mp3)"))
                    {
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{safeFileNames[i]}.mp3"))
                        using (NAudio.Lame.LameMP3FileWriter writer = new NAudio.Lame.LameMP3FileWriter(fileStream, inputStream.WaveFormat, 320000))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = 0;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                writer.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }
                    }
                    //else if (selectedFileType.EndsWith("(.wma)"))
                    //{
                    //    FileStream fileStream = File.Create($"c:/temp/conversiontest/{safeFileName}.wma");
                    //    fileStream.Close();
                    //    using (MediaFoundationEncoder writer = new MediaFoundationEncoder(new NAudio.MediaFoundation.MediaType(inputStream.WaveFormat)))
                    //    {
                    //        writer.Encode($"c:/temp/conversiontest/{safeFileName}.wma", inputStream);
                    //    }
                    //}
                    else if (selectedFileType.EndsWith("(.wav)"))
                    {
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{safeFileNames[i]}.wav"))
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
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error converting " + filePaths[i] + ", message: " + e.Message);
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
            });
        }

        public void DoFileSelect(object sender, RoutedEventArgs args)
        {
            bool? result = openFileDialog.ShowDialog();
            if (result ?? false)
            {
                filePaths = openFileDialog.FileNames;
                safeFileNames = openFileDialog.SafeFileNames;
                FileLabel.Content = "";
                foreach (string s in filePaths)
                {
                    FileLabel.Content = FileLabel.Content + s + ", ";
                }
            }
        }
    }
}
