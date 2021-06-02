using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Syn3Updater.UI;
using Newtonsoft.Json;
using QRCoder;

namespace Cyanlabs.Syn3Updater
{
    public class SimpleLogger
    {
        public List<LogEntry> Log = new();

        public void Debug(object log, [CallerMemberName] string cmn = "")
        {
            Log.Add(new LogEntry(log.ToString(), "Debug", null, cmn));
        }

        public void Info(object log, [CallerMemberName] string cmn = "")
        {
            Log.Add(new LogEntry(log.ToString(), "Info", null, cmn));
        }

        public void CrashWindow(Exception ex, [CallerMemberName] string callerMemberName = "")
        {
            CrashWindow crashWindow = new() {ErrorName = {Text = ex.GetType().ToString()}, Message = {Text = ex.Message}, StackTrace = {Text = ex.StackTrace}};
            crashWindow.Show();

            Log.Add(new LogEntry(ex.GetType().ToString(), "Crash", ex));
            string guid = crashWindow.SendReport(ex);
            var output = JsonConvert.DeserializeAnonymousType(guid, new {uuid = "", status = ""});
            string url = Api.CrashLogUrl + output.uuid;
            crashWindow.ErrorReportUrl = url;

            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            crashWindow.Qrcode.Source = BitmapToImageSource(qrCodeImage);
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }

        public class LogEntry
        {
            public LogEntry(string log, string logType = "Info", Exception exception = null, [CallerMemberName] string callerMemberName = "")
            {
                Log = log;
                Time = DateTime.Now;
                Caller = callerMemberName;
                LogType = logType;

                if (exception != null) Exception = exception;
            }

            public string Log { get; set; }
            public DateTime Time { get; set; }
            public string Caller { get; set; }
            public string LogType { get; set; }
            public Exception Exception { get; set; }
        }
    }
}