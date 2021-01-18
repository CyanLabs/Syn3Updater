using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Octokit;
using SharedCode;
using Application = System.Windows.Application;

namespace Cyanlabs.Launcher
{
    public class UpdateCheck
    {
        public bool Complete;
        public UpgradingWindow UpgradingWindow;
        public async Task Execute(LauncherPrefs.ReleaseType releaseType, UpgradingWindow upgrading, string destFolder)
        {
            UpgradingWindow = upgrading;
            UpgradingWindow.Show();
            UpgradingWindow.UpdateLayout();
            UpgradingWindow.UpdateDefaultStyle();
            
            UpgradingWindow.Vm.Message = "Checking For Update...";
            await Task.Delay(1000);
            Release latest = new Release();
            var githubclient = new GitHubClient(new ProductHeaderValue("CyanLabs-Launcher"));

            //  try
            {
                switch (releaseType)
                {
                    case LauncherPrefs.ReleaseType.Ci:
                        //Soon
                        throw new NotImplementedException();

                    case LauncherPrefs.ReleaseType.Beta:
                        try
                        {
                            var githubreleases = await githubclient.Repository.Release.GetAll("cyanlabs", "Syn3Updater");
                            latest = githubreleases[0];
                        }
                        catch (RateLimitExceededException e)
                        {
                            if (File.Exists("Syn3Updater.exe"))
                            {
                                Process.Start("Syn3Updater.exe", "/launcher");
                            }
                            MessageBox.Show(e.Message);
                            Application.Current.Shutdown();
                            return;
                        }
                        break;

                    case LauncherPrefs.ReleaseType.Release:
                        try
                        {
                            latest = await githubclient.Repository.Release.GetLatest("cyanlabs", "Syn3Updater");
                        }
                        catch (RateLimitExceededException e)
                        {
                            if (File.Exists("Syn3Updater.exe"))
                            {
                                Process.Start("Syn3Updater.exe", "/launcher");
                            }
                            MessageBox.Show(e.Message);
                            Application.Current.Shutdown();
                            return;
                        }
                        break;

                }

                string version = new String(latest.TagName.Where(Char.IsDigit).ToArray());
                int intversion = Int32.Parse(version);
                Console.WriteLine("The latest release is tagged at {0} and is named {1}", latest.TagName, latest.Name);
                int maxReleaseNumber = intversion;

                if (Core.LauncherPrefs.ReleaseInstalled < maxReleaseNumber || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType  || !File.Exists(destFolder + "\\Syn3Updater.exe") )
                {
                    Vm.Message = "Installing " + releaseType + " release " + latest.TagName;

                    string zipPath = destFolder+"\\"+releaseType + "_" + latest.TagName + ".zip";

                    WebClient wc = new WebClient();
                    wc.DownloadProgressChanged += client_DownloadProgressChanged;
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        if (Directory.Exists(destFolder+"\\temp"))
                        {
                            Directory.Delete(destFolder+"\\temp", true);
                        }

                        Directory.CreateDirectory(destFolder+"\\temp");

                        string archivePath = destFolder + "\\Launcher_OldVersion.exe";
                        
                        if (File.Exists(archivePath)) File.Delete(archivePath);
                        File.Move(destFolder + "\\Launcher.exe", archivePath);

                        Vm.Message = "Extracting...";
                        ZipFile.ExtractToDirectory(zipPath, destFolder+"\\temp");
                        DirectoryCopy(destFolder+"\\temp", destFolder, true);
                        File.Delete(zipPath);

                        try
                        {
                            Directory.Delete(destFolder+"\\temp", true);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        UpgradingWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UpgradingWindow.Hide();
                            UpgradingWindow.Close();
                        }));

                        Complete = true;

                        Core.LauncherPrefs.ReleaseInstalled = maxReleaseNumber;
                        Core.LauncherPrefs.ReleaseTypeInstalled = releaseType;

                        string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
                        string json = JsonConvert.SerializeObject(Core.LauncherPrefs);
                        if (!Directory.Exists(configFolderPath))
                        {
                            Directory.CreateDirectory(configFolderPath);
                        }
                        File.WriteAllText(configFolderPath + "\\LauncherPrefs.json", json);
                    };

                    wc.DownloadFileAsync(new Uri(latest.Assets.First(x => x.ContentType == "application/x-zip-compressed").BrowserDownloadUrl), zipPath);
                }
                else
                {
                    Complete = true;
                }
            }

        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!string.IsNullOrWhiteSpace(destDirName) && !Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                string temppath = Path.Combine(destDirName, file.Name);
                Debug.WriteLine("Copying to "+temppath);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch
                {
                    // ignored
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, true);
                }
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            Vm.Message = "Downloaded " + (e.BytesReceived / 1000000) + " MB of " + (e.TotalBytesToReceive / 1000000) + " MB.";

            Vm.Percentage = 100-(int)percentage;
        }

        private UpgradingViewModel Vm => UpgradingWindow.Vm;
    }
}
