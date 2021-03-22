using System.Net.Http;
using System.Threading.Tasks;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;
using Newtonsoft.Json;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for API functions
    /// </summary>
    internal static class ApiHelper
    {
        #region Methods
        /// <summary>
        ///     Get special IVSU package such as Downgrade or Reformat from our API and passes it to ConvertIvsu
        /// </summary>
        /// <param name="url">URL of a valid 'SpecialPackage'</param>
        public async static Task<SModel.Ivsu> GetSpecialIvsu(string url)
        {
            HttpResponseMessage response = await ApplicationManager.Instance.Client.GetAsync(url).ConfigureAwait(false);
            return ConvertIvsu(JsonHelpers.Deserialize<Api.Ivsu>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)));
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SModel.Ivsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>ivsu as type SModel.Ivsu</returns>
        public static SModel.Ivsu ConvertIvsu(Api.Ivsu ivsu)
        {
            string fileName = FileHelper.url_to_filename(ivsu.Url);
            return new SModel.Ivsu
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
        }
        #endregion

    }
}