using System;
using System.Diagnostics;
using System.Net.Http;
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

        public string ChangeLog
        {
            get => _changelog;
            set => SetProperty(ref _changelog, value);
        }
        #endregion

        #region Constructors

        public void Init()
        {
            ImportantNotices = "Loading notices, please wait...";
        }

        public void Reload()
        {
            Task.Run(UpdateNoticesAsync);
        }

        public async Task UpdateNoticesAsync()
        {
            ImportantNotices = "<style>h3 { margin:0px; } div { padding-bottom:10px;}</style>";
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(Api.NoticesURL);
            Api.Notices output = JsonConvert.DeserializeObject<Api.Notices>(await response.Content.ReadAsStringAsync());
            string updatedDate = "";
            foreach (Api.Notice notice in output.Notice)
            {
                DateTime utcCreatedDate = DateTime.SpecifyKind(DateTime.Parse(notice.DateCreated), DateTimeKind.Local);
                string createdDate = $"Published: {utcCreatedDate.ToLocalTime():dddd, dd MMMM yyyy HH:mm:ss}";

                if (notice?.DateUpdated != null)
                {
                    DateTime utcUpdatedDate = DateTime.SpecifyKind(DateTime.Parse(notice.DateUpdated), DateTimeKind.Local);
                    updatedDate = $" (Updated: {utcUpdatedDate.ToLocalTime():dddd, dd MMMM yyyy HH:mm:ss})";
                }
                
                string html = $"<div><h3><u>{notice.Title}</u></h3>" +  notice.NoticeContent + $"<h6>{createdDate} {updatedDate}</h6></div>";
                if (notice.Important)
                    ImportantNotices += html;
                else
                    OtherNotices += html;
            }
        }

        #endregion
    }
}