using System.Collections.ObjectModel;
using System.Linq;

namespace Syn3Updater.UI.Tabs
{

    internal class SettingsViewModel : LanguageAwareBaseViewModel
    {
        public void Init()
        {
            SyncRegions = new ObservableCollection<SyncRegion>
            {
                new SyncRegion {Code = "EU", Name = "Europe"},
                new SyncRegion {Code = "NA", Name = "North America & Canada"},
                new SyncRegion {Code = "CN", Name = "China"},
                new SyncRegion {Code = "ANZ", Name = "Australia & New Zealand"},
                new SyncRegion {Code = "ROW", Name = "Rest Of World"}
            };

            CurrentSyncRegion = SyncRegions.FirstOrDefault(x => x.Code == Properties.Settings.Default.CurrentSyncRegion);

            OnPropertyChanged("SyncRegions");
            OnPropertyChanged("CurrentSyncRegion");
        }

        public ObservableCollection<SyncRegion> SyncRegions { get; set; }

        private SyncRegion currentSyncRegion;

        public SyncRegion CurrentSyncRegion
        {
            get => currentSyncRegion;
            set
            {
                if (value != null)
                {
                    SetProperty(ref currentSyncRegion, value);
                    Properties.Settings.Default.CurrentSyncRegion = value.Code;
                }
            }
        }
        public class SyncRegion
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
    }
}

