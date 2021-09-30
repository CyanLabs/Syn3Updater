using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        public static async Task<ObservableCollection<string>> GetSyncVersions()
        {
            GraphQLResponse<IvsuRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<IvsuRoot>(GraphQlHelper.GetAppVersions());
            IvsuRoot response = graphQlResponse.Data;
            ObservableCollection<string> versions = new();
            if (response.Ivsus != null)
                foreach (Ivsu ivsu in response.Ivsus)
                    if (!versions.Contains(ivsu.Version)) versions.Add(ivsu.Version);
            return versions;
        }

        /// <summary>
        ///     Get special IVSU package such as Downgrade or Reformat from our API and passes it to ConvertIvsu
        /// </summary>
        /// <param name="specialPackage">SpecialPackage type e.g downgradetool, logtool34 etc.</param>
        public static async Task<SModel.Ivsu> GetSpecialIvsu(string specialPackage)
        {
            var graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<IvsuRoot>(GraphQlHelper.GetSpecialPackage(specialPackage));
            IvsuRoot ivsu = graphQlResponse.Data;
            return ConvertIvsu(ivsu.Ivsus?[0] ?? throw new InvalidOperationException());
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
        
        /// <summary>
        /// Gets generated UserAgent from API
        /// </summary>
        public static string GetGeneratedUserAgent()
        {
            try
            {
                Random rand = new();
                GraphQLResponse<UseragentRoot> graphQlResponse = Task.Run(async () => await AppMan.App.GraphQlClient.SendQueryAsync<UseragentRoot>(GraphQlHelper.GetUserAgents())).Result;
                UseragentRoot userAgents = graphQlResponse.Data;
                List<string> header = userAgents.UserAgents.Select(ua => ua.Useragent.Replace("[PLACEHOLDER]", rand.Next(ua.Min, ua.Max).ToString())).ToList();
                int index = rand.Next(header.Count);
                return header[index];
            }
            catch (Exception)
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:70.0) Gecko/20100101 Firefox/86.0";
            }
        }
    }
}