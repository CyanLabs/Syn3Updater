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
using Cyanlabs.Updater.Common;
using Application = System.Windows.Application;

namespace Cyanlabs.Launcher
{
    /// <summary>
    ///     Checks for any update on the selected branch, if found downloads and extracts it
    /// </summary
    public class UpdateCheck
    {
        #region Properties & Fields  
        public UpgradingWindow UpgradingWindow;
        private UpgradingViewModel Vm => UpgradingWindow.Vm;
        #endregion

        #region Methods
        public async Task<bool> Execute(LauncherPrefs.ReleaseType releaseType, UpgradingWindow upgrading, string destFolder)
        {
            UpgradingWindow = upgrading;
            UpgradingWindow.Show();
            UpgradingWindow.UpdateLayout();
            UpgradingWindow.UpdateDefaultStyle();

            UpgradingWindow.Vm.Message = "Checking For Update...";
            await Task.Delay(1000);

            // Sets up Octokit API with a UserAgent and assigns latest to a new Release();
            Release githubrelease = null;
            GitHubClient githubclient = new GitHubClient(new ProductHeaderValue("CyanLabs-Launcher"));
            CIRelease ciRelease = null;
            // Attempt to get latest GitHub release
            try
            {
                // Get Correct Release
                switch (releaseType)
                {
                    // Continuous Intergration, 
                    case LauncherPrefs.ReleaseType.Alpha:
                        // Gets latest build via API at Cyanlabs.net
                        WebClient wc = new WebClient();
                        ciRelease = JsonConvert.DeserializeObject<CIRelease>(wc.DownloadString(new Uri("https://api.cyanlabs.net/ci/Syn3Updater/latest")));
                        break;

                    // Beta
                    case LauncherPrefs.ReleaseType.Beta:
                        // Get all GitHub releases for Syn3Updater and sets the value of 'latest' to the first (newest) retrieved
                        IReadOnlyList<Release> githubreleases = await githubclient.Repository.Release.GetAll("cyanlabs", "Syn3Updater");
                        githubrelease = githubreleases[0];
                        break;

                    // Release
                    case LauncherPrefs.ReleaseType.Stable:
                        // Get all GitHub releases for Syn3Updater that aren't marked as 'prerelease' and sets the value of 'latest' to the first (newest) retrieved
                        githubrelease = await githubclient.Repository.Release.GetLatest("cyanlabs", "Syn3Updater");
                        break;
                } // End of Switch 'Get Correct Release'
            }

            // Catch RateLimitExceededException exception, bypasses update check and launches Syn3Updater.exe
            catch (RateLimitExceededException)
            {
                if (File.Exists("Syn3Updater.exe")) Process.Start("Syn3Updater.exe", "/launcher");
                Application.Current.Shutdown();
                return false;
            }

            string version;
            if (releaseType == LauncherPrefs.ReleaseType.Alpha)
            {
                version = ciRelease.Number;
            }
            else
            {
                version = githubrelease.TagName;
                // Use GitHub release tagname as version and parse in to an integer
            }

            if (!Core.LauncherPrefs.ReleaseInstalled.Contains(".")) Core.LauncherPrefs.ReleaseInstalled = "0.0.0.0";
            // Current version is less than new version OR current branch is different to new branch OR Syn3Updater.exe is missing
            if (Version.Parse(Core.LauncherPrefs.ReleaseInstalled) < Version.Parse(version) || Core.LauncherPrefs.ReleaseTypeInstalled != releaseType || !File.Exists(destFolder + "\\Syn3Updater.exe"))
            {
                Vm.Message = "Installing " + releaseType + " release " + version;

                string zipPath = destFolder + "\\" + releaseType + "_" + version + ".zip";

                WebClient wc = new WebClient();

                // Hook WebClient DownloadProgressChanged handler
                wc.DownloadProgressChanged += client_DownloadProgressChanged;

                // Do the actual file download of the first Asset in the chosen GitHub Release or the CI download link with the previously created WebClient
                await wc.DownloadFileTaskAsync(
                     releaseType == LauncherPrefs.ReleaseType.Alpha
                         ? new Uri(ciRelease.Download)
                         : new Uri(githubrelease.Assets.First(x => x.ContentType == "application/x-zip-compressed").BrowserDownloadUrl), zipPath);

                if (Directory.Exists(destFolder + "\\temp"))
                    Directory.Delete(destFolder + "\\temp", true);
                Directory.CreateDirectory(destFolder + "\\temp");

                string archivePath = destFolder + "\\Launcher_OldVersion.exe";

                if (File.Exists(archivePath))
                    File.Delete(archivePath);
                File.Move(destFolder + "\\Launcher.exe", archivePath);

                Vm.Message = "Extracting...";
                ZipFile.ExtractToDirectory(zipPath, destFolder + "\\temp");
                DirectoryCopy(destFolder + "\\temp", destFolder, true);
                File.Delete(zipPath);

                Directory.Delete(destFolder + "\\temp", true);

                await UpgradingWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpgradingWindow.Hide();
                    UpgradingWindow.Close();
                }));

                // Update settings to match new release version and branch
                Core.LauncherPrefs.ReleaseInstalled = version;
                Core.LauncherPrefs.ReleaseTypeInstalled = releaseType;

                // Save settings to json file, create if doesn't already exist.
                string configFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\CyanLabs\\Syn3Updater";
                if (!Directory.Exists(configFolderPath))
                    Directory.CreateDirectory(configFolderPath);
                File.WriteAllText(configFolderPath + "\\LauncherPrefs.json", JsonConvert.SerializeObject(Core.LauncherPrefs));
                return true;
            }
            return false;
            // end of If 'current version is less than new version OR current branch is different to new branch OR Syn3Updater.exe is missing'
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

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!string.IsNullOrWhiteSpace(destDirName) && !Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            foreach (FileInfo file in dir.GetFiles())
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

            Vm.Message = $"Downloaded {e.BytesReceived / 1000000}  MB of {e.TotalBytesToReceive / 1000000} MB.";

            Vm.Percentage = 100 - (int)percentage;
        }
        #endregion

    }

    // ReSharper disable once InconsistentNaming
    public class CIRelease
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("number")] public string Number { get; set; }
        [JsonProperty("branchName")] public string BranchName { get; set; }
        [JsonProperty("finishOnAgentDate")] public string FinishOnAgentDate { get; set; }
        [JsonProperty("download")] public string Download { get; set; }
    }
}