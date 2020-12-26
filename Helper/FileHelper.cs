using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Syn3Updater.Model;

namespace Syn3Updater.Helper
{
    public class FileHelper
    {
        #region Events

        private readonly EventHandler<EventArgs<int>> _percentageChanged;

        #endregion

        #region Properties & Fields

        public struct ValidateResult
        {
            public string Message;
            public bool Result;
        }

        #endregion

        #region Constructors

        private static readonly HttpClient Client = new HttpClient();

        public FileHelper(EventHandler<EventArgs<int>> externalPercentageChanged)
        {
            _percentageChanged = externalPercentageChanged;
        }

        #endregion

        #region Methods

        public void CopyFile(string source, string destination, CancellationToken ct)
        {
            int bufferSize = 1024 * 512;
            using (FileStream inStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (FileStream fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write))
            {
                int bytesRead;
                int totalReads = 0;
                long totalBytes = inStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;

                while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                {
                    if (ct.IsCancellationRequested)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                        try
                        {
                            File.Delete(destination);
                        }
                        catch (IOException)
                        {
                        }

                        return;
                    }

                    fileStream.Write(bytes, 0, bytesRead);
                    totalReads += bytesRead;
                    int percent = Convert.ToInt32(totalReads / (decimal) totalBytes * 100);
                    if (percent != prevPercent)
                    {
                        _percentageChanged.Raise(this, percent);
                        prevPercent = percent;
                    }
                }
            }
        }

        //https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/
        public async Task download_file(string path, string filename, CancellationToken ct)
        {
            using (HttpResponseMessage response = await Client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                long total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;

                bool canReportProgress = total != -1;

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    long totalRead = 0L;
                    byte[] buffer = new byte[4096];
                    bool moreToRead = true;
                    const int chunkSize = 4096;
                    FileStream fileStream = File.Create(filename, chunkSize);
                    do
                    {
                        if (ct.IsCancellationRequested)
                        {
                            fileStream.Close();
                            fileStream.Dispose();
                            try
                            {
                                File.Delete(filename);
                            }
                            catch (IOException)
                            {
                            }

                            return;
                        }

                        int read = await stream.ReadAsync(buffer, 0, buffer.Length, ct);

                        if (read == 0)
                        {
                            moreToRead = false;
                            fileStream.Close();
                            fileStream.Dispose();
                        }
                        else
                        {
                            byte[] data = new byte[read];
                            buffer.ToList().CopyTo(0, data, 0, read);
                            await fileStream.WriteAsync(buffer, 0, read, ct);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                double downloadPercentage = totalRead * 1d / (total * 1d) * 100;
                                int value = Convert.ToInt32(downloadPercentage);
                                _percentageChanged.Raise(this, value);
                            }
                        }
                    } while (moreToRead);
                }
            }
        }

        public ValidateResult validate_file(string srcfile, string localfile, string md5, bool copy, CancellationToken ct)
        {
            ValidateResult validateResult = new ValidateResult();
            string filename = Path.GetFileName(localfile);
            if (ct.IsCancellationRequested)
            {
                validateResult.Message = "[App] Process cancelled by user";
                validateResult.Result = false;
                return validateResult;
            }

            if (ApplicationManager.Instance.SkipCheck)
            {
                validateResult.Message = $"[Validator] SkipCheck activated, spoofing validation check for {filename}";
                validateResult.Result = true;
                return validateResult;
            }

            if (!File.Exists(localfile))
            {
                validateResult.Message = $"[Validator] {filename} is missing";
                validateResult.Result = false;
                return validateResult;
            }

            string localMd5 = md5_helper(localfile);

            if (md5 == null)
            {
                long filesize = new FileInfo(localfile).Length;
                if (copy)
                {
                    long srcfilesize = new FileInfo(srcfile).Length;

                    if (srcfilesize == filesize)
                        if (localMd5 == md5_helper(srcfile))
                        {
                            validateResult.Message = $"[Validator] {filename} checksum matches already verified local copy";
                            validateResult.Result = true;
                            return validateResult;
                        }
                }
                else
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        long newfilesize = -1;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, new Uri(srcfile));

                        if (long.TryParse(httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result.Content.Headers.ContentLength.ToString(),
                            out long contentLength))
                            newfilesize = contentLength;

                        if (newfilesize == filesize)
                        {
                            validateResult.Message = $"[Validator] no source checksum available for {filename} comparing filesize only";
                            validateResult.Result = true;
                            return validateResult;
                        }
                    }
                }
            }
            else if (string.Equals(localMd5, md5, StringComparison.CurrentCultureIgnoreCase))
            {
                validateResult.Message = $"[Validator] {filename} matches known good checksum";
                validateResult.Result = true;
                return validateResult;
            }

            validateResult.Message = $"[Validator] {filename} failed to validate";
            validateResult.Result = false;
            return validateResult;
        }

        public string md5_helper(string filename)
        {
            long totalBytesRead = 0;
            using (Stream file = File.OpenRead(filename))
            {
                long size = file.Length;
                HashAlgorithm hasher = MD5.Create();
                int bytesRead;
                byte[] buffer;
                do
                {
                    buffer = new byte[4096];
                    bytesRead = file.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;
                    hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                    long read = totalBytesRead;
                    if (totalBytesRead % 102400 == 0) _percentageChanged.Raise(this, (int) ((double) read / size * 100));
                } while (bytesRead != 0);

                hasher.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hasher.Hash).Replace("-", string.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ivsus"></param>
        /// <returns></returns>
        public static ObservableCollection<SyncModel.SyncIvsu> DebugMode(ObservableCollection<SyncModel.SyncIvsu> ivsus)
        {
            foreach (SyncModel.SyncIvsu ivsu in ivsus)
            {
                Uri myUri = new Uri(ivsu.Url);
                ivsu.Url = ivsu.Url.Replace(myUri.Host, "127.0.0.1").Replace(myUri.Scheme, "http");
            }
            return ivsus;
        }
        #endregion
    }
}