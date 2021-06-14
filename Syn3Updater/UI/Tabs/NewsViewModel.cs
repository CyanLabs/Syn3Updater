using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using Nito.AsyncEx;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class NewsViewModel : LanguageAwareBaseViewModel
    {
        #region Properties & Fields

        private AsyncCommand _reloadNotices;
        public AsyncCommand ReloadNotices => _reloadNotices ??= new AsyncCommand(ReloadNoticesAction);

        private AsyncCommand _reloadChangelog;
        public AsyncCommand ReloadChangelog => _reloadChangelog ??= new AsyncCommand(ReloadChangelogAction);

        private AsyncCommand<string> _visitGithub;
        public AsyncCommand<string> VisitGithub => _visitGithub ??= new AsyncCommand<string>(VisitGithubAction);

        private string _importantNotices;

        public string ImportantNotices
        {
            get => _importantNotices;
            set => SetProperty(ref _importantNotices, value);
        }

        private string _otherNotices;

        public string OtherNotices
        {
            get => _otherNotices;
            set => SetProperty(ref _otherNotices, value);
        }

        private Api.Changelogs _changelogs;

        public Api.Changelogs Changelogs
        {
            get => _changelogs;
            set => SetProperty(ref _changelogs, value);
        }

        private string _updatedNotice;

        public string UpdatedNotice
        {
            get => _updatedNotice;
            set => SetProperty(ref _updatedNotice, value);
        }

        private Visibility _updatedNoticeVisible;

        public Visibility UpdatedNoticeVisible
        {
            get => _updatedNoticeVisible;
            set => SetProperty(ref _updatedNoticeVisible, value);
        }
        
        private string _outdatedNotice;

        public string OutdatedNotice
        {
            get => _outdatedNotice;
            set => SetProperty(ref _outdatedNotice, value);
        }

        private Visibility _outdatedNoticeVisible;

        public Visibility OutdatedNoticeVisible
        {
            get => _outdatedNoticeVisible;
            set => SetProperty(ref _outdatedNoticeVisible, value);
        }

        private Visibility _importantNoticesGrid;

        public Visibility ImportantNoticesGrid
        {
            get => _importantNoticesGrid;
            set => SetProperty(ref _importantNoticesGrid, value);
        }

        private Visibility _otherNoticesGrid;

        public Visibility OtherNoticesGrid
        {
            get => _otherNoticesGrid;
            set => SetProperty(ref _otherNoticesGrid, value);
        }

        #endregion

        #region Constructors

        public void Init()
        {
            UpdatedNoticeVisible = Visibility.Collapsed;
            OutdatedNoticeVisible = Visibility.Collapsed;
            AsyncContext.Run(async () => await UpdateNoticesAsync());
            AsyncContext.Run(async () => await GetChangelog());
            if (AppMan.App.AppUpdated != 0)
            {
                UpdatedNoticeVisible = Visibility.Visible;
                UpdatedNotice = $"Syn3 Updater has been updated to {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} version {Assembly.GetEntryAssembly()?.GetName().Version}";
            } 
            if (AppMan.App.Outdated != null)
            {
                OutdatedNoticeVisible = Visibility.Visible;
                OutdatedNotice = $"Syn3 Updater version {AppMan.App.Outdated} is available! Please restart Syn3 Updater and run the Launcher to update, if this fails please update manually";
                AppMan.App.Outdated = null;
            }
        }

        public async Task ReloadNoticesAction()
        {
            await UpdateNoticesAsync();
        }

        public async Task ReloadChangelogAction()
        {
            await GetChangelog();
        }

        public async Task UpdateNoticesAsync()
        {
            ImportantNotices = "Loading notices, please wait...";
            OtherNotices = "Loading notices, please wait...";
            HttpResponseMessage response = await AppMan.Client.GetAsync(Api.NoticesURL);
            Api.Notices output = JsonHelpers.Deserialize<Api.Notices>(await response.Content.ReadAsStreamAsync());
            string updatedDate;
            ImportantNotices = "<style>h4 { margin:0px; } div { padding-bottom:10px;}</style>";
            OtherNotices = "";
            foreach (Api.Notice notice in output.Notice)
            {
                DateTime utcCreatedDate = DateTime.SpecifyKind(DateTime.Parse(notice.DateCreated), DateTimeKind.Local);
                string createdDate = $"Published: {utcCreatedDate.ToLocalTime():dddd, dd MMMM yyyy HH:mm:ss}";

                if (notice?.DateUpdated != null)
                {
                    DateTime utcUpdatedDate = DateTime.SpecifyKind(DateTime.Parse(notice.DateUpdated), DateTimeKind.Local);
                    updatedDate = $" (Updated: {utcUpdatedDate.ToLocalTime():dddd, dd MMMM yyyy HH:mm:ss})";
                }
                else
                {
                    updatedDate = null;
                }

                string html = $"<div><h4><u>{notice.Title}</u></h4>" + notice.NoticeContent + $"<h6>{createdDate} {updatedDate}</h6></div>";
                if (notice.Important)
                    ImportantNotices = html + ImportantNotices;
                else
                    OtherNotices = html + OtherNotices;
            }

            OtherNoticesGrid = string.IsNullOrEmpty(OtherNotices) ? Visibility.Collapsed : Visibility.Visible;
            ImportantNoticesGrid = ImportantNotices == "<style>h4 { margin:0px; } div { padding-bottom:10px;}</style>" ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task GetChangelog()
        {
            HttpResponseMessage response = await AppMan.Client.GetAsync(Api.ChangelogURL);
            Changelogs = JsonHelpers.Deserialize<Api.Changelogs>(await response.Content.ReadAsStreamAsync());
        }

        public async Task VisitGithubAction(string version)
        {
            Process.Start($"https://github.com/CyanLabs/Syn3Updater/releases/tag/{version}");
        }

        #endregion
    }
}