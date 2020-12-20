using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Syn3Updater.Properties;

namespace Syn3Updater.Model
{
    public class LanguageModel
    {
        public LanguageModel(string path)
        {
            var contents = File.ReadAllText(path);
            List<string> lines = contents.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)
                .Select(x => x.Trim()).ToList();

            Code = lines[0];
            EnglishName = lines[2];
            NativeName = lines[1];
            //this.Emoji = lines[3];

            Items = new List<LanguageItem>();
            foreach (var s in lines.Skip(3))
            {
                string[] parts = s.Replace("  ", "\t").Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1 && parts[0].Contains(" "))
                {
                    parts = new string[2];

                    parts[0] = s.Substring(0, s.IndexOf(" ", StringComparison.Ordinal)).Trim();
                    parts[1] = s.Substring(s.IndexOf(" ", StringComparison.Ordinal)).Trim();
                }

                if (parts.Length > 1)
                    Items.Add(new LanguageItem
                    {
                        Key = parts[0],
                        Value = parts[1]
                    });
                else
                    Debug.WriteLine(parts);
            }
        }

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
    }

    public static class LanguageManager
    {
        static LanguageManager()
        {
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles("Languages");

                foreach (var file in files)
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

        public static List<LanguageModel> Languages { get; set; } = new List<LanguageModel>();

        public static string GetValue(string key, string lang)
        {
            try
            {
                LanguageModel l = Languages.FirstOrDefault(x => x.Code.ToUpper() == lang.ToUpper());
                if (l == null)
                    l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-').First()));

                if (l == null) l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith("EN"));

                if (l == null) return $"[{lang}:{key}]";

                Debug.WriteLine($"Looking for {key} in {l.Code}");
                var r = l.Items.FirstOrDefault(x => x.Key.ToLower() == key.ToLower())?.Value
                    .Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine)
                    .Replace("\\r", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = $"[{lang}:{key}]";
                    Debug.WriteLine($"couldnt find {r}");
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
                var lang = string.Empty; // "EN-US";

                if (Settings.Default.Lang != null) lang = Settings.Default.Lang;

                if (string.IsNullOrWhiteSpace(lang))
                {
                    lang = CultureInfo.CurrentCulture.Name;

                    Settings.Default.Lang = lang;
                }

                LanguageModel l = Languages.FirstOrDefault(x => x.Code.ToUpper() == lang.ToUpper());
                if (l == null)
                    l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-').First()));

                if (l == null) l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith("EN"));

                if (l == null)
                {
                    //Have to hardcode path for design time :(
                    var fn = $"E:\\Scott\\Documents\\GitHub\\Syn3Updater\\Languages{lang}.txt";

                    if (File.Exists(fn))
                    {
                        l = new LanguageModel(fn);
                        Languages.Add(l);
                    }
                }

                if (l == null) return $"[{lang}:{key}]";

                // ReSharper disable once ConstantConditionalAccessQualifier
                Debug.WriteLine($"Looking for {key} in {l?.Code}");
                var r = l.Items.FirstOrDefault(x => x.Key.ToLower() == key.ToLower())?.Value
                    .Replace("\\n", Environment.NewLine).Replace("\\r", Environment.NewLine)
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
    }
}