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
        public static SModel.Ivsu GetSpecialIvsu(string url)
        {
            HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(url).Result;
            Api.Ivsu ivsu = JsonConvert.DeserializeObject<Api.Ivsu>(response.Content.ReadAsStringAsync().Result);
            return ConvertIvsu(ivsu);
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SModel.Ivsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>ivsu as type SModel.Ivsu</returns>
        public static SModel.Ivsu ConvertIvsu(Api.Ivsu ivsu)
        {
            string fileName = FileHelper.url_to_filename(ivsu.Url);
            SModel.Ivsu output = new SModel.Ivsu
            {
                Type = ivsu.Type,
                Name = ivsu.Name,
                Version = ivsu.Version,
                Notes = ivsu.Notes,
                Url = ivsu.Url,
                Md5 = ivsu.Md5,
                Selected = true,
                FileName = fileName.Replace("?dl=1", "")
            };
            return output;
        }
        #endregion

    }
}