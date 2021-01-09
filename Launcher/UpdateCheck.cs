using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using SharedCode;
using File = System.IO.File;

namespace Launcher
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
            
            UpgradingWindow.vm.Message = "Checking For Update...";
            await Task.Delay(1000);
            Release latest = new Release();
            var githubclient = new GitHubClient(new ProductHeaderValue("CyanLabs-Launcher"));
            var tokenAuth = new Credentials("fma965", "hjuBpZko7BUAFeA2NnJsuhO6");
            githubclient.Credentials = tokenAuth;

            //  try
            {
                switch (releaseType)
                {
                    case LauncherPrefs.ReleaseType.CI:
                        //Soon
                        throw new NotImplementedException();

                    case LauncherPrefs.ReleaseType.Beta:
                        var githubreleases = await githubclient.Repository.Release.GetAll("cyanlabs", "Syn3UpdaterUpdateTest");
                        latest = githubreleases[0];
                        break;

                    case LauncherPrefs.ReleaseType.Release:
                        latest = await githubclient.Repository.Release.GetLatest("cyanlabs", "Syn3UpdaterUpdateTest");
                        break;

                }

                string version = new String(latest.TagName.Where(Char.IsDigit).ToArray());
                int intversion = Int32.Parse(version);
                Console.WriteLine("The latest release is tagged at {0} and is named {1}", latest.TagName, latest.Name);
                int maxReleaseNumber = intversion;

                if (Core.LauncherPrefs.ReleaseInstalled < maxReleaseNumber || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType )
                {
                    
                    try
                    {
                        if (File.Exists(destFolder+"\\Syn3Updater.exe"))
                        {
                            if (!Directory.Exists(destFolder+"\\.old"))
                            {
                                DirectoryInfo dir = Directory.CreateDirectory(destFolder+"\\old");
                                
                                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                            }

                            File.Move("Syn3Updater.exe", destFolder+"\\.old\\oldsyn3updater_" +Guid.NewGuid()+".exe");
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (Directory.Exists(".old"))
                    {
                        Thread.Sleep(1000);
                        foreach (var f in Directory.GetFiles(destFolder+"\\.old\\"))
                        {
                            try
                            {
                                File.Delete(f);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    if (Directory.Exists(destFolder+"\\.old"))
                    {
                        Thread.Sleep(1000);

                        if (Directory.GetFiles(destFolder+"\\.old\\").Length == 0)
                        {
                            Directory.Delete(destFolder+"\\.old", true);
                        }
                    }

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

                        string json = JsonConvert.SerializeObject(Core.LauncherPrefs);
                        File.WriteAllText(destFolder+"\\LauncherPrefs.json", json);


                        if (Directory.Exists(destFolder+"\\.old"))
                        {
                            Thread.Sleep(1000);

                            if (Directory.GetFiles(destFolder+"\\.old\\").Length == 0)
                            {
                                Directory.Delete(destFolder+"\\.old", true);
                            }
                        }

                            
                    };

                    wc.DownloadFileAsync(new Uri(latest.Assets.First(x => x.ContentType == "application/x-zip-compressed").BrowserDownloadUrl), zipPath);
                }
                else
                {
                    Complete = true;
                }
            }
            //catch (Exception e)
            //{
            //    Complete = true;
            //}

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

            Vm.Message = "Downloaded " + (e.BytesReceived / 1000000).ToString() + " MB of " + (e.TotalBytesToReceive / 1000000).ToString() + " MB.";

            Vm.Percentage = 100-(int)percentage;
        }

        private UpgradingViewModel Vm => UpgradingWindow.vm;
    }
}
