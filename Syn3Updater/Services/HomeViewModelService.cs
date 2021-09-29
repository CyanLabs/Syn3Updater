using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Syn3Updater.Helpers;
using Syn3Updater.Models;

namespace Syn3Updater.Services
{
    public static class HomeViewModelService
    {
        public static async Task<ObservableCollection<SModel.Ivsu>> GetReleaseIvsus(string selectedRegion, string selectedRelease, string selectedMapRelease, bool navigation)
        {
            ObservableCollection<SModel.Ivsu> ivsuList = new();
            string navtype = navigation ? "nav" : "nonnav";
            
            GraphQLResponse<ReleasesRoot> graphQlResponse = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetReleaseIvsus(selectedRelease,navtype));
            ReleasesRoot jsonIvsUs = graphQlResponse.Data;
            foreach (ReleasesIvsus item in jsonIvsUs.Releases[0].IvsusList.Where(x => x.Ivsu != null && (x.Ivsu.Regions.Contains("ALL") || x.Ivsu.Regions.Contains(selectedRegion))))
            {
                if (item.Ivsu != null)
                {
                    ivsuList.Add(new SModel.Ivsu
                    {
                        Type = item.Ivsu.Type,
                        Name = item.Ivsu.Name,
                        Version = item.Ivsu.Version,
                        Notes = item.Ivsu.Notes,
                        Url = item.Ivsu.Url,
                        Md5 = item.Ivsu.Md5,
                        Selected = true,
                        FileName = FileHelper.url_to_filename(item.Ivsu.Url),
                        FileSize = item.Ivsu.FileSize
                    });
                }
                
            }
            
            GraphQLResponse<ReleasesRoot> graphQlResponse2 = await AppMan.App.GraphQlClient.SendQueryAsync<ReleasesRoot>(GraphQlHelper.GetMapReleaseIvsus(selectedMapRelease));
            ReleasesRoot jsonMapIvsUs = graphQlResponse2.Data;
            
            AppMan.App.AutoInstall = jsonMapIvsUs.MapReleases[0].Autoinstall;

            foreach (ReleasesIvsus item in jsonMapIvsUs.MapReleases[0].IvsusList.Where(x => x.MapIvsu != null && (x.MapIvsu.Regions.Contains("ALL") || x.MapIvsu.Regions.Contains(selectedRegion))))
            {
                if (item.MapIvsu != null)
                {
                    ivsuList.Add(new SModel.Ivsu
                    {
                        Type = item.MapIvsu.Type,
                        Name = item.MapIvsu.Name,
                        Version = item.MapIvsu.Version,
                        Notes = item.MapIvsu.Notes,
                        Url = item.MapIvsu.Url,
                        Md5 = item.MapIvsu.Md5,
                        Selected = true,
                        FileName = FileHelper.url_to_filename(item.MapIvsu.Url),
                        FileSize = item.MapIvsu.FileSize,
                        Source = item.MapIvsu.Source
                    });
                }
            }
            
            AppMan.App.SelectedRegion = selectedRegion;
            AppMan.App.SelectedRelease = selectedRelease;
            AppMan.App.SelectedMapVersion = selectedMapRelease;
            return ivsuList;
        }
    }
}