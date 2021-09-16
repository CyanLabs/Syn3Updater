using GraphQL;

namespace Cyanlabs.Syn3Updater.Helper
{
    public class GraphQlRequests
    {
        public static GraphQLRequest GetAppVersions()
        {
            return new GraphQLRequest
            {
                Query = @" 
                {
                    ivsu (sort: ""-name"",limit: -1,
                      filter: {type: { _eq: ""APPS"" }}) {
                        version
                    }
                }"
            };
        }
        
        public static GraphQLRequest IvsuVersionLookup(string ivsuVersion)
        {
            return new GraphQLRequest
            {
                Query = @" 
                {
                    ivsu(limit: 1, filter: {name: {_eq: """ + ivsuVersion + @"""}}) {
                        version
                    }
                }"
            };
        }
        
        public static GraphQLRequest GetReleases(string selectedRegion)
        {
            return new GraphQLRequest
            {
                Query = @"
                {
                    releases (
                        sort: ""-name"",
                        limit: -1,
                        filter: { 
                            status: { _in: [""published"", ""private""] },
                            key: { _in: [""public"", ""v2"", """ + AppMan.App.MainSettings.LicenseKey+ @"""]},
                            regions: {_contains: """ + selectedRegion + @"""},
                        }
                    ) {
                        name, notes, regions, version, feedbackurl
                    }
                }"
            };
        }
        
        public static GraphQLRequest GetMapReleases(string selectedRegion, string license, string esn, string compat)
        {
            return new GraphQLRequest 
            {
                Query = @"
                {
                    map_releases(sort: ""-name"", limit: -1,
                        filter: {_and: 
                            [{ _or: [ {licensekeys: { _null: true}}, {licensekeys: { _empty: true}}," + license + @"],
                            status: { _in: [""published"", ""private""] }, regions: {_in: """ + selectedRegion + @"""},
                            " + esn + @"compatibility: {_contains: """+ compat +@"""} }]
                        }){ name, regions, esn }
                }"
            };
        }
        public static GraphQLRequest GetReleaseIvsus(string selectedRelease, string navtype)
        {
            return new GraphQLRequest 
            {
                Query = @"
                {
                    releases(limit: -1, filter: {name: {_eq: """ + selectedRelease + @"""}}) {
                        name
                        ivsus {
                            ivsu(filter: {navtype: { _in: [""" + navtype + @""",""all""]}}) { 
                                id, name, type, version, notes, url, md5, filesize, regions}
                            }
                        }
                }"
            };
        }
        
        public static GraphQLRequest GetMapReleaseIvsus(string selectedMapVersion)
        {
            return new GraphQLRequest 
            {
                Query = @"
                {
                    map_releases(limit: -1, filter: {name: {_eq: """ + selectedMapVersion + @"""}}) {
                        name
                        ivsus {
                          map_ivsu { id, name, type, version, notes, url, md5, filesize, regions, source}
                        }
                    }
                }"
            };
        }
        
        public static GraphQLRequest GetNotices()
        {
            return new GraphQLRequest 
            {
                Query = @" 
                {
                    notices(limit: -1, filter: {enabled: {_eq: true}}) {
                        id, title, notice, date_created, date_updated, important
                    }
                }"
            };
        }
    }
}