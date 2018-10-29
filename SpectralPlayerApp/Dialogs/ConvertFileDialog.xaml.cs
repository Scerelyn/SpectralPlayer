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
        private OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect=false };
        private string filePath = "";
        private string safeFileName = "";

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

        public void DoFileConvert(object sender, RoutedEventArgs args)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Select a file first!");
            }
            else
            {
                WaveStream inputStream = null;
                try
                {
                    if (filePath.EndsWith(".ogg"))
                    {
                        inputStream = new VorbisWaveReader(filePath);
                    }
                    else if (filePath.EndsWith(".flac"))
                    {
                        inputStream = new FlacReader(filePath);
                    }
                    else
                    {
                        inputStream = new AudioFileReader(filePath);
                    }
                    string selectedFileType = OutputFileTypeComboBox.SelectedItem as string;
                    if (selectedFileType.EndsWith("(.mp3)"))
                    {
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{safeFileName}.mp3"))
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
                        using (FileStream fileStream = File.Create($"c:/temp/conversiontest/{safeFileName}.wav"))
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
                catch(Exception e)
                {
                    MessageBox.Show("Error occured: " + e.Message);
                }
                finally
                {
                    inputStream?.Close();
                }
            }
        }

        public void DoFileSelect(object sender, RoutedEventArgs args)
        {
            bool? result = openFileDialog.ShowDialog();
            if (result ?? false)
            {
                filePath = openFileDialog.FileName;
                safeFileName = openFileDialog.SafeFileName;
                FileLabel.Content = filePath;
            }
        }
    }
}
