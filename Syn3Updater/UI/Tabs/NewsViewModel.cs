using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;


namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class NewsViewModel : LanguageAwareBaseViewModel
    {
        #region Properties & Fields

        private AsyncCommand _reloadNotices;
        public AsyncCommand ReloadNotices => _reloadNotices ??= new AsyncCommand(ReloadNoticesAction);

        private AsyncCommand _reloadChangelog;
        public AsyncCommand ReloadChangelog => _reloadChangelog ??= new AsyncCommand(ReloadChangelogAction);

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

        private string _changelog;

        public string Changelog
        {
            get => _changelog;
            set => SetProperty(ref _changelog, value);
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
            Task.Run(UpdateNoticesAsync);
            Task.Run(GetChangelog);
            if (AppMan.App.AppUpdated != 0)
            {
                UpdatedNoticeVisible = Visibility.Visible;
                UpdatedNotice = $"Syn3 Updater has been updated to {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} version {Assembly.GetEntryAssembly()?.GetName().Version}";
            }
        }

        public async Task ReloadNoticesAction()
        {
           await UpdateNoticesAsync();
        }

        public async Task  ReloadChangelogAction()
        {
            await GetChangelog();
        }

        public async Task UpdateNoticesAsync()
        {
            ImportantNotices = "Loading notices, please wait...";
            OtherNotices = "Loading notices, please wait...";
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.NoticesURL);
            Api.Notices output = JsonHelpers.Deserialize<Api.Notices>(await response.Content.ReadAsStreamAsync());
            string updatedDate = "";
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
            Changelog = "Loading changelog, please wait...";
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.ChangelogURL);
            Api.Changelogs output = JsonHelpers.Deserialize<Api.Changelogs>(await response.Content.ReadAsStreamAsync());
            Changelog = "<style>h4 { margin:0px; } div { padding-bottom:0px;}</style>";
            foreach (Api.Changelog changelog in output.Changelog)
            {
                string changelognotes = string.IsNullOrWhiteSpace(changelog.ReleaseNotes) ? "No Changelog Available" : changelog.ReleaseNotes.Replace(Environment.NewLine, "<br>");
                string html = $"<div><h4><u>v{changelog.Version} - {changelog.Date}</u></h4>" + changelognotes + "</div>";
                Changelog += html;
            }
        }

        #endregion
    }
}