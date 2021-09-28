using System;
using System.IO;
using Newtonsoft.Json;
using Syn3Updater.Models;

namespace Syn3Updater.Helpers
{
    public static class SettingsHelper
    {
        
        
        public static void SaveMainSettings(string mainConfigFile, MainSettings mainSettings)
        {
            try
            {
                string mainJson = JsonConvert.SerializeObject(mainSettings, Formatting.Indented);
                File.WriteAllText(mainConfigFile, mainJson);
            }
            catch (IOException)
            {
                // ignored
            }
        }
        
        public static void SaveProfileSettings(string profileFile, ProfileSettings settings)
        {
            try
            {
                string profileJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(profileFile, profileJson);
            }
            catch (IOException)
            {
                // ignored
            }
        }

        public static void ResetSettings(string mainConfigFile, string ProfileFile)
        {
            try
            {
                if (File.Exists(mainConfigFile)) File.Delete(mainConfigFile);
                if (File.Exists(ProfileFile)) File.Delete(ProfileFile);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}