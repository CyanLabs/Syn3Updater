using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Cyanlabs.Syn3Updater.Model
{
    public class LanguageModel
    {
        #region Constructor

        public LanguageModel(string path)
        {
            string contents = File.ReadAllText(path);
            Code = Path.GetFileName(path).Replace(".json", "");
            EnglishName = new CultureInfo(Code, false).DisplayName;
            NativeName = new CultureInfo(Code, false).NativeName;
            Items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in JObject.Parse(contents))
            {
                if (!string.IsNullOrEmpty(s.Value?.ToString()))
                {
                    Items.Add(s.Key, Regex.Replace(s.Value?.ToString(), @"\r\n|\n|\r", Environment.NewLine));
                }
                else
                {
                    Debug.WriteLine(s.Key + ":" + s.Value?.ToString());
                }
            }
        }

        #endregion

        #region Properties & Fields

        public string Code { get; set; }
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public string Emoji { get; set; }
        internal Dictionary<string, string> Items { get; set; }
        #endregion
    }

    public static class LanguageManager
    {
        #region Constructors

        static LanguageManager()
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles("Languages"))
                {
                    try
                    {
                        var lang = new LanguageModel(file);
                        Languages.Add(lang);
                        InternalLanguages.Add(lang.Code, lang);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Properties & Fields

        public static List<LanguageModel> Languages { get; set; } = new List<LanguageModel>();

        private static Dictionary<string, LanguageModel> InternalLanguages = new Dictionary<string, LanguageModel>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Methods

        public static string GetValue(string key, string lang)
        {
            LanguageModel l = null;
            // If this is not a 2 char culture (or doesn't exist in list) then...
            if (!InternalLanguages.TryGetValue(lang, out l))
            {
                //toss the 2 chars after the "-" 
                var culture = lang?.Split('-')[0];
                if (culture != null && !InternalLanguages.TryGetValue(culture, out l))
                {
                    //default to USA 
                    l = InternalLanguages["en-US"];
                }
            }
            string r = l.Items[key];

            if (string.IsNullOrWhiteSpace(r))
            {
                r = $"[{lang}:{key}]";
                Debug.WriteLine($"couldn't find {r}");
            }

            return r;
        }

        public static string GetValue(string key)
        {
            if (key == null)
                return null;

            string lang = string.Empty; // "EN-US";

            if (ApplicationManager.Instance.Settings.Lang != null)
                lang = ApplicationManager.Instance.Settings.Lang;

            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = CultureInfo.CurrentCulture.Name;

                ApplicationManager.Instance.Settings.Lang = lang;
            }

            LanguageModel l = (InternalLanguages[lang] ?? InternalLanguages[lang?.Split('-')[0]] ?? InternalLanguages["en-US"]);

            if (l == null)
            {
                //Have to hardcode path for design time :(
                string fn = $"E:\\Scott\\Documents\\GitHub\\Syn3Updater\\Syn3Updater\\Languages\\{lang}.json";

                if (File.Exists(fn))
                {
                    l = new LanguageModel(fn);
                    Languages.Add(l);
                }
            }

            if (l == null)
                return $"[{lang}:{key}]";

            // ReSharper disable once ConstantConditionalAccessQualifier
            string r = l.Items[key];

            if (string.IsNullOrWhiteSpace(r))
            {
                r = $"[{lang}:{key}]";
                Debug.WriteLine($"couldn't find {r}");
            }

            return r;

        }

        #endregion
    }
}