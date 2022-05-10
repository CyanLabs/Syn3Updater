using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Cyanlabs.UpdateLanguages
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Getting list of available languages, please wait...");
                if (!Directory.Exists("Languages")) Directory.CreateDirectory("Languages");
                JsonData.Root output;
                using (WebClient client = new())
                {
                    NameValueCollection reqparm = new();
                    reqparm.Add("api_token", "2dd6278944c92d9ba1b6e985c2a978ce"); //Readonly Don't worry!
                    reqparm.Add("id", "413809");
                    byte[] responsebytes = client.UploadValues("https://api.poeditor.com/v2/languages/list", "POST", reqparm);
                    string responsebody = Encoding.UTF8.GetString(responsebytes);
                    output = JsonConvert.DeserializeObject<JsonData.Root>(responsebody);
                }

                foreach (JsonData.Language language in output.Result.Languages)
                    using (WebClient client = new())
                    {
                        NameValueCollection reqparm = new();
                        reqparm.Add("api_token", "2dd6278944c92d9ba1b6e985c2a978ce");
                        reqparm.Add("id", "413809");
                        reqparm.Add("language", language.Code);
                        reqparm.Add("type", "key_value_json");
                        byte[] responsebytes = client.UploadValues("https://api.poeditor.com/v2/projects/export", "POST", reqparm);
                        string responsebody = Encoding.UTF8.GetString(responsebytes);
                        string url = JsonConvert.DeserializeObject<JsonData.Root>(responsebody).Result.Url;
                        client.DownloadFile(url, language.Code == "en" ? "Languages\\en-US.json" : $"Languages\\{language.Code}.json");
                        Console.WriteLine($"Downloaded {language.Name} ({language.Code})");
                    }
            }
            catch (Exception)
            {
                Console.WriteLine("");
                Console.WriteLine("Something went wrong, this is likely due to rate limits on POEditor, try again in a few minutes!");
                Console.ReadKey();
            }
        }
    }

    internal class JsonData
    {
        public class Response
        {
            [JsonProperty("status")] public string Status { get; set; }

            [JsonProperty("code")] public string Code { get; set; }

            [JsonProperty("message")] public string Message { get; set; }
        }

        public class Language
        {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("code")] public string Code { get; set; }

            [JsonProperty("translations")] public int Translations { get; set; }

            [JsonProperty("percentage")] public double Percentage { get; set; }

            [JsonProperty("updated")] public DateTime? Updated { get; set; }
        }

        public class Result
        {
            [JsonProperty("languages")] public List<Language> Languages { get; set; }

            [JsonProperty("url")] public string Url { get; set; }
        }

        public class Root
        {
            [JsonProperty("response")] public Response Response { get; set; }

            [JsonProperty("result")] public Result Result { get; set; }
        }
    }
}