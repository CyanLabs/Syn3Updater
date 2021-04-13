using Cyanlabs.Syn3Updater.Helper;
using System;
using NUnit.Framework;

namespace Syn3Updater.Tests.Helper
{
    [TestFixture]
    [Ignore("FIXME")]
    public static class USBHelperTests
    {
        [Test]
        public static void CanCallRefreshDevices()
        {
            var fakeusb = true;
            var result = USBHelper.RefreshDevices(fakeusb);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallUpdateDriveInfo()
        {
            var selectedDrive = new USBHelper.Drive { Path = "TestValue924467263", Name = "TestValue1764848851" };
            var result = USBHelper.UpdateDriveInfo(selectedDrive);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallUpdateDriveInfoWithNullSelectedDrive()
        {
            Assert.Throws<ArgumentNullException>(() => USBHelper.UpdateDriveInfo(default));
        }


        // TODO: make test smarter 
        [Test]
        public static void CanCallGenerateLog()
        {
            var log = "TestValue202940782";
            USBHelper.GenerateLog(log, false);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public static void CannotCallGenerateLogWithInvalidLog(string value)
        {
            Assert.Throws<ArgumentNullException>(() => USBHelper.GenerateLog(value, false));
        }

        //TODO: make test smarter , need moq 
        [Test]
        public static void CanCallUploadLog()
        {
            var log = "TestValue70820892";
            // USBHelper.UploadLog(log);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public static void CannotCallUploadLogWithInvalidLog(string value)
        {
            // Assert.Throws<ArgumentNullException>(() => USBHelper.UploadLog(value));
        }
    }
}