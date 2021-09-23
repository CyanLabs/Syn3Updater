using System;
using System.Threading.Tasks;
using Avalonia;
using GraphQL;
using Syn3Updater.Models;

namespace Syn3Updater.Helpers
{
    public static class ApiHelper
    {
        /// <summary>
        ///     Gets All Sync APP versions from CyanLabs DB
        /// </summary>
        /// <returns>IvsuRoot with all retrieved APP versions</returns>
        public static async Task<IvsuRoot?> GetSyncVersions()
        {
            GraphQLResponse<IvsuRoot?> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<IvsuRoot>(GraphQlHelper.GetAppVersions());
            return graphQlResponse.Data;
        }

        /// <summary>
        ///     Get special IVSU package such as Downgrade or Reformat from our API and passes it to ConvertIvsu
        /// </summary>
        /// <param name="specialPackage">SpecialPackage type e.g downgradetool, logtool34 etc.</param>
        public static async Task<SModel.Ivsu> GetSpecialIvsu(string specialPackage)
        {
            var graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<IvsuRoot>(GraphQlHelper.GetSpecialPackage(specialPackage));
            IvsuRoot ivsu = graphQlResponse.Data;
            return ConvertIvsu(ivsu.Ivsus[0]);
        }

        /// <summary>
        ///     Converts a API retrieved Api.Ivsu to SModel.Ivsu
        /// </summary>
        /// <param name="ivsu">SpecialPackage IVSU object from GetSpecialIvsu</param>
        /// <returns>ivsu as type SModel.Ivsu</returns>
        private static SModel.Ivsu ConvertIvsu(Ivsu ivsu)
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
                FileName = ivsu.Url.Substring(ivsu.Url.LastIndexOf("/", StringComparison.Ordinal) + 1, ivsu.Url.Length - ivsu.Url.LastIndexOf("/", StringComparison.Ordinal) - 1).Replace("?dl=1", ""),
                FileSize = ivsu.FileSize
            };
        }
    }
}