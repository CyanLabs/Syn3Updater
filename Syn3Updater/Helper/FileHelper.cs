using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cyanlabs.Syn3Updater.Model;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for file related functionality
    /// </summary>
    public class FileHelper
    {
        #region Events

        private readonly EventHandler<EventArgs<int>> _percentageChanged;

        #endregion

        #region Properties & Fields

        public struct OutputResult
        {
            public string Message;
            public bool Result;
        }

        #endregion

        #region Constructors

        public FileHelper(EventHandler<EventArgs<int>> externalPercentageChanged)
        {
            _percentageChanged = externalPercentageChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Async copy file from source to destination with CancellationToken support
        /// </summary>
        /// <param name="source">Source file</param>
        /// <param name="destination">Destination file</param>
        /// <param name="ct">CancellationToken</param>
        public async Task CopyFileAsync(string source, string destination, CancellationToken ct)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            int bufferSize = 1024 * 512;
            using FileStream inStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, fileOptions);
            using FileStream fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
            int bytesRead;
            int totalReads = 0;
            long totalBytes = inStream.Length;
            byte[] bytes = new byte[bufferSize];
            int prevPercent = 0;

            while ((bytesRead = await inStream.ReadAsync(bytes, 0, bufferSize).ConfigureAwait(false)) > 0)
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

                await fileStream.WriteAsync(bytes, 0, bytesRead).ConfigureAwait(false);
                totalReads += bytesRead;
                int percent = Convert.ToInt32(totalReads / (decimal)totalBytes * 100);
                if (percent != prevPercent)
                {
                    _percentageChanged.Raise(this, percent);
                    prevPercent = percent;
                }
            }
        }

        private static HttpClient Client;

        /// <summary>
        ///     Downloads file from URL to specified filename using HTTPClient with CancellationToken support
        ///     <see href="https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/">See more</see>
        /// </summary>
        /// <param name="path">Source URL</param>
        /// <param name="filename">Destination filename</param>
        /// <param name="ct">CancellationToken</param>
        public async Task DownloadFile(string path, string filename, CancellationToken ct)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd(ApplicationManager.Instance.Header);
            using (HttpResponseMessage response = await Client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {
                long total = response.Content.Headers.ContentLength ?? -1L;

                bool canReportProgress = total != -1;

                using (Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
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
                            await fileStream.WriteAsync(buffer, 0, read, ct).ConfigureAwait(false);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                double downloadPercentage = totalRead * 1d / (total * 1d) * 100;
                                int value = Convert.ToInt32(downloadPercentage);
                                _percentageChanged.Raise(this, value);
                            }
                        }
                    } while (moreToRead);
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        ///     Validates downloaded or copied file against passed md5 using the <see cref="GenerateMd5"/> method
        ///     Supports a local source using FileInfo or a remote source using HTTP HEAD requests
        ///     Supports CancellationToken
        /// </summary>
        /// <param name="source">Source file or URL</param>
        /// <param name="localfile">Local file to compare against</param>
        /// <param name="md5">MD5 checksum to compare against</param>
        /// <param name="localonly">Set to true if comparing to local sources else set to false</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>outputResult with Message and Result properties</returns>
        public OutputResult ValidateFile(string source, string localfile, string md5, bool localonly, CancellationToken ct)
        {
            OutputResult outputResult = new OutputResult();
            string filename = Path.GetFileName(localfile);

            if (!File.Exists(localfile))
            {
                outputResult.Message = "";
                outputResult.Result = false;
                return outputResult;
            }

            string localMd5 = GenerateMd5(localfile, ct);
            if (md5 == null)
            {
                long filesize = new FileInfo(localfile).Length;
                if (localonly)
                {
                    long srcfilesize = new FileInfo(source).Length;

                    if (srcfilesize == filesize && localMd5 == GenerateMd5(source, ct))
                    {
                        outputResult.Message = $"{filename} checksum matches already verified local copy";
                        outputResult.Result = true;
                        return outputResult;
                    }
                }
                else
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(ApplicationManager.Instance.Header);
                        long newfilesize = -1;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, new Uri(source));

                        if (long.TryParse(httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result.Content.Headers.ContentLength.ToString(),
                            out long contentLength))
                            newfilesize = contentLength;

                        if (newfilesize == filesize)
                        {
                            outputResult.Message = $"no source checksum available for {filename} comparing file size";
                            outputResult.Result = true;
                            return outputResult;
                        }
                    }
                }
            }
            else if (string.Equals(localMd5, md5, StringComparison.CurrentCultureIgnoreCase))
            {
                outputResult.Message = "";
                outputResult.Result = true;
                return outputResult;
            }

            if (ct.IsCancellationRequested)
            {
                outputResult.Message = "Process cancelled by user";
                outputResult.Result = false;
                return outputResult;
            }

            outputResult.Message = $"Validate: {filename} (Failed!, Downloading)";
            outputResult.Result = false;
            return outputResult;
        }

        /// <summary>
        ///     Hashes source file against the Md5 algorithm with support for CancellationToken
        /// </summary>
        /// <param name="filename">Source File</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>MD5 hash as String</returns>
        public string GenerateMd5(string filename, CancellationToken ct)
        {
            long totalBytesRead = 0;
            try
            {
                using (Stream file = File.OpenRead(filename))
                {
                    long size = file.Length;
                    HashAlgorithm hasher = MD5.Create();
                    int bytesRead;
                    byte[] buffer;
                    do
                    {
                        if (ct.IsCancellationRequested)
                        {
                            file.Close();
                            file.Dispose();
                            return null;
                        }
                        buffer = new byte[4096];
                        bytesRead = file.Read(buffer, 0, buffer.Length);
                        totalBytesRead += bytesRead;
                        hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
                        long read = totalBytesRead;
                        if (totalBytesRead % 102400 == 0) _percentageChanged.Raise(this, (int)((double)read / size * 100));
                    } while (bytesRead != 0);

                    hasher.TransformFinalBlock(buffer, 0, 0);
                    return BitConverter.ToString(hasher.Hash).Replace("-", string.Empty);
                }
            }
            catch (IOException e)
            {
                Application.Current.Dispatcher.Invoke(() => ModernWpf.MessageBox.Show(e.GetFullMessage(), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation));
                ApplicationManager.Logger.Info("ERROR: " + e.GetFullMessage());
                return "error";
            }
        }

        /// <summary>
        ///     Extracts the last part of a URL to for use as a filename
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>filename as String</returns>
        public static string url_to_filename(string url) => url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1, url.Length - url.LastIndexOf("/", StringComparison.Ordinal) - 1);

        /// <summary>
        ///     Extracts the tar.gz file in to multiple packages (naviextras)
        /// </summary>
        /// <param name="item">The SModel.Ivsu of the item to extract</param>
        /// <param name="ct"></param>
        /// <returns>outputResult with Message and Result properties</returns>
        public OutputResult ExtractMultiPackage(SModel.Ivsu item, CancellationToken ct)
        {
            OutputResult outputResult = new OutputResult { Message = "" };
            if (item.Source != "naviextras")
            {
                outputResult.Result = true;
                return outputResult;
            }
            else
            {
                string path = ApplicationManager.Instance.DownloadPath + item.FileName;
                string destination = System.IO.Path.ChangeExtension(path, null);
                Stream inStream = File.OpenRead(path);
                Stream gzipStream = new GZipInputStream(inStream);

                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
                tarArchive.ExtractContents(destination);
                tarArchive.Close();

                gzipStream.Close();
                inStream.Close();

                foreach (var tarfile in Directory.GetFiles(destination, "*.tar.gz*", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(tarfile).Replace(".tar", "");
                    string filename = Path.GetFileName(tarfile);
                    string newpath = ApplicationManager.Instance.DownloadPath + filename;
                    if (File.Exists(newpath))
                        File.Delete(newpath);
                    File.Move(tarfile, newpath);
                    string type = "";

                    if (name.Contains("14G424"))
                    {
                        type = "MAP_LICENSE";
                    }
                    else if (name.Contains("14G421"))
                    {
                        type = "MAP";
                    }

                    ApplicationManager.Instance.ExtraIvsus.Add(new SModel.Ivsu
                    {
                        Type = type,
                        Name = name,
                        Version = "",
                        Notes = "",
                        Url = "",
                        Md5 = GenerateMd5(newpath, ct),
                        Selected = true,
                        FileName = filename
                    });
                }
                outputResult.Message = "Added MultiPackage files to Queue";
                outputResult.Result = true;
            }
            return outputResult;
        }
        #endregion
    }
}