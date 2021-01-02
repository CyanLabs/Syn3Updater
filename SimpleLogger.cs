using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using QRCoder;
using Syn3Updater.UI;

namespace Syn3Updater
{
    public class SimpleLogger
    {
        public List<LogEntry> Log = new List<LogEntry>();

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
            CrashWindow crashWindow = new CrashWindow {errorName = {Text = ex.GetType().ToString()}, message = {Text = ex.Message}, stackTrace = {Text = ex.StackTrace}};
            crashWindow.Show();

            Log.Add(new LogEntry(ex.GetType().ToString(), "Crash", ex));
            string guid = crashWindow.SendReport(ex);
            var definition = new { uuid = "", status = "" };
            var output = JsonConvert.DeserializeAnonymousType(guid, definition);
            string url = "https://cyanlabs.net/api/Syn3Updater/crash-logs/?uuid=" + output.uuid;
            crashWindow.ErrorReportUrl = url;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            crashWindow.qrcode.Source = BitmapToImageSource(qrCodeImage);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }

        public class LogEntry
        {
            public string Log { get; set; }
            public DateTime Time { get; set; }
            public string Caller { get; set; }
            public string LogType { get; set; }
            public Exception Exception { get; set; }
            public LogEntry(string log, string logType = "Info", Exception exception = null, [CallerMemberName] string callerMemberName = "")
            {
                Log = log;
                Time = DateTime.Now;
                Caller = callerMemberName;
                LogType = logType;

                if (exception != null)
                {
                    Exception = exception;
                }
            }
        }
    }
}