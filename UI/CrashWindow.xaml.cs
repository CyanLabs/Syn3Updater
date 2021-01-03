﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Syn3Updater.Model;

namespace Syn3Updater.UI
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        public CrashWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationManager.Instance.Exit();
        }

        public string SendReport(Exception exception)
        {
            CrashContainer crashContainer = new CrashContainer();

            StackTrace st = new StackTrace(exception, true);
            StackFrame frame = st.GetFrame(st.FrameCount - 1);

            crashContainer.ErrorName = exception.GetType().ToString();
            if(frame != null) crashContainer.ErrorLocation = frame.GetFileName() + " / " + frame.GetMethod().Name + " / " + frame.GetFileLineNumber();
            crashContainer.Logs = ApplicationManager.Logger.Log;

            string text = JsonConvert.SerializeObject(crashContainer);
            string version = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            string computername = System.Environment.MachineName;
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "detail", text },
                { "version", version },
                { "computername",computername},
                { "error",crashContainer.ErrorName}
            };

            var content = new FormUrlEncodedContent(values);

            var response = client.PostAsync(Api.CrashLogPost, content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;

            return responseString;
        }

        public string ErrorReportUrl;
        private void ClickQRCode(object sender, RoutedEventArgs e)
        {
            Process.Start(ErrorReportUrl);
        }

        private void ViewReport(object sender, RoutedEventArgs e)
        {
            Process.Start(ErrorReportUrl);
        }

        public class CrashContainer
        {
            public string ErrorName { get; set; }
            public string ErrorLocation { get; set; }
            public List<SimpleLogger.LogEntry> Logs { get; set; } = new List<SimpleLogger.LogEntry>();
        }
    }
}