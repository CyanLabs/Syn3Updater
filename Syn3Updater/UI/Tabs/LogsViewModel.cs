using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using MessageBox = ModernWpf.MessageBox;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class LogsViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors
        private AsyncCommand<string> _openLogFile;
        public AsyncCommand<string> OpenLogFile => _openLogFile ??= new AsyncCommand<string>(OpenLogFileAction);
        
        private AsyncCommand _deleteLogFiles;
        public AsyncCommand DeleteLogFiles => _deleteLogFiles ??= new AsyncCommand(DeleteLogFilesAction);
        
        private AsyncCommand _openLogDirectory;
        public AsyncCommand OpenLogDirectory => _openLogDirectory ??= new AsyncCommand(OpenLogDirectoryAction);
        
        #endregion

        #region Properties & Fields
        
        private string _logLocation;
        public string LogLocation
        {
            get => _logLocation;
            set => SetProperty(ref _logLocation, value);
        }
        
        private ObservableCollection<LogModel.Log> _logFiles;

        public ObservableCollection<LogModel.Log> LogFiles
        {
            get => _logFiles;
            set => SetProperty(ref _logFiles, value);
        }
        
        private LogModel.Log _selectedLogFile;

        public LogModel.Log SelectedLogFile
        {
            get => _selectedLogFile;
            set => SetProperty(ref _selectedLogFile, value);
        }

        private string _logTitle;
        public string LogTitle
        {
            get => _logTitle;
            set => SetProperty(ref _logTitle, value);
        }
        
        private string _logDetails;
        public string LogDetails
        {
            get => _logDetails;
            set => SetProperty(ref _logDetails, value);
        }
        
        #endregion

        #region Methods

        public void Reload()
        {
            LogLocation = AppMan.App.MainSettings.LogPath;
            Task.Run(ReloadLogs);
        }
        
        private async Task OpenLogFileAction(string path)
        {
            try
            {
                DateTime date = File.GetLastWriteTime(AppMan.App.MainSettings.LogPath + path);
                LogTitle = date.ToString(CultureInfo.CurrentCulture);
                LogDetails = File.ReadAllText(AppMan.App.MainSettings.LogPath + path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        
        private async Task DeleteLogFilesAction()
        {
            foreach (LogModel.Log log in LogFiles)
            {
                if (!log.Selected) continue;
                try
                {
                    File.Delete(AppMan.App.MainSettings.LogPath + log.FileName);
                }
                catch
                {
                    // ignored
                }
            }
            await Task.Run(ReloadLogs);
        }

        private async Task ReloadLogs()
        {
            LogDetails = null;
            LogFiles = new ObservableCollection<LogModel.Log>();
            if (!string.IsNullOrEmpty(AppMan.App.MainSettings.LogPath))
            {
                DirectoryInfo dir = new(AppMan.App.MainSettings.LogPath);
                foreach (FileInfo file in dir.GetFiles("*.txt"))
                {
                    LogFiles.Add(new LogModel.Log {Selected=false, FileName=file.Name, Date=file.LastWriteTime, Profile = "Default"});
                }
            }
        }

        private async Task OpenLogDirectoryAction()
        {
            try
            {
                Process.Start(AppMan.App.MainSettings.LogPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}