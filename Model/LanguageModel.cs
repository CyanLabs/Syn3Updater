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

        public LanguageModel(string path)
        {
            string contents = File.ReadAllText(path);
            List<string> lines = contents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Select(x => x.Trim()).ToList();

            Code = lines[0];
            EnglishName = lines[2];
            NativeName = lines[1];
            //this.Emoji = lines[3];

            Items = new List<LanguageItem>();
            foreach (string s in lines.Skip(3))
            {
                string[] parts = s.Replace("  ", "\t").Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1 && parts[0].Contains(" "))
                {
                    parts = new string[2];

                    parts[0] = s.Substring(0, s.IndexOf(" ", StringComparison.Ordinal)).Trim();
                    parts[1] = s.Substring(s.IndexOf(" ", StringComparison.Ordinal)).Trim();
                }

                if (parts.Length > 1)
                {
                    Items.Add(new LanguageItem
                    {
                        Key = parts[0],
                        Value = parts[1]
                    });
                }
                else
                {
                    Debug.WriteLine(parts);
                }
            }
        }
    }

    public static class LanguageManager
    {
        public static List<LanguageModel> Languages { get; set; } = new List<LanguageModel>();
        static LanguageManager()
        {
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles("Languages");

                foreach (string file in files)
                {
                    try
                    {
                        Languages.Add(new LanguageModel(file));
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

        public static string GetValue(string key, string lang)
        {
            try
            {
                LanguageModel l = Languages.FirstOrDefault(x => x.Code.ToUpper() == lang.ToUpper());
                if (l == null)
                {
                    l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-').First()));
                }

                if (l == null)
                {
                    return "[" + lang + ":" + key + "]";
                }

                Debug.WriteLine("Looking for " + key + " in " + l.Code);
                string r = l.Items.FirstOrDefault(x => x.Key.ToLower() == key.ToLower())?.Value.Replace("\\n",Environment.NewLine).Replace("\\r",Environment.NewLine).Replace("\\r\\n",Environment.NewLine);
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = "[" + lang + ":" + key + "]";
                    Debug.WriteLine("couldnt find " + r);
                }

                return r;
            }
            catch
            {
                // ignored
            }

            return lang + ":::" + key;
        }

        public static string GetValue(string key)
        {
            if (key == null)
            {
                return null;
            }

            string dbg = "";
            try
            {
                string lang = string.Empty;// "EN-US";

                //if (ApplicationManager.Instance != null)
                //{
                if (Settings.Default.Lang != null)
                {
                    lang = Settings.Default.Lang;
                }
                //}

                if (string.IsNullOrWhiteSpace(lang))
                {
                    lang = CultureInfo.CurrentCulture.Name;

                    Settings.Default.Lang = lang;
                }

                LanguageModel l = Languages.FirstOrDefault(x => x.Code.ToUpper() == lang.ToUpper());
                if (l == null)
                {
                    l = Languages.FirstOrDefault(x => x.Code.ToUpper().StartsWith(lang.ToUpper().Split('-').First()));
                }

                if (l == null)
                {
                    dbg += " lang null, loading...";
                    //Have to hardcode path for design time :(
                    string fn = "E:\\Scott\\Documents\\GitHub\\Syn3Updater\\Languages" + lang + ".txt";

                    //return fn;

                    dbg = dbg + "\r\nloading " + fn;


                    if (File.Exists(fn))
                    {

                        l = new LanguageModel(fn);
                        Languages.Add(l);
                        dbg += "\r\nLoaded";
                    }
                }

                if (l == null)
                {
                    return "[" + lang + ":" + key + "]";
                }

                Debug.WriteLine("Looking for " + key + " in " + l?.Code);
                string r = l.Items.FirstOrDefault(x => x.Key.ToLower() == key.ToLower())?.Value.Replace("\\n", Environment.NewLine).Replace("\\r", Environment.NewLine).Replace("\\r\\n", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(r))
                {
                    r = "[" + lang + ":" + key + "]";
                    Debug.WriteLine("couldnt find " + r);
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