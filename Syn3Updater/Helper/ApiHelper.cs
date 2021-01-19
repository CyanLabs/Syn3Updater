using System.Net.Http;
using Cyanlabs.Syn3Updater.Model;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for API functions
    /// </summary>
    internal class ApiHelper
    {

        #region Methods
        /// <summary>
        ///     Get special IVSU package such as Downgrade or Reformat from our API and passes it to ConvertIvsu
        /// </summary>
        /// <param name="url">URL of a valid 'SpecialPackage'</param>
        public static SyncModel.SyncIvsu GetSpecialIvsu(string url)
        {
            HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(url).Result;
            Api.Ivsu ivsu = JsonConvert.DeserializeObject<Api.Ivsu>(response.Content.ReadAsStringAsync().Result);
            return ConvertIvsu(ivsu);
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SyncModel.SyncIvsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>Returns ivsu as type SyncModel.SyncIvsu</returns>
        public static SyncModel.SyncIvsu ConvertIvsu(Api.Ivsu ivsu)
        {
            string fileName = FileHelper.url_to_filename(ivsu.url);
            SyncModel.SyncIvsu output = new SyncModel.SyncIvsu
            {
                Type = ivsu.type,
                Name = ivsu.name,
                Version = ivsu.version,
                Notes = ivsu.notes,
                Url = ivsu.url,
                Md5 = ivsu.md5,
                Selected = true,
                FileName = fileName.Replace("?dl=1", "")
            };
            return output;
        }
        #endregion

    }
}