using NUnit.Framework;
using Cyanlabs.Updater.Services;
using Cyanlabs.Syn3Updater;
using System;
using System.Reflection;

namespace Syn3Updater.Tests.Common.Services
{  

    [TestFixture]
    public class DownloadViewModelServiceTests
    {
        //private string expectedString = $@"; CyanLabs Syn3Updater 2.x - Autoinstall Mode - Sync 3.3.19052 NA{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}Item1 = APPS - 4U5T-14G381-AN_1552583626000.TAR.GZ{Environment.NewLine}Open1 = SyncMyRide\4U5T-14G381-AN_1552583626000.TAR.GZ{Environment.NewLine}Options = AutoInstall{Environment.NewLine}";
        private string expectedString = $@"; CyanLabs Syn3Updater {Assembly.GetEntryAssembly()?.GetName().Version} {AppMan.App.LauncherPrefs.ReleaseTypeInstalled} - Autoinstall Mode - Sync 3.3.19052 NA{Environment.NewLine}{Environment.NewLine}[SYNCGen3.0_ALL_PRODUCT]{Environment.NewLine}Item1 = APPS - 4U5T-14G381-AN_1552583626000.TAR.GZ{Environment.NewLine}Open1 = SyncMyRide\4U5T-14G381-AN_1552583626000.TAR.GZ{Environment.NewLine}Options = AutoInstall{Environment.NewLine}";

        //called before each [Test] 
        [SetUp]
        public void SetUp()
        {
            //so we know that this list is clear before every run 
            AppMan.App.Ivsus.Clear();
            AppMan.App.SVersion = string.Empty;
        }
        //A real unit test(since this method is being tested in total isolation)! 
        [Test]
        public void ServiceGeneratesProperAutoInstallFile()
        {
            AppMan.App.SVersion = "3.3.19052";
            AppMan.App.Ivsus.Add(new Cyanlabs.Syn3Updater.Model.SModel.Ivsu
            {
                Selected = true,
                Type = "APPS",
                Name = "4U5T-14G381-AN",
                Version = "3.3.19052",
                Notes = "Some Notes", //convention to put random values for values that aren't relevant to the unit test 
                Url ="Some Url",
                Md5 ="Some hash",
                FileName = "4U5T-14G381-AN_1552583626000.TAR.GZ",
                FileSize = 502997338
            }); 
            var testedString= DownloadViewModelService.CreateAutoInstallFile("Sync 3.3.19052","NA");
            Assert.AreEqual(testedString.ToString(), expectedString);      
        }
    }
}