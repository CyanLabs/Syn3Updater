namespace Syn3Updater.Tests.Helper
{
    using Cyanlabs.Syn3Updater.Helper;
    using System;
    using NUnit.Framework;

    [TestFixture]
    public static class SanityCheckHelperTests
    {
        [Test]
        public static void CanCallCancelDownloadCheck()
        {
            var selectedDrive = new USBHelper.Drive { Path = "TestValue866096061", Name = "TestValue539938438" };
            var allowDownloadonly = true;
            var result = SanityCheckHelper.CancelDownloadCheck(selectedDrive, allowDownloadonly);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallCancelDownloadCheckWithNullSelectedDrive()
        {
            Assert.Throws<ArgumentNullException>(() => SanityCheckHelper.CancelDownloadCheck(default, false));
        }
    }
}