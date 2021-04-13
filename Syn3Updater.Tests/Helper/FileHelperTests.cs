using Cyanlabs.Syn3Updater.Helper;
using System;
using NUnit.Framework;
using System.Threading;
using Cyanlabs.Syn3Updater.Model;
using System.Threading.Tasks;

namespace Syn3Updater.Tests.Helper
{
    [TestFixture]
    [Ignore("FIXME")]
    public class FileHelperTests
    {
        private FileHelper _testClass;
        private EventHandler<EventArgs<int>> _externalPercentageChanged;

        [SetUp]
        public void SetUp()
        {
            _externalPercentageChanged = default;
            _testClass = new FileHelper(_externalPercentageChanged);
        }

        [Test]
        public void CanConstruct()
        {
            var instance = new FileHelper(_externalPercentageChanged);
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void CannotConstructWithNullExternalPercentageChanged()
        {
            Assert.Throws<ArgumentNullException>(() => new FileHelper(default));
        }

        [Test]
        public async Task CanCallCopyFileAsync()
        {
            var source = "TestValue1341751390";
            var destination = "TestValue442833009";
            var ct = CancellationToken.None;
            await _testClass.CopyFileAsync(source, destination, ct);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallCopyFileAsyncWithInvalidSource(string value)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.CopyFileAsync(value, "TestValue988473579", CancellationToken.None));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallCopyFileAsyncWithInvalidDestination(string value)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.CopyFileAsync("TestValue888829737", value, CancellationToken.None));
        }

        [Test]
        public async Task CanCallDownloadFile()
        {
            var path = "TestValue806668900";
            var filename = "TestValue1123419722";
            var ct = CancellationToken.None;
            await _testClass.DownloadFile(path, filename, ct);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallDownloadFileWithInvalidPath(string value)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadFile(value, "TestValue1826660125", CancellationToken.None));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallDownloadFileWithInvalidFilename(string value)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadFile("TestValue1989092351", value, CancellationToken.None));
        }

        [Test]
        public void CanCallValidateFile()
        {
            var source = "TestValue379678139";
            var localfile = "TestValue1507250646";
            var md5 = "TestValue1995825226";
            var localonly = true;
            var ct = CancellationToken.None;
            var result = _testClass.ValidateFile(source, localfile, md5, localonly, ct);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallValidateFileWithInvalidSource(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.ValidateFile(value, "TestValue1591156435", "TestValue1461748148", true, CancellationToken.None));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallValidateFileWithInvalidLocalfile(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.ValidateFile("TestValue1248780129", value, "TestValue1745235058", false, CancellationToken.None));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallValidateFileWithInvalidMd5(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.ValidateFile("TestValue466552766", "TestValue9818853", value, false, CancellationToken.None));
        }

        [Test]
        public void CanCallGenerateMd5()
        {
            var filename = "TestValue1138810608";
            var ct = CancellationToken.None;
            var result = _testClass.GenerateMd5(filename, ct);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallGenerateMd5WithInvalidFilename(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.GenerateMd5(value, CancellationToken.None));
        }

        [Test]
        public void CanCallurl_to_filename()
        {
            var url = "TestValue1520508193";
            var result = FileHelper.url_to_filename(url);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CannotCallurl_to_filenameWithInvalidUrl(string value)
        {
            Assert.Throws<ArgumentNullException>(() => FileHelper.url_to_filename(value));
        }

        [Test]
        public void CanCallExtractMultiPackage()
        {
            var item = new SModel.Ivsu { Selected = true, Type = "TestValue818008845", Name = "TestValue126881402", Version = "TestValue1525867736", Notes = "TestValue1513126080", Url = "TestValue918057671", Md5 = "TestValue165400975", FileName = "TestValue1848913370", Source = "TestValue1373573975", FileSize = 1375353975 };
            var ct = CancellationToken.None;
            var result = _testClass.ExtractMultiPackage(item, ct);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public void CannotCallExtractMultiPackageWithNullItem()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.ExtractMultiPackage(default, CancellationToken.None));
        }
    }
}