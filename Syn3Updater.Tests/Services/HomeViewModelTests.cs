using NUnit.Framework;
using Cyanlabs.Syn3Updater;
using Cyanlabs.Updater.Common;
using Cyanlabs.Syn3Updater.Model;
using Cyanlabs.Syn3Updater.Helper;
using System.Threading.Tasks;

namespace Syn3Updater.Tests.Common.Services
{
    [TestFixture]
    public class HomeViewModelServiceTests
    {
        private System.Collections.ObjectModel.ObservableCollection<SModel.Ivsu> IvsuList;
        private SModel.Ivsu toTest;
        private SModel.SRegion merica;
        private USBHelper.Drive nosavedrive;

        [SetUp]
        public void SetUp()
        {
            toTest = new SModel.Ivsu
            {
                Selected = true,
                Type = "APPS",
                Name = "5U5T-14G381-AP",
                Version = "3.4.19101",
                Notes = null,
                Url = "https://ivsubinaries.azureedge.net/swparts/5U5T-14G381-AP_1555085337000.TAR.GZ",
                Md5 = "98c289f37416a07ba274585350acaa4c",
                FileName = "5U5T-14G381-AP_1555085337000.TAR.GZ",
                Source = null
            };

            IvsuList = new System.Collections.ObjectModel.ObservableCollection<SModel.Ivsu>() {
                new SModel.Ivsu
                {
                    Selected=false,
                    Type="GRACENOTES",
                    Name="4U5T-14G423-CA",
                    Version="1.1.1.1332",
                    Notes="",
                    Url="https://ivsubinaries.azureedge.net/swparts/4U5T-14G423-CA_123766.tar.gz",
                    Md5="95d8331ef437041ec4edd2163c1f2619",
                    FileName="4U5T-14G423-CA_123766.tar.gz",
                    Source=null
                },
                toTest,
                 new SModel.Ivsu
                {
                     Selected=false,
                     Type="VOICE",
                     Name="5U5T-14G391-CL",
                     Version=null,
                     Notes=null,
                     Url="https://ivsubinaries.azureedge.net/swparts/5U5T-14G391-CL_1580862907000.TAR.GZ",
                     Md5="672fc1d83f6a28fb1ec5ca713d23f050",
                     FileName="5U5T-14G391-CL_1580862907000.TAR.GZ",
                     Source=null
                 },
            };
            merica = new SModel.SRegion() { Code = "NA", Name = "United States, Canada & Mexico" };
            nosavedrive = new USBHelper.Drive() { Name = "Download Only (NO USB)" };
        }
        //Integration Test 
        [Test]
        public void WhenTheProperParametersArePassedADownloadIsInitiated()
        {

            //HomeViewModelService.Download();
            //TODO: Capture the Download invoke 
        }

        //"Unit" test
        [Test]
        public async Task WhenTheProperParametersArePassedTheStateVariablesAreSetProperly()
        {
            await HomeViewModelService.SetIvsuList("downgrade", IvsuList, merica, "Sync 3.4.19101", "Non Nav APIM", null);

            Assert.AreEqual(AppMan.App.Ivsus.Count, 4);
            Assert.AreEqual(AppMan.App.DriveLetter, null);
            Assert.AreEqual(AppMan.App.DownloadOnly, false);
            Assert.AreEqual(AppMan.App.SelectedRegion, merica.Code);
            Assert.AreEqual(AppMan.App.SelectedRelease, "Sync 3.4.19101");
            Assert.AreEqual(AppMan.App.SelectedMapVersion, "Non Nav APIM");
            //TODO: wire in deep equality checker 
            Assert.AreEqual(AppMan.App.Ivsus[3].Url, toTest.Url);
        }
    }
}