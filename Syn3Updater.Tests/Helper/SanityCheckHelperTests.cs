using Cyanlabs.Syn3Updater.Helper;
using System;
using NUnit.Framework;

namespace Syn3Updater.Tests.Helper
{
    [TestFixture]
    [Ignore("FIXME")]
    public static class SanityCheckHelperTests
    {
        [Test]
        public static void CanCallCancelDownloadCheck()
        {
            var selectedDrive = new USBHelper.Drive { Path = "TestValue866096061", Name = "TestValue539938438" };
            var result = SanityCheckHelper.CancelDownloadCheck(selectedDrive);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallCancelDownloadCheckWithNullSelectedDrive()
        {
            Assert.Throws<ArgumentNullException>(() => SanityCheckHelper.CancelDownloadCheck(default));
        }
    }
}