using Microsoft.Win32;
using NAudio.Flac;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ookii.Dialogs.Wpf;
using SpectralPlayerApp.UIComponents;
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
        private OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Multiselect = true,
            Filter = "Audio (*.mp3;*.wav;*.flac;*.ogg;*.aac;*.m4a;*.wma)|*.mp3;*.wav;*.flac;*.ogg;*.aac;*.m4a;*.wma"
        };
        private VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();

        private string outputPath;
        private string[] filePaths = new string[0];
        private string[] safeFileNames = new string[0];

        private MainWindow parentWindow = null;

        public ConvertFileDialog(MainWindow parentWindow=null)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            List<string> Filetypes = new List<string>() {
                //"Windows Media Audio (.wma)",
                "Waveform Audio File Format (.wav)",
                "MPEG Audio Layer III (.mp3)",
            };
            OutputFileTypeComboBox.ItemsSource = Filetypes;
            OutputFileTypeComboBox.SelectedIndex = 0;

            List<string> MSTypes = new List<string>()
            {
                "Leave as is",
                "Mono",
                "Stereo",
            };
            MSConvertComboBox.ItemsSource = MSTypes;
            MSConvertComboBox.SelectedIndex = 0;
        }

        public async void DoFileConvert(object sender, RoutedEventArgs args)
        {
            if (filePaths.Count() <= 0)
            {
                MessageBox.Show("Select files first!");
            }
            else if (!Directory.Exists(outputPath))
            {
                MessageBox.Show("Select a valid folder for output");
            }
            else
            {
                WaitingProgressBar.Value = WaitingProgressBar.Minimum;
                WaitingProgressBar.IsIndeterminate = true;
                WaitingProgressBar.Visibility = Visibility.Visible;
                WaitingLabel.Visibility = Visibility.Visible;
                WaitingLabel.Content = "Converting files, hang on...";
                BackgroundTaskControl bgtc = null;
                if (parentWindow != null)
                {
                    bgtc = new BackgroundTaskControl(parentWindow.BackgroundTaskStackPanel, WaitingLabel.Content + ""); ;
                }

                Exportbutton.IsEnabled = false;
                await Task.Factory.StartNew( async () => {
                    await ConvertFiles(ConvertCallBack, bgtc);
                });
            }
        }

        private async Task ConvertFiles(Func<BackgroundTaskControl, Task> callBack, BackgroundTaskControl bgtc)
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
                        if(bgtc != null)
                        {
                            bgtc.BackgroundTaskLabel.Content = WaitingLabel.Content;
                        }
                    });  
                    if (selectedFileType.EndsWith("(.mp3)"))
                    {
                        string monoStereoSelection = "";
                        Dispatcher.Invoke(() =>
                        {
                            monoStereoSelection = MSConvertComboBox.SelectedItem as string;
                        });
                        if (inputStream.WaveFormat.BitsPerSample == 16)
                        {
                            IWaveProvider provider = MonoStereoConvert16(inputStream, monoStereoSelection == "Mono");
                            using (FileStream fileStream = File.Create($"{outputPath}/{safeFileNames[i]}.mp3"))
                            using (NAudio.Lame.LameMP3FileWriter writer = new NAudio.Lame.LameMP3FileWriter(fileStream, provider.WaveFormat, 320000))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = 0;
                                do
                                {
                                    bytesRead = provider.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, bytesRead);
                                } while (bytesRead > 0);
                            }
                        }
                        else if(inputStream.WaveFormat.BitsPerSample == 32)
                        {
                            ISampleProvider provider = MonoStereoConvert32(inputStream as ISampleProvider, monoStereoSelection == "Mono");
                            using (FileStream fileStream = File.Create($"{outputPath}/{safeFileNames[i]}.mp3"))
                            using (NAudio.Lame.LameMP3FileWriter writer = new NAudio.Lame.LameMP3FileWriter(fileStream, provider.WaveFormat, 320000))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = 0;
                                do
                                {
                                    var providerAsWave = provider.ToWaveProvider(); // need to write with bytes not floats
                                    bytesRead = providerAsWave.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, bytesRead);
                                } while (bytesRead > 0);
                            }
                        }
                        
                        
                        TagLib.File outputTags = TagLib.File.Create($"{outputPath}//{safeFileNames[i]}.mp3");
                        TagLib.File inputTags = TagLib.File.Create(filePaths[i]);
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
                        string monoStereoSelection = "";
                        Dispatcher.Invoke(() => 
                        {
                            monoStereoSelection = MSConvertComboBox.SelectedItem as string;
                        });
                        if (inputStream.WaveFormat.BitsPerSample == 16)
                        {
                            IWaveProvider provider = MonoStereoConvert16(inputStream, monoStereoSelection == "Mono");
                            using (FileStream fileStream = File.Create($"{outputPath}/{safeFileNames[i]}.wav"))
                            using (WaveFileWriter writer = new WaveFileWriter(fileStream, provider.WaveFormat))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = 0;
                                do
                                {
                                    bytesRead = provider.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, bytesRead);
                                } while (bytesRead > 0);
                            }
                        }
                        else if (inputStream.WaveFormat.BitsPerSample == 32)
                        {
                            ISampleProvider provider = MonoStereoConvert32(inputStream as ISampleProvider, monoStereoSelection == "Mono");
                            using (FileStream fileStream = File.Create($"{outputPath}/{safeFileNames[i]}.wav"))
                            using (WaveFileWriter writer = new WaveFileWriter(fileStream, provider.WaveFormat))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead = 0;
                                do
                                {
                                    var providerAsWave = provider.ToWaveProvider(); // need to write with bytes not floats
                                    bytesRead = providerAsWave.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, bytesRead);
                                } while (bytesRead > 0);
                            }
                        }
                        TagLib.File outputTags = TagLib.File.Create($"{outputPath}/{safeFileNames[i]}.wav");
                        TagLib.File inputTags = TagLib.File.Create(filePaths[i]);
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
                    MessageBox.Show("Error converting " + filePaths[i] + ", message: " + e.Message);
                    //just move onto the next song
                }
                finally
                {
                    (inputStream as WaveStream)?.Close();
                }
            }
            await callBack(bgtc);
            MessageBox.Show("File(s) successfully converted!");
        }

        private async Task ConvertCallBack(BackgroundTaskControl bgtc)
        {
            Dispatcher.Invoke(() => 
            {
                WaitingLabel.Content = "Conversion Done";
                WaitingProgressBar.IsIndeterminate = false;
                WaitingProgressBar.Value = WaitingProgressBar.Maximum;
                Exportbutton.IsEnabled = true;
                if (parentWindow != null)
                {
                    bgtc?.Dispose();
                }
            });
        }

        /// <summary>
        /// Converts the given input wavestream into mono or stereo for 16 bit samples
        /// </summary>
        /// <param name="input">The input WaveProvider to convert</param>
        /// <param name="toMono">True for mono output, or false for stereo output</param>
        /// <returns>A converted IWaveProvider of the original input in either mono or stereo</returns>
        public IWaveProvider MonoStereoConvert16(IWaveProvider input, bool toMono)
        {
            if (toMono && input.WaveFormat.Channels != 1)
            {
                var stmp = new StereoToMonoProvider16(input);
                return stmp;
            }
            else if (!toMono && input.WaveFormat.Channels != 2)
            {
                var mtsp = new MonoToStereoProvider16(input);
                mtsp.LeftVolume = 0.7f;
                mtsp.RightVolume = 0.7f; //0.7 on each to avoid double loud
                return mtsp;
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Converts between Stereo and Mono SampleProviders for 32 bit sampled audio
        /// </summary>
        /// <param name="input">The input 32 bit SampleProvider</param>
        /// <param name="toMono">True for mono audio, falses for stereo</param>
        /// <returns></returns>
        public ISampleProvider MonoStereoConvert32(ISampleProvider input, bool toMono)
        {
            if (toMono && input.WaveFormat.Channels != 1)
            {
                var stmp = new StereoToMonoSampleProvider(input);
                return stmp;
            }
            else if (!toMono && input.WaveFormat.Channels != 2)
            {
                var mtsp = new MonoToStereoSampleProvider(input);
                mtsp.LeftVolume = 0.7f;
                mtsp.RightVolume = 0.7f; //0.7 on each to avoid double loud
                return mtsp;
            }
            else
            {
                return input;
            }
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

        public void DoSelectOutputFolder(object sender, RoutedEventArgs args)
        {
            bool? result = folderBrowserDialog.ShowDialog();
            if (result ?? false)
            {
                outputPath = folderBrowserDialog.SelectedPath;
                OutputFolderLabel.Content = outputPath;
            }
        }
    }
}
