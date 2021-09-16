using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        /// <param name="specialPackage">SpecialPackage type e.g downgradetool, logtool34 etc.</param>
        public static async Task<SModel.Ivsu> GetSpecialIvsu(string specialPackage)
        {
            try
            {
                var graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<Api.IvsuRoot>(GraphQlRequests.GetSpecialPackage(specialPackage));
                Api.IvsuRoot ivsu = graphQlResponse2.Data;
                return ConvertIvsu(ivsu.Ivsus[0]);
            }
            catch (Exception e)
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(e.GetFullMessage()));
                AppMan.Logger.Info("ERROR: fetching SpecialPackage - " + e.GetFullMessage());
                return null;
            }
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SModel.Ivsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>ivsu as type SModel.Ivsu</returns>
        public static SModel.Ivsu ConvertIvsu(Api.Ivsu ivsu)
        {
            return new()
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