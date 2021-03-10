using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
            var dict = JObject.Parse(contents);
            Items = new List<LanguageItem>();
            foreach (var s in dict)
            {
                if (s.Value?.ToString() != "")
                    Items.Add(new LanguageItem
                    {
                        Key = s.Key,
                        Value = s.Value?.ToString()
                    });
                else
                    Debug.WriteLine(s.Key + ":" + s.Value?.ToString());
            }
        }

        #endregion

        #region Properties & Fields

        public string Code { get; set; }
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public string Emoji { get; set; }
        public List<LanguageItem> Items { get; set; }

        public class LanguageItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        #endregion
    }

    public static class LanguageManager
    {
        #region Constructors

        static LanguageManager()
        {
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles("Languages");

                foreach (string file in files)
                    try
                    {
                        Languages.Add(new LanguageModel(file));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
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

        #endregion

        #region Methods

        public static string GetValue(string key, string lang)
        {
            try
            {
                LanguageModel l = (Languages.Find(x => string.Equals(x.Code, lang, StringComparison.OrdinalIgnoreCase))
                    ?? Languages.Find(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-')[0])))
                    ?? Languages.Find(x => x.Code.StartsWith("EN", StringComparison.OrdinalIgnoreCase));

                if (l == null)
                    return $"[{lang}:{key}]";

                string r = l.Items.Find(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))?.Value.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine)
                    .Replace("\\r", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = $"[{lang}:{key}]";
                    Debug.WriteLine($"couldn't find {r}");
                }

                return r;
            }
            catch
            {
                // ignored
            }

            return $"{lang}:::{key}";
        }

        public static string GetValue(string key)
        {
            if (key == null) return null;

            try
            {
                string lang = string.Empty; // "EN-US";

                if (ApplicationManager.Instance.Settings.Lang != null) lang = ApplicationManager.Instance.Settings.Lang;

                if (string.IsNullOrWhiteSpace(lang))
                {
                    lang = CultureInfo.CurrentCulture.Name;

                    ApplicationManager.Instance.Settings.Lang = lang;
                }

                LanguageModel l = (Languages.Find(x => string.Equals(x.Code, lang, StringComparison.OrdinalIgnoreCase))
                    ?? Languages.Find(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-')[0])))
                    ?? Languages.Find(x => x.Code.StartsWith("EN", StringComparison.OrdinalIgnoreCase));

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

                if (l == null) return $"[{lang}:{key}]";

                // ReSharper disable once ConstantConditionalAccessQualifier
                string r = l.Items.Find(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))?.Value.Replace("\\n", Environment.NewLine).Replace("\\r", Environment.NewLine)
                    .Replace("\\r\\n", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = $"[{lang}:{key}]";
                    Debug.WriteLine($"couldn't find {r}");
                }

                return r;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion
    }
}