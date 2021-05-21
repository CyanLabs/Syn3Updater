using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;
using MessageBox = ModernWpf.MessageBox;

namespace Cyanlabs.Syn3Updater.UI.Tabs
{
    public static class ProfileModel
    {
        #region Properties & Fields
        public class Profile
        {
            public string Name { get; set; }
        }
        #endregion
    }
    internal class ProfilesViewModel : LanguageAwareBaseViewModel
    {
        #region Constructors
        private AsyncCommand<string> _selectProfile;
        public AsyncCommand<string> SelectProfile => _selectProfile ??= new AsyncCommand<string>(SelectProfileAction);
        
        private AsyncCommand<string> _deleteProfile;
        public AsyncCommand<string> DeleteProfile => _deleteProfile ??= new AsyncCommand<string>(DeleteProfileAction);   
        
        private AsyncCommand _createProfile;
        public AsyncCommand CreateProfile => _createProfile ??= new AsyncCommand(CreateProfileAction);
        #endregion

        #region Properties & Fields
        
        private string _currentProfile;
        public string CurrentProfile
        {
            get => _currentProfile;
            set => SetProperty(ref _currentProfile, value);
        }
        
        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set => SetProperty(ref _profileName, value);
        }
        
        private ObservableCollection<ProfileModel.Profile> _profileList;

        public ObservableCollection<ProfileModel.Profile> ProfileList
        {
            get => _profileList;
            set => SetProperty(ref _profileList, value);
        }
        
        private ProfileModel.Profile _selectedProfile;

        public ProfileModel.Profile SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }
     
        #endregion

        #region Methods

        public void Reload()
        {
            CurrentProfile = AppMan.App.MainSettings.Profile;
            Task.Run(ReloadProfiles);
        }

        private async Task DeleteProfileAction(string name)
        {
            if (name != AppMan.App.MainSettings.Profile)
            {
                try
                {
                    File.Delete(AppMan.App.ProfileConfigFolderPath + name + ".json");
                }
                catch
                {
                    // ignored
                }
                await ReloadProfiles();
            }
            else
            {
                MessageBox.Show(LM.GetValue("MessageBox.InvalidProfile"), "Syn3 Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task CreateProfileAction()
        {
            if (!string.IsNullOrEmpty(ProfileName))
            {
                string filename = FileHelper.MakeValidFileName(ProfileName);
                await SelectProfileAction(filename);
                await ReloadProfiles();
            }
        }
        
        private async Task SelectProfileAction(string name)
        {
            CurrentProfile = name;
            AppMan.App.MainSettings.Profile = name;
            AppMan.App.LoadProfile();
            AppMan.App.FireSettingsTabEvent();
        }

        private async Task ReloadProfiles()
        {
            ProfileList = new ObservableCollection<ProfileModel.Profile>();
            if (!string.IsNullOrEmpty(AppMan.App.ProfileConfigFolderPath))
            {
                DirectoryInfo dir = new(AppMan.App.ProfileConfigFolderPath);
                foreach (FileInfo file in dir.GetFiles("*.json"))
                {
                    ProfileList.Add(new ProfileModel.Profile() {Name = file.Name.Replace(".json","")});
                }
            }
        }

        #endregion
    }
}