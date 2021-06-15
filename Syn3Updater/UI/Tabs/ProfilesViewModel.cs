using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using Cyanlabs.Syn3Updater.Helper;
using Cyanlabs.Syn3Updater.Model;

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

        private AsyncValueCommand<string> _selectProfile;
        public AsyncValueCommand<string> SelectProfile => _selectProfile ??= new AsyncValueCommand<string>(SelectProfileAction);

        private AsyncCommand<string> _deleteProfile;
        public AsyncCommand<string> DeleteProfile => _deleteProfile ??= new AsyncCommand<string>(DeleteProfileAction);

        private ActionCommand _createProfile;
        public ActionCommand CreateProfile => _createProfile ??= new ActionCommand(CreateProfileAction);

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
            ReloadProfiles();
        }

        private async Task DeleteProfileAction(string name)
        {
            if (name != AppMan.App.MainSettings.Profile)
            {
                try
                {
                    File.Delete(AppMan.App.ProfilePath + name + ".json");
                }
                catch
                {
                    // ignored
                }

                ReloadProfiles();
            }
            else
            {
                await Application.Current.Dispatcher.BeginInvoke(() => UIHelper.ShowErrorDialog(LM.GetValue("MessageBox.InvalidProfile")).ShowAsync());
            }
        }

        private void CreateProfileAction()
        {
            if (!string.IsNullOrEmpty(ProfileName))
            {
                string filename = FileHelper.MakeValidFileName(ProfileName);
                SelectProfileAction(filename);
                ReloadProfiles();
            }
        }

        private ValueTask SelectProfileAction(string name)
        {
            CurrentProfile = name;
            AppMan.App.MainSettings.Profile = name;
            AppMan.App.LoadProfile();
            AppMan.App.SaveSettings();
            AppMan.App.FireSettingsTabEvent();
            return new ValueTask();
        }

        private void ReloadProfiles()
        {
            ProfileList = new ObservableCollection<ProfileModel.Profile>();
            if (!string.IsNullOrEmpty(AppMan.App.ProfilePath))
            {
                DirectoryInfo dir = new(AppMan.App.ProfilePath);
                foreach (FileInfo file in dir.GetFiles("*.json")) ProfileList.Add(new ProfileModel.Profile {Name = file.Name.Replace(".json", "")});
            }
        }

        #endregion
    }
}