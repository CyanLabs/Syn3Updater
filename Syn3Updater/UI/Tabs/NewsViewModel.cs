using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    internal class NewsViewModel : LanguageAwareBaseViewModel
    {
        #region Properties & Fields

        private ActionCommand _reloadNotices;
        public ActionCommand ReloadNotices => _reloadNotices ??= new ActionCommand(ReloadNoticesAction);

        private ActionCommand _reloadChangelog;
        public ActionCommand ReloadChangelog => _reloadChangelog ??= new ActionCommand(ReloadChangelogAction);

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

        public Visibility
            UpdatedNoticeVisible
        {
            get => _updatedNoticeVisible;
            set => SetProperty(ref _updatedNoticeVisible, value);
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

        public void ReloadNoticesAction()
        {
            Task.Run(UpdateNoticesAsync);
        }

        public void ReloadChangelogAction()
        {
            Task.Run(GetChangelog);
        }

        public async Task UpdateNoticesAsync()
        {
            ImportantNotices = "Loading notices, please wait...";
            OtherNotices = "Loading notices, please wait...";
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.NoticesURL);
            Api.Notices output = JsonConvert.DeserializeObject<Api.Notices>(await response.Content.ReadAsStringAsync());
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
        }

        public async Task GetChangelog()
        {
            Changelog = "Loading changelog, please wait...";
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.ChangelogURL);
            string output = await response.Content.ReadAsStringAsync();
            Changelog = "<style>h3 { margin:0px; } div { padding-bottom:10px;}</style>" + output.Replace("<br />", "");
        }

        #endregion
    }
}