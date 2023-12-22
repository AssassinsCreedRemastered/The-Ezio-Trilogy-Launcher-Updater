using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpCompress.Common;
using SharpCompress.Archives;

namespace The_Ezio_Trilogy_Launcher_Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\Assassin's Creed - The Ezio Trilogy Remastered\";
        public string url = "https://github.com/AssassinsCreedRemastered/The-Ezio-Trilogy-Launcher/releases/latest/download/Launcher.zip";

        private async Task DeleteOldLauncher()
        {
            try
            {
                if (File.Exists(path + @"\The Ezio Trilogy Launcher.exe"))
                {
                    File.Delete(path + @"\The Ezio Trilogy Launcher.exe");
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task DownloadNewVersion()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(path + @"\Launcher.zip", FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var totalBytes = response.Content.Headers.ContentLength ?? -1;
                            var buffer = new byte[8192];
                            var bytesRead = 0;
                            var totalRead = 0;
                            do
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                                    totalRead += bytesRead;

                                    // Calculate progress percentage
                                    var progressPercentage = totalBytes == -1 ? 0 : (int)((double)totalRead / totalBytes * 100);
                                    Progress.Value = progressPercentage;
                                }
                            } while (bytesRead > 0);
                        }
                    }
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }


        private async Task Installation()
        {
            try
            {
                if (!Directory.Exists(path + @"\Update"))
                {
                    Directory.CreateDirectory(path + @"\Update");
                }
                await Extract(path + @"\Launcher.zip", path + @"\Update");
                await Move();
                await Cleanup();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        // Used to extract files
        private async Task Extract(string fullPath, string directory)
        {
            try
            {
                using (var archive = ArchiveFactory.Open(fullPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(directory, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                GC.Collect();
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task Move()
        {
            try
            {
                if (Directory.Exists(path + @"\Update"))
                {
                    foreach (string file in Directory.GetFiles(path + @"\Update"))
                    {
                        if (System.IO.Path.GetFileName(file) != "Assassins Creed Remastered Launcher Updater.exe")
                        {
                            System.IO.File.Move(file, path + @"\" + System.IO.Path.GetFileName(file), true);
                        }
                    }
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task Cleanup()
        {
            try
            {
                if (File.Exists(path + @"\Launcher.zip"))
                {
                    File.Delete(path + @"\Launcher.zip");
                }
                if (Directory.Exists(path + @"\Update"))
                {
                    Directory.Delete(path + @"\Update", true);
                }

                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DeleteOldLauncher();
            await DownloadNewVersion();
            await Installation();
            OpenLauncher.IsEnabled = true;
        }

        private void OpenLauncher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process Launcher = new Process();
                Launcher.StartInfo.WorkingDirectory = path;
                Launcher.StartInfo.FileName = "Assassins Creed Remastered Launcher.exe";
                Launcher.StartInfo.UseShellExecute = true;
                Launcher.Start();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}