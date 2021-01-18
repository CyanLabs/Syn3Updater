using System;
using System.Net.Http;
using Cyanlabs.Syn3Updater.Model;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.Helper
{
    class ApiHelper
    {
        public static SyncModel.SyncIvsu GetSpecialIvsu(string url)
        {
            HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(url).Result;
            Api.Ivsu ivsu = JsonConvert.DeserializeObject<Api.Ivsu>(response.Content.ReadAsStringAsync().Result);
            return ConvertIvsu(ivsu);
        }
        public static SyncModel.SyncIvsu ConvertIvsu(Api.Ivsu ivsu)
        {
            string fileName = ivsu.url.Substring(ivsu.url.LastIndexOf("/", StringComparison.Ordinal) + 1, ivsu.url.Length - ivsu.url.LastIndexOf("/", StringComparison.Ordinal) - 1);
            SyncModel.SyncIvsu output = new SyncModel.SyncIvsu {
                Type = ivsu.type,
                Name = ivsu.name,
                Version = ivsu.version,
                Notes = ivsu.notes,
                Url = ivsu.url,
                Md5 = ivsu.md5,
                Selected = true,
                FileName = fileName.Replace("?dl=1","")
            };
            return output;
        }
    }
}
