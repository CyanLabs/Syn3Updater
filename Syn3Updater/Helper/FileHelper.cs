﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        #region Constructors

        public FileHelper(EventHandler<EventArgs<int>> externalPercentageChanged)
        {
            _percentageChanged = externalPercentageChanged;
        }

        #endregion

        #region Properties & Fields

        public struct OutputResult
        {
            public string Message;
            public bool Result;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Async copy file from source to destination with CancellationToken support
        /// </summary>
        /// <param name="source">Source file</param>
        /// <param name="destination">Destination file</param>
        /// <param name="ct">CancellationToken</param>
        public async Task<bool> CopyFileAsync(string source, string destination, CancellationToken ct)
        {
            try
            {
                FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
                int bufferSize = 1024 * 512;
                using FileStream inStream = new(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, fileOptions);
                using FileStream fileStream = new(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
                int bytesRead;
                int totalReads = 0;
                long totalBytes = inStream.Length;
                byte[] bytes = new byte[bufferSize];
                int prevPercent = 0;

                while ((bytesRead = await inStream.ReadAsync(bytes, 0, bufferSize, ct)) > 0)
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

                        return false;
                    }

                    await fileStream.WriteAsync(bytes, 0, bytesRead, ct);
                    totalReads += bytesRead;
                    int percent = Convert.ToInt32(totalReads / (decimal) totalBytes * 100);
                    if (percent == prevPercent) continue;
                    _percentageChanged.Raise(this, percent, 0);
                    prevPercent = percent;
                }
            }
            catch (IOException ioException)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(ioException.GetFullMessage()));
                AppMan.Logger.Info("ERROR: " + ioException.GetFullMessage());
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(exception.GetFullMessage()));
                AppMan.Logger.Info("ERROR: " + exception.GetFullMessage());
                return false;
            }

            return true;
        }

        public class Range
        {
            public long Start { get; set; }
            public long End { get; set; }
        }

        public class DownloadPartResult
        {
            public long RangeStart { get; set; }
            public string FilePath { get; set; }
            public Exception Ex { get; set; }
        }

        /// <summary>
        ///     Downloads file from URL to specified filename using HTTPClient with CancellationToken support
        ///     <see
        ///         href="https://www.technical-recipes.com/2018/reporting-the-percentage-progress-of-large-file-downloads-in-c-wpf/">
        ///         See
        ///         more
        ///     </see>
        /// </summary>
        /// <param name="filename">Destination filename</param>
        /// <param name="fileUrl">Source URL</param>
        /// <param name="destinationFilePath"></param>
        /// <param name="ct">CancellationToken</param>
        /// <param name="numberOfParallelDownloads"></param>
        /// <returns>bool with True if successful or False if not</returns>
        public async Task<bool> DownloadFile(string fileUrl, string destinationFilePath, CancellationToken ct, int numberOfParallelDownloads = 1)
        {
            try
            {
                #region Get file size
                
                HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(fileUrl);
                webRequest.UserAgent = AppMan.App.Header;
                webRequest.Method = "HEAD";
                long responseLength;
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    if(!long.TryParse(webResponse.Headers.Get("Content-Length"), out responseLength))
                        responseLength = long.Parse(webResponse.Headers.Get("X-Dropbox-Content-Length"));
                }

                #endregion

                if (numberOfParallelDownloads > 1)
                {

                    using (FileStream destinationStream = new(destinationFilePath, FileMode.Append))
                    {
                        ConcurrentDictionary<long, string> tempFilesDictionary = new();

                        #region Calculate ranges

                        List<Range> readRanges = new();
                        for (int chunk = 0; chunk < numberOfParallelDownloads - 1; chunk++)
                        {
                            Range range = new()
                            {
                                Start = chunk * (responseLength / numberOfParallelDownloads),
                                End = (chunk + 1) * (responseLength / numberOfParallelDownloads) - 1
                            };
                            readRanges.Add(range);
                        }

                        readRanges.Add(new Range
                        {
                            Start = readRanges.Any() ? readRanges.Last().End + 1 : 0,
                            End = responseLength - 1
                        });

                        #endregion

                        #region Parallel download

                        int i = 1;

                        List<Task> tasks = new();
                        List<DownloadPartResult> results = new();

                        foreach (Range readRange in readRanges)
                        {
                            int i1 = i;
                            Task t = Task.Run(async () =>
                            {
                                DownloadPartResult result = await DownloadFilePart(fileUrl, destinationFilePath, readRange, i1, ct);
                                AppMan.Logger.Debug($"DownloadFilePart: {i1} ({readRange.Start}/{readRange.End} - {responseLength.ToString()})");
                                results.Add(result);
                            }, ct);
                            i++;
                            tasks.Add(t);
                        }
                        try
                        {
                            await Task.WhenAll(tasks);
                            AppMan.Logger.Debug($"DownloadFilePart: All Tasks Completed {tasks.Count}");
                        }
                        catch (OperationCanceledException e)
                        {
                            AttemptDownloadFileDelete(destinationFilePath);
                            await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                            return false;
                        }
                        catch (IOException e)
                        {
                            AttemptDownloadFileDelete(destinationFilePath);
                            await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                            return false;
                        }
                        catch (ObjectDisposedException  e)
                        {
                            AttemptDownloadFileDelete(destinationFilePath);
                            await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                            return false;
                        }

                        foreach (DownloadPartResult result in results)
                        {
                            if (ct.IsCancellationRequested) return false;
                            if (result == null) return false;
                            if (result.FilePath == "cancelled") return false;
                            if (result.Ex != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(result.Ex.GetFullMessage()));
                                return false;
                            }

                            tempFilesDictionary.TryAdd(result.RangeStart, result.FilePath);

                        }

                        #endregion

                        #region Merge to single file

                        foreach (KeyValuePair<long, string> tempFile in tempFilesDictionary.OrderBy(b => b.Key))
                        {
                            try
                            {
                                byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                                destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                                File.Delete(tempFile.Value);
                            }
                            catch (IOException e)
                            {
                                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                                return false;
                            }
                        }

                        #endregion

                        return true;
                    }
                }

                DownloadPartResult result2 = await DownloadFilePart(fileUrl, destinationFilePath, new Range {Start = 0, End = responseLength}, 0, ct);
                return result2.FilePath != null;
            }
            catch (WebException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(fileUrl.Contains("dropbox") ? "Unable to download the Reformat Tool from Dropbox!" + Environment.NewLine + "Dropbox may be blocked by your Internet Provider" + Environment.NewLine : null + e.GetFullMessage()));
                return false;
            }
            catch (HttpRequestException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(fileUrl.Contains("dropbox") ? "Unable to download the Reformat Tool from Dropbox!" + Environment.NewLine + "Dropbox may be blocked by your Internet Provider" + Environment.NewLine : null + e.GetFullMessage()));
                return false;
            }
            catch (IOException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage())));
                return false;
            }
            catch (OutOfMemoryException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage())));
                return false;
            }
            catch (TaskCanceledException e)
            {
                return false;
            }
        }

        private void AttemptDownloadFileDelete(string destinationFilePath)
        {
            try
            {
                foreach (string file in Directory.GetFiles(Path.GetDirectoryName(destinationFilePath), "*" + Path.GetFileName(destinationFilePath) + "-part*"))
                    File.Delete(file);
                File.Delete(destinationFilePath);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        

        public async Task<DownloadPartResult> DownloadFilePart(string fileUrl, string destinationFilePath, Range readRange, int concurrent, CancellationToken ct)
        {
            try
            {
                string tempFilePath = concurrent == 0 ? destinationFilePath : destinationFilePath + $"-part{concurrent}";
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);

                HttpRequestMessage request = new(HttpMethod.Get, new Uri(fileUrl));
                request.Headers.Range = new RangeHeaderValue(readRange.Start, readRange.End);
                HttpResponseMessage response = await AppMan.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    long total = readRange.End - readRange.Start;
                    long totalRead = 0L;
                    byte[] buffer = new byte[4096];
                    bool moreToRead = true;
                    const int chunkSize = 4096;
                    using (FileStream output = File.Create(tempFilePath, chunkSize))
                    {
                        do
                        {
                            //TODO: Change to Span/Memory once the netframework version is depricated  
                            int read = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                            if (ct.IsCancellationRequested)
                            {
                                try
                                {
                                    File.Delete(destinationFilePath);
                                }
                                catch (IOException)
                                {
                                }

                                return new DownloadPartResult {FilePath = "cancelled", RangeStart = readRange.Start};
                            }

                            if (read == 0)
                            {
                                moreToRead = false;
                            }
                            else
                            {
                                //byte[] data = new byte[read];
                                //Array.Copy(buffer, data, read);   
                                await output.WriteAsync(buffer, 0, read, ct);
                                totalRead += read;
                                double downloadPercentage = totalRead * 1d / (total * 1d) * 100;
                                int value = Convert.ToInt32(downloadPercentage);
                                _percentageChanged.Raise(this, value, concurrent);
                            }
                        } while (moreToRead);

                        return new DownloadPartResult {FilePath = tempFilePath, RangeStart = readRange.Start, Ex = null};
                    }
                }
            }
            catch (IOException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                return new DownloadPartResult {FilePath = "cancelled", RangeStart = readRange.Start};
            }
        }

        /// <summary>
        ///     Validates downloaded or copied file against passed md5 using the <see cref="GenerateMd5" /> method
        ///     Supports a local source using FileInfo or a remote source using HTTP HEAD requests
        ///     Supports CancellationToken
        /// </summary>
        /// <param name="source">Source file or URL</param>
        /// <param name="localfile">Local file to compare against</param>
        /// <param name="md5">MD5 checksum to compare against</param>
        /// <param name="localonly">Set to true if comparing to local sources else set to false</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>outputResult with Message and Result properties</returns>
        public async Task<OutputResult> ValidateFile(string source, string localfile, string md5, bool localonly, CancellationToken ct)
        {
            OutputResult outputResult = new();
            string filename = Path.GetFileName(localfile);

            if (!File.Exists(localfile))
            {
                outputResult.Message = "";
                outputResult.Result = false;
                return outputResult;
            }

            string localMd5 = await GenerateMd5(localfile, ct);
            if (md5 == null)
            {
                long filesize = new FileInfo(localfile).Length;
                if (localonly)
                {
                    long srcfilesize = new FileInfo(source).Length;

                    if (srcfilesize == filesize && localMd5 == await GenerateMd5(source, ct))
                    {
                        outputResult.Message = $"{filename} checksum matches already verified local copy";
                        outputResult.Result = true;
                        return outputResult;
                    }
                }
                else
                {
                    long newfilesize = -1;
                    HttpRequestMessage request = new(HttpMethod.Head, new Uri(source));

                    long? len = (await AppMan.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)).Content.Headers.ContentLength;

                    if (len != null)
                        newfilesize = len.GetValueOrDefault();
                    else
                        throw new Exception("Could not get size of file from remote server");

                    if (newfilesize == filesize)
                    {
                        outputResult.Message = $"no source checksum available for {filename} comparing file size";
                        outputResult.Result = true;
                        return outputResult;
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
        public async Task<string> GenerateMd5(string filename, CancellationToken ct)
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
                        if (totalBytesRead % 102400 == 0) _percentageChanged.Raise(this, (int) ((double) read / size * 100), 0);
                    } while (bytesRead != 0);

                    hasher.TransformFinalBlock(buffer, 0, 0);
                    return BitConverter.ToString(hasher.Hash).Replace("-", string.Empty);
                }
            }
            catch (IOException e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                AppMan.Logger.Info("ERROR: " + e.GetFullMessage());
                return "error";
            }
            catch (UnauthorizedAccessException exception)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(exception.GetFullMessage()));
                AppMan.Logger.Info("ERROR: " + exception.GetFullMessage());
                return "error";
            }
        }

        /// <summary>
        ///     Extracts the last part of a URL to for use as a filename
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>filename as String</returns>
        public static string url_to_filename(string url)
        {
            return url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1, url.Length - url.LastIndexOf("/", StringComparison.Ordinal) - 1);
        }

        /// <summary>
        ///     Extracts the tar.gz file in to multiple packages (naviextras)
        /// </summary>
        /// <param name="item">The SModel.Ivsu of the item to extract</param>
        /// <param name="ct"></param>
        /// <returns>outputResult with Message and Result properties</returns>
        public async Task<OutputResult> ExtractMultiPackage(SModel.Ivsu item, CancellationToken ct)
        {
            OutputResult outputResult = new() {Message = ""};
            if (item.Source != "naviextras")
            {
                outputResult.Result = true;
                return outputResult;
            }

            string path = AppMan.App.DownloadPath + item.FileName;
            string destination = Path.ChangeExtension(path, null);
            Stream inStream = File.OpenRead(path);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.ASCII);
            tarArchive.ExtractContents(destination);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();

            foreach (string tarfile in Directory.GetFiles(destination, "*.tar.gz*", SearchOption.AllDirectories))
            {
                string name = Path.GetFileNameWithoutExtension(tarfile).Replace(".tar", "");
                string filename = Path.GetFileName(tarfile);
                string newpath = AppMan.App.DownloadPath + filename;
                if (File.Exists(newpath))
                    File.Delete(newpath);
                File.Move(tarfile, newpath);
                string type = "";

                if (name.Contains("14G424"))
                    type = "MAP_LICENSE";
                else if (name.Contains("14G421")) type = "MAP";
                FileInfo fi = new(newpath);
                long size = fi.Length;
                AppMan.App.ExtraIvsus.Add(new SModel.Ivsu
                {
                    Type = type,
                    Name = name,
                    Version = "",
                    Notes = "",
                    Url = "",
                    Md5 = await GenerateMd5(newpath, ct),
                    Selected = true,
                    FileName = filename,
                    FileSize = size
                });
            }

            outputResult.Message = "Added MultiPackage files to Queue";
            outputResult.Result = true;
            return outputResult;
        }

        //https://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        public static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, "_");
        }

        public static string RemoveInvalidXmlChars(string text)
        {
            var validChars = text.Where(ch => System.Xml.XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validChars);
        }

        #endregion
    }
}