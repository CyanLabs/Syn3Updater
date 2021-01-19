using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using SharedCode;
using Application = System.Windows.Application;

namespace Cyanlabs.Launcher
{
    /// <summary>
    ///     Checks for any update on the selected branch, if found downloads and extracts it
    /// </summary>
    public class UpdateCheck
    {

        #region Properties & Fields
        public bool Complete;
        public UpgradingWindow UpgradingWindow;
        private UpgradingViewModel Vm => UpgradingWindow.Vm;
        #endregion

        #region Methods
        public async Task Execute(LauncherPrefs.ReleaseType releaseType, UpgradingWindow upgrading, string destFolder)
        {
            UpgradingWindow = upgrading;
            UpgradingWindow.Show();
            UpgradingWindow.UpdateLayout();
            UpgradingWindow.UpdateDefaultStyle();

            UpgradingWindow.Vm.Message = "Checking For Update...";
            await Task.Delay(1000);

            // Sets up Octokit API with a UserAgent and assigns latest to a new Release();
            Release latest = new Release();
            GitHubClient githubclient = new GitHubClient(new ProductHeaderValue("CyanLabs-Launcher"));

            // Attempt to get latest GitHub release
            try
            {
                // Get Correct Release
                switch (releaseType)
                {
                    // Continuous Intergration - Not implemented yet
                    case LauncherPrefs.ReleaseType.Ci:
                        throw new NotImplementedException();

                    // Beta
                    case LauncherPrefs.ReleaseType.Beta:
                        // Get all GitHub releases for Syn3Updater and sets the value of 'latest' to the first (newest) retrieved
                        IReadOnlyList<Release> githubreleases = await githubclient.Repository.Release.GetAll("cyanlabs", "Syn3Updater");
                        latest = githubreleases[0];
                        break;

                    // Release
                    case LauncherPrefs.ReleaseType.Release:
                        // Get all GitHub releases for Syn3Updater that aren't marked as 'prerelease' and sets the value of 'latest' to the first (newest) retrieved
                        latest = await githubclient.Repository.Release.GetLatest("cyanlabs", "Syn3Updater");
                        break;
                } // End of Switch 'Get Correct Release'
            }

            // Catch RateLimitExceededException exception, bypasses update check and launches Syn3Updater.exe
            catch (RateLimitExceededException e)
            {
                if (File.Exists("Syn3Updater.exe")) Process.Start("Syn3Updater.exe", "/launcher");
                Application.Current.Shutdown();
                return;
            }

            // Use GitHub release tagname as version and parse in to an integer
            string version = new string(latest.TagName.Where(char.IsDigit).ToArray());
            int maxReleaseNumber = int.Parse(version);

            // Current version is less than new version OR current branch is different to new branch OR Syn3Updater.exe is missing
            if (Core.LauncherPrefs.ReleaseInstalled < maxReleaseNumber || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType || !File.Exists(destFolder + "\\Syn3Updater.exe"))
            {
                Vm.Message = "Installing " + releaseType + " release " + latest.TagName;

                string zipPath = destFolder + "\\" + releaseType + "_" + latest.TagName + ".zip";

                WebClient wc = new WebClient();

                // Hook  WebClient DownloadProgressChanged handler
                wc.DownloadProgressChanged += client_DownloadProgressChanged;

                // Setup WebClient DownloadFileCompleted handler
                wc.DownloadFileCompleted += (sender, e) =>
                {
                    if (Directory.Exists(destFolder + "\\temp")) Directory.Delete(destFolder + "\\temp", true);
                    Directory.CreateDirectory(destFolder + "\\temp");

                    string archivePath = destFolder + "\\Launcher_OldVersion.exe";

                    if (File.Exists(archivePath)) File.Delete(archivePath);
                    File.Move(destFolder + "\\Launcher.exe", archivePath);

                    Vm.Message = "Extracting...";
                    ZipFile.ExtractToDirectory(zipPath, destFolder + "\\temp");
                    DirectoryCopy(destFolder + "\\temp", destFolder, true);
                    File.Delete(zipPath);

                    // Attempt to delete temp folder, if failed do nothing
                    try
                    {
                        Directory.Delete(destFolder + "\\temp", true);
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

                    // Update settings to match new release version and branch
                    Core.LauncherPrefs.ReleaseInstalled = maxReleaseNumber;
                    Core.LauncherPrefs.ReleaseTypeInstalled = releaseType;

                    // Save settings to json file, create if doesn't already exist.
                    string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
                    string json = JsonConvert.SerializeObject(Core.LauncherPrefs);
                    if (!Directory.Exists(configFolderPath)) Directory.CreateDirectory(configFolderPath);
                    File.WriteAllText(configFolderPath + "\\LauncherPrefs.json", json);
                };

                // Do the actual file download of the first Asset in the chosen GitHub Release with the previously created WebClient
                wc.DownloadFileAsync(new Uri(latest.Assets.First(x => x.ContentType == "application/x-zip-compressed").BrowserDownloadUrl), zipPath);
            }
            else
            {
                Complete = true;
            } // end of If 'current version is less than new version OR current branch is different to new branch OR Syn3Updater.exe is missing'
        }

        /// <summary>
        ///     Copy directory from one directory to another directory, optionally including subdirectories.
        /// </summary>
        /// <param name="sourceDirName">Source directory</param>
        /// <param name="destDirName">Destination directory</param>
        /// <param name="copySubDirs">Copy subdirectories?</param>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!string.IsNullOrWhiteSpace(destDirName) && !Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                string temppath = Path.Combine(destDirName, file.Name);
                Debug.WriteLine("Copying to " + temppath);
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
                foreach (DirectoryInfo subdir in dirs)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, true);
                }
        }

        /// <summary>
        ///     Method to handle DownloadProgressChanged and update UI with relevant information
        /// </summary>
        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            Vm.Message = "Downloaded " + e.BytesReceived / 1000000 + " MB of " + e.TotalBytesToReceive / 1000000 + " MB.";

            Vm.Percentage = 100 - (int)percentage;
        }
        #endregion

    }
}