using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Updater.Common;

namespace Cyanlabs.Syn3Updater.Helper
{
    /// <summary>
    ///     Helper class for API functions
    /// </summary>
    public static class ApiHelper
    {
        #region Methods
        /// <summary>
        ///     Get special IVSU package such as Downgrade or Reformat from our API and passes it to ConvertIvsu
        /// </summary>
        /// <param name="url">URL of a valid 'SpecialPackage'</param>
        public async static Task<SModel.Ivsu> GetSpecialIvsu(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new System.ArgumentNullException(nameof(url), "Url to download file is empty");
            }
            HttpResponseMessage response = await AppMan.App.Client.GetAsync(url);
            Api.Ivsu ivsu = JsonHelpers.Deserialize<Api.Ivsu>(await response.Content.ReadAsStreamAsync());
            return ConvertIvsu(ivsu);
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SModel.Ivsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>ivsu as type SModel.Ivsu</returns>
        public static SModel.Ivsu ConvertIvsu(Api.Ivsu ivsu)
        {
            return new SModel.Ivsu
            {
                Type = ivsu.Type,
                Name = ivsu.Name,
                Version = ivsu.Version,
                Notes = ivsu.Notes,
                Url = ivsu.Url,
                Md5 = ivsu.Md5,
                Selected = true,
                FileName = FileHelper.url_to_filename(ivsu.Url).Replace("?dl=1", ""),
                FileSize = ivsu.FileSize
            };
        }
        #endregion

    }
}