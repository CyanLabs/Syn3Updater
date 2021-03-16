namespace Syn3Updater.Tests.Helper
{
    using Cyanlabs.Syn3Updater.Helper;
    using System;
    using NUnit.Framework;

    [TestFixture]
    public static class SystemHelperTests
    {
        [Test]
        public static void CanCallGetPathWithKnownFolder()
        {
            var knownFolder = SystemHelper.KnownFolder.Downloads;
            var result = SystemHelper.GetPath(knownFolder);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallGetPathWithKnownFolderAndDefaultUser()
        {
            var knownFolder = SystemHelper.KnownFolder.Downloads;
            var defaultUser = true;
            var result = SystemHelper.GetPath(knownFolder, defaultUser);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallGetOsFriendlyName()
        {
            var result = SystemHelper.GetOsFriendlyName();
            Assert.Fail("Create or modify test");
        }
    }
}