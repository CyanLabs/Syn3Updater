using System.Net.Http;
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
        public static SModel.Ivsu GetSpecialIvsu(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new System.ArgumentNullException("Url to download file is empty");
            }
            HttpResponseMessage response = ApplicationManager.Instance.Client.GetAsync(url).GetAwaiter().GetResult();
            return ConvertIvsu(JsonHelpers.Deserialize<Api.Ivsu>(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()));
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
                FileName = FileHelper.url_to_filename(ivsu.Url).Replace("?dl=1", "")
            };
        }
        #endregion

    }
}